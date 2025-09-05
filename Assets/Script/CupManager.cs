using System;
using System.Buffers.Binary;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CupManager : MonoBehaviour
{
    enum DATA { DATA_UNACTABLE, DATA_SKIPTURN };
    private int[] FloorFirstIndex = { 0, 8, 15, 21, 26, 30, 33, 35, 36 };
    private const int MAXCUPINDEX = 36;
    private const int MAXUSABLEINDEX = 11;
    private const int GAP = 55;
    private const int BLANK = 5;

    public SoundBox m_SoundBox;
    public TextManager m_TextManager;
    public TextMeshProUGUI m_EndText;
    public SpriteAtlas Atlas;
    public Button[] Cups;
    public Button[] UsableCups;

    public CSocket m_Socket;
    private PACKET m_SendPacket = new PACKET();
    private PACKET m_RecvPacket = new PACKET();
    static ManualResetEvent PauseEvent = new ManualResetEvent(true);

    public string ChangeImageIndex = "2";

    private bool m_Toggle = false;
    private int m_Remain = MAXUSABLEINDEX + 1;
    private int[] m_Remains = new int[3];
    private int[] m_Actables = new int[3];
    private int m_SelectIndex = -1; // 하단 버튼 선택
    private bool m_StopLoop = false;
    private bool m_Myturn = false;
    private object m_ToggleLock;// = new object();

    void Start()
    {
        m_ToggleLock = new object();
        m_SendPacket.Data = new byte[512];
        m_RecvPacket.Data = new byte[512];

        Thread RecvThread = new Thread(() => RecvMessage());
        RecvThread.IsBackground = true;
        RecvThread.Start();

        for (int i = 0; i < FloorFirstIndex[1]; ++i)
        {
            Cups[i].interactable = true;
        }

        Vector2 Target = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(Target.x + GAP * i, Target.y);
        }

        m_Actables[0] = m_Actables[1] = m_Actables[2] = FloorFirstIndex[1];
        m_TextManager.SetRoundText(1);
        m_TextManager.Set_MyPlayer(m_Socket.GetUserNum());
        if (m_Socket.GetUserNum() == 0)
        {
            Debug.Log("Start on my turn");
            m_Myturn = true;
            m_TextManager.Set_TimerStop(false);
        }
        else
        {
            m_Myturn = false;
            m_TextManager.Set_TimerStop(true);
        }
    }

    void Update()
    {
        if (true == m_Myturn && true == m_TextManager.Get_TimerStop())
        {
            Debug.Log("TimeOut!");
            SkipTurn(true);
            m_TextManager.TurnTextOff();
        }

        if (true == m_Toggle)
        {
            switch (m_RecvPacket.Type)
            {
                case DATATYPE.DATATYPE_GAME:
                    {
                        StackCup();
                        m_TextManager.TurnTextOff();
                        break;
                    }
                case DATATYPE.DATATYPE_TURN:
                    {
                        m_TextManager.StartTurn();
                        break;
                    }
                case DATATYPE.DATATYPE_USERINFO:
                    {
                        TableSetting();
                        break;
                    }
                case DATATYPE.DATATYPE_ENDGAME:
                    {
                        int winner = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
                        if (m_Socket.GetUserNum() == winner)
                        {
                            m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_WIN);
                            Debug.Log("You Are Winner!");
                            m_TextManager.EndText(true, true);
                        }
                        else
                        {
                            m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_LOSE);
                            Debug.Log("You Are Loser!");
                            m_TextManager.EndText(false, true);
                        }
                        break;
                    }
                case DATATYPE.DATATYPE_GAMESET:
                    {
                        int winner = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
                        if (m_Socket.GetUserNum() == winner)
                        {
                            m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_WIN);
                            Debug.Log($"You Win! Game Set!");
                            m_TextManager.EndText(true, false);
                        }
                        else
                        {
                            m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_LOSE);
                            Debug.Log($"You Lose! Game Set!");
                            m_TextManager.EndText(false, false);
                        }
                        break;
                    }
            }
            m_Toggle = false;
            lock (m_ToggleLock)
            {
                PauseEvent.Set();
            }
        }
    }

    public void GameAct()
    {
        if (false == m_Myturn)
            return;

        if (m_SelectIndex == -1)
            return;

        GameObject ClickedButton = EventSystem.current.currentSelectedGameObject;

        int StackIndex = -1;
        Button Temp = ClickedButton.GetComponent<Button>();
        for (int i = 0; i < MAXCUPINDEX; ++i)
        {
            if (Temp == Cups[i])
            {
                StackIndex = i;
            }
        }
        if (StackIndex == -1)
            Debug.Log("StackIndex Error!");

        if (false == UseCup(StackIndex))
            return;

        //Usablecups 원소들의 인덱스가 바뀌어버린다.
        m_SelectIndex = -1;
        m_Myturn = false;
    }

    public void SelectBol()
    {
        if (false == m_Myturn || m_TextManager.Get_TimerStop())
        {
            m_SelectIndex = -1;
            return;
        }

        m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_SELECT);

        //선택한 버튼의 인덱스를 저장하기
        GameObject selectButton = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < m_Remain; ++i)
        {
            if (UsableCups[i].gameObject == selectButton)
            {
                m_SelectIndex = i;
                ChangeImageIndex = selectButton.GetComponent<Image>().sprite.name;
                break;
            }
        }
    }

    public bool UseCup(int StackIndex)
    {
        //선택하고 있었던 버튼을 뒤로 보내고 디폴트 이미지로 전환
        if (m_Remain <= 0)
        {
            Debug.Log("UseBol Failed : No Remaining!");
            return false;
        }

        int FloorIndex = StackIndex;
        int Floor = CheckFloor(ref FloorIndex);

        if (StackIndex >= FloorFirstIndex[1]) // 0층이 아니라면 입력 유효성 체크
        {
            string CheckSelect = UsableCups[m_SelectIndex].GetComponent<Image>().sprite.name[0].ToString(); // 선택위치
            string CheckLeft = Cups[FloorFirstIndex[Floor - 1] + FloorIndex].GetComponent<StackCup>().GetName(); // 좌하단
            string CheckRight = Cups[FloorFirstIndex[Floor - 1] + FloorIndex + 1].GetComponent<StackCup>().GetName(); // 우하단

            if (CheckLeft != CheckSelect && CheckRight != CheckSelect)
            {
                m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_ERROR);
                return false;
            }
        }

        m_SendPacket.Type = DATATYPE.DATATYPE_GAME;
        m_SendPacket.DataSize = CSocket.DATASIZE_GAMEACT;
        int spriteName = UsableCups[m_SelectIndex].GetComponent<Image>().sprite.name[0] - '0';
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(0, 4), StackIndex);
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(4, 4), spriteName);
        m_Socket.SendMessage(m_SendPacket);

        UsableCups[m_SelectIndex].GetComponent<Image>().sprite = Atlas.GetSprite($"{BLANK}");

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = m_SelectIndex + 1; i <= MAXUSABLEINDEX; ++i)
        {
            (UsableCups[i], UsableCups[i - 1]) = (UsableCups[i - 1], UsableCups[i]);
        }

        for (int i = m_SelectIndex; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }

        --m_Remains[spriteName];
        --m_Remain;

        return true;
    }

    private int CheckFloor(ref int Index) // 인덱스로 층 체크하기
    {
        if (Index == MAXCUPINDEX)
        {
            Index = 0;
            return 7;
        }
        for (int i = 8; i >= 1; --i)
        {
            if (Index < i)
            {
                return 8 - i;
            }
            Index -= i;
        }

        Debug.Log("FloorCheck Error!");
        return -1;
    }

    private void RecvMessage()
    {
        while (m_StopLoop == false)
        {
            Debug.Log("==================================");
            m_Socket.RecvMessage(ref m_RecvPacket);
            switch (m_RecvPacket.Type)
            {
                case DATATYPE.DATATYPE_DEBUG:
                    {
                        string Message = Encoding.UTF8.GetString(m_RecvPacket.Data, 0, m_RecvPacket.DataSize);
                        Debug.Log(Message);
                        break;
                    }
                case DATATYPE.DATATYPE_GAME:
                    {
                        lock (m_ToggleLock)
                        {
                            m_Toggle = true;
                            PauseEvent.Reset(); // set 이전에 reset 실행이 보장돼야 함.
                        }
                        PauseEvent.WaitOne(); // 쓰레드 멈춤
                        break;
                    }
                case DATATYPE.DATATYPE_TURN:
                    {
                        int TurnPlayer = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
                        m_TextManager.Set_PlayerTurn(TurnPlayer);

                        if (m_Socket.GetUserNum() == TurnPlayer) // 내 차례면
                        {
                            Debug.Log("AbleCheck");
                            if (false == ActableCheck())
                            {
                                Debug.Log("Unable to Act");
                                SkipTurn(false);
                                break;
                            }

                            lock (m_ToggleLock)
                            {
                                Debug.Log("Myturn Start");
                                m_Myturn = true;
                                m_TextManager.Set_TimerStop(false);
                                m_Toggle = true;
                                PauseEvent.Reset();
                            }
                            PauseEvent.WaitOne();

                            //update -> Textmanager.StartTurn;
                        }
                        else // 내차례가 아님
                        {
                            m_TextManager.ResetTimer();
                            m_Myturn = false;
                        }
                        break;
                    }
                case DATATYPE.DATATYPE_ENDGAME:
                    {
                        Debug.Log("EndGame!");
                        m_Toggle = true;
                        m_StopLoop = true; // 쓰레드 끌거라 쓰레드 멈출 필요 없음

                        m_SendPacket.Type = DATATYPE.DATATYPE_ENDGAME;
                        m_SendPacket.DataSize = CSocket.DATASIZE_NODATA;
                        m_Socket.SendMessage(m_SendPacket);
                        m_Socket.Shutdown();
                        m_Socket.Close();
                        return;
                    }
                case DATATYPE.DATATYPE_GAMESET:
                    {
                        lock (m_ToggleLock)
                        {
                            Debug.Log("GameSet!");
                            m_Toggle = true;
                            PauseEvent.Reset();
                        }
                        PauseEvent.WaitOne();
                        break;
                    }
                case DATATYPE.DATATYPE_USERINFO:
                    {
                        lock (m_ToggleLock)
                        {
                            Debug.Log("TableSet!");
                            m_Toggle = true;
                            PauseEvent.Reset();
                        }
                        PauseEvent.WaitOne(); // 쓰레드 멈춤
                        break;
                    }
                default:
                    {
                        Debug.Log("DATATYPE Error!");
                        return;
                    }
            }
        }
        return;
    }

    private void StackCup()
    {
        m_TextManager.add_Score();
        m_SoundBox.PlaySound((int)SoundBox.SOUND.SOUND_USE);

        int CupPos = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
        int GameAct = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data.AsSpan(4, 4));
        //Debug.Log($"\nCupPos : {CupPos}\tGameAct : {GameAct}");

        Button Cup = Cups[CupPos];
        Image ThisImage = Cup.GetComponent<Image>();
        ThisImage.sprite = Atlas.GetSprite(GameAct.ToString());
        Cups[CupPos].GetComponent<StackCup>().SetName(GameAct.ToString());

        int Originalindex = CupPos;
        int Floor = CheckFloor(ref CupPos);

        //CupPos는 해당 층에서의 인덱스로 바뀜
        if (Floor == 7) // 마지막 칸 넣었으면 게임 끝난거지
        {
            m_Remain = m_Actables[0] = m_Actables[1] = m_Actables[2] = 0;
            return;
        }

        if (Originalindex < FloorFirstIndex[1]) // 0층이면
        {
            --m_Actables[0];
            --m_Actables[1];
            --m_Actables[2];
        }
        else // 1층부터는 아래 쌓인 카드들 체크
        {
            string CheckLeft = Cups[FloorFirstIndex[Floor - 1] + CupPos].GetComponent<StackCup>().GetName(); // 좌하단
            string CheckRight = Cups[FloorFirstIndex[Floor - 1] + CupPos + 1].GetComponent<StackCup>().GetName(); // 우하단

            --m_Actables[CheckLeft[0] - '0'];
            --m_Actables[CheckRight[0] - '0'];
            if (CheckLeft[0] == CheckRight[0])
            {
                ++m_Actables[CheckLeft[0] - '0'];
            }
        }

        string CurrName = Cups[Originalindex].GetComponent<Image>().sprite.name;

        if (CupPos != 0) // 좌측체크
        {
            //Debug.Log($"LeftName {LeftName}, RightName {CurrName}");
            string LeftName = Cups[Originalindex - 1].GetComponent<Image>().sprite.name; // 좌상단
            if (LeftName[0] != '5')
            {
                Debug.Log($"Left {LeftName[0]}");
                int NextIndex = Originalindex + 7 - Floor;
                ++m_Actables[LeftName[0] - '0'];
                ++m_Actables[CurrName[0] - '0'];
                if (LeftName[0] == CurrName[0])
                {
                    --m_Actables[CurrName[0] - '0'];
                }
                Cups[NextIndex].interactable = true;
            }
        }

        if (CupPos != 7 - Floor) //우측
        {
            string RightName = Cups[Originalindex + 1].GetComponent<Image>().sprite.name; // 우상단
            Debug.Log($"Right {RightName[0]}");
            if (RightName[0] != '5')
            {
                int NextIndex = Originalindex + (7 - Floor) + 1;
                ++m_Actables[RightName[0] - '0'];
                ++m_Actables[CurrName[0] - '0'];
                if (RightName[0] == CurrName[0])
                {
                    --m_Actables[CurrName[0] - '0'];
                }
                Cups[NextIndex].interactable = true;
            }
        }

        Debug.Log($"\nActables      -> 0 : {m_Actables[0]}, 1 : {m_Actables[1]}, 2 : {m_Actables[2]}");
        Debug.Log($"\nRemain Usable -> 0  : {m_Remains[0]}, 1 : {m_Remains[1]}, 2 : {m_Remains[2]}");
    }

    public void SkipTurn(bool actable)
    {
        m_SendPacket.Type = DATATYPE.DATATYPE_TURN;
        m_SendPacket.DataSize = sizeof(int);
        if (true == actable)
        {
            BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data, (int)DATA.DATA_SKIPTURN);
        }
        else
        {
            BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data, (int)DATA.DATA_UNACTABLE);
        }
        m_Socket.SendMessage(m_SendPacket);

        m_TextManager.ResetTimer();
        m_Myturn = false;
    }

    private void TableSetting()
    {
        m_Remains[0] = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
        m_Remains[1] = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data.AsSpan(4, 4));
        m_Remains[2] = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data.AsSpan(8, 4));
        Debug.Log($"0 : {m_Remains[0]}, 1 : {m_Remains[1]}, 2  : {m_Remains[2]}");

        for (int i = 0; i < m_Remains[0]; ++i)
        {
            UsableCups[i].GetComponent<Image>().sprite = Atlas.GetSprite("0");
        }
        for (int i = 0; i < m_Remains[1]; ++i)
        {
            UsableCups[i + m_Remains[0]].GetComponent<Image>().sprite = Atlas.GetSprite("1");
        }
        for (int i = 0; i < m_Remains[2]; ++i)
        {
            UsableCups[i + m_Remains[0] + m_Remains[1]].GetComponent<Image>().sprite = Atlas.GetSprite("2");
        }


        for (int i = 0; i < MAXCUPINDEX; ++i)
        {
            Cups[i].GetComponent<Image>().sprite = Atlas.GetSprite($"{BLANK}");
            Cups[i].GetComponent<StackCup>().SetName($"{BLANK}");
            Cups[i].interactable = false;
        }

        for (int i = 0; i < FloorFirstIndex[1]; ++i)
        {
            Cups[i].interactable = true;
        }

        m_Remain = MAXUSABLEINDEX + 1;
        m_Actables[0] = m_Actables[1] = m_Actables[2] = FloorFirstIndex[1];
    }

    private bool ActableCheck()
    {
        Debug.Log($"Remain : {m_Remain}");
        if (m_Remain == 0)
            return false;

        if ((m_Remains[0] > 0 && m_Actables[0] > 0)
        || (m_Remains[1] > 0 && m_Actables[1] > 0)
        || (m_Remains[2] > 0 && m_Actables[2] > 0))
        {
            return true;
        }

        return false;
    }

    public void RoundStart()
    {
        m_TextManager.NextRound();
        if (m_Socket.GetUserNum() == 0)
        {
            m_TextManager.StartTurn();
        }
        else
        {
            m_TextManager.TurnTextOff();
        }
    }
}