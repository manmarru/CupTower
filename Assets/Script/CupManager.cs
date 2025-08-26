using System;
using System.Buffers.Binary;
using System.Text;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CupManager : MonoBehaviour
{
    private int[] FloorFirstIndex = { 0, 8, 15, 21, 26, 30, 33, 35, 36 };
    private const int MAXCUPINDEX = 36;
    private const int MAXUSABLEINDEX = 11;
    private const int GAP = 55;

    public TextManager m_TextManager;
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

    void Start()
    {
        if (m_Socket.GetUserNum() == 0)
        {
            m_Myturn = true;
        }
        m_SendPacket.Data = new byte[512];
        m_RecvPacket.Data = new byte[512];

        Thread RecvThread = new Thread(() => RecvMessage());
        RecvThread.IsBackground = true;
        RecvThread.Start();

        for (int i = 0; i < FloorFirstIndex[1]; ++i)
        {
            Cups[i].interactable = true;
        }

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }

        m_Actables[0] = m_Actables[1] = m_Actables[2] = FloorFirstIndex[1];
    }

    void Update()
    {
        if (true == m_Myturn && true == m_TextManager.m_TimeOut)
        {
            SkipTurn();
        }

        if (true == m_Toggle)
        {
            if (m_RecvPacket.Type == DATATYPE.DATATYPE_GAME)
            {
                StackCup();
            }
            else if (m_RecvPacket.Type == DATATYPE.DATATYPE_TURN)
            {
                m_TextManager.StartTurn();
            }
            else if (m_RecvPacket.Type == DATATYPE.DATATYPE_USERINFO)
            {
                TableSetting();
            }
            m_Toggle = false;
            PauseEvent.Set();
        }
    }

    public void GameAct()
    {
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

        if (false == UseBol(StackIndex))
            return;

        //Usablecups 원소들의 인덱스가 바뀌어버린다.

        m_SelectIndex = -1;
    }

    public void SelectBol()
    {
        if (false == m_Myturn)
        {
            m_SelectIndex = -1;
            return;
        }
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

    public bool UseBol(int StackIndex)
    {
        //선택하고 있었던 버튼을 뒤로 보내고 디폴트 이미지로 전환
        if (m_Remain <= 0)
        {
            Debug.Log("UseBol Failed : No Remaining!");
            return false;
        }

        if (StackIndex >= FloorFirstIndex[1]) // 0층이 아니라면 입력 유효성 체크
        {
            int FloorIndex = StackIndex;
            int Floor = CheckFloor(ref FloorIndex);
            //Debug.Log($"{Floor} Floor, {FloorIndex}th");

            string CheckSelect = UsableCups[m_SelectIndex].GetComponent<Image>().sprite.name[0].ToString();
            string CheckLeft = Cups[FloorFirstIndex[Floor - 1] + FloorIndex].GetComponent<StackCup>().GetName();
            string CheckRight = Cups[FloorFirstIndex[Floor - 1] + FloorIndex + 1].GetComponent<StackCup>().GetName();

            //Debug.Log($"Select : {CheckSelect}, Left : {CheckLeft}, Right : {CheckRight}");

            if (CheckLeft != CheckSelect && CheckRight != CheckSelect)
                return false;

            --m_Actables[CheckLeft[0] - '0'];
            --m_Actables[CheckRight[0] - '0'];
            if (CheckLeft == CheckRight)
            {
                ++m_Actables[CheckLeft[0] - '0'];
            }
        }
        else
        {
            --m_Actables[0];
            --m_Actables[1];
            --m_Actables[2];
        }


        m_SendPacket.Type = DATATYPE.DATATYPE_GAME;
        m_SendPacket.DataSize = CSocket.HEADERSIZE_DEFAULT + 8;
        int spriteName = UsableCups[m_SelectIndex].GetComponent<Image>().sprite.name[0] - '0';
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(0, 4), StackIndex);
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(4, 4), spriteName);
        m_Socket.SendMessage(m_SendPacket);

        UsableCups[m_SelectIndex].GetComponent<Image>().sprite = Atlas.GetSprite("Blank");

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

        // if (m_Remain == 0)
        // {
        //     m_SendPacket.Type = DATATYPE.DATATYPE_GAMESET;
        //     m_SendPacket.DataSize = CSocket.DATASIZE_NODATA;
        //     m_Socket.SendMessage(m_SendPacket);
        // }
        /*



        */
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
            m_Socket.RecvMessage(ref m_RecvPacket);
            Debug.Log("==================================");
            switch (m_RecvPacket.Type)
            {
                case DATATYPE.DATATYPE_CHAT:
                    {
                        string Message = Encoding.UTF8.GetString(m_RecvPacket.Data, 0, m_RecvPacket.DataSize);
                        Debug.Log(Message);
                        break;
                    }
                case DATATYPE.DATATYPE_GAME:
                    {
                        m_Toggle = true;
                        PauseEvent.Reset();
                        PauseEvent.WaitOne(); // 쓰레드 멈춤
                        break;
                    }
                case DATATYPE.DATATYPE_TURN:
                    {
                        int TurnNum = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
                        if (m_Socket.GetUserNum() == TurnNum)
                        {
                            m_Myturn = true;
                            m_Toggle = true;
                            PauseEvent.Reset();
                            PauseEvent.WaitOne(); // 쓰레드 멈춤
                        }
                        else
                        {
                            m_Myturn = false;
                            m_TextManager.TurnPlayer = TurnNum.ToString();
                            m_TextManager.EndTurn();
                            Debug.Log($"Player {TurnNum} Turn");
                        }
                        break;
                    }
                case DATATYPE.DATATYPE_ENDGAME:
                    {
                        m_StopLoop = true;
                        m_Socket.Shutdown();
                        m_Socket.Release();
                        break;
                    }
                case DATATYPE.DATATYPE_GAMESET:
                    {
                        break;
                    }
                case DATATYPE.DATATYPE_USERINFO:
                    {
                        m_Toggle = true;
                        PauseEvent.Reset();
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
        int CupPos = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data);
        int GameAct = BinaryPrimitives.ReadInt32BigEndian(m_RecvPacket.Data.AsSpan(4, 4));
        Debug.Log($"\nCupPos : {CupPos}\tGameAct : {GameAct}");

        Button Cup = Cups[CupPos];
        Image ThisImage = Cup.GetComponent<Image>();
        ThisImage.sprite = Atlas.GetSprite(GameAct.ToString());
        Cups[CupPos].GetComponent<StackCup>().SetName(GameAct.ToString());

        int Originalindex = CupPos;
        int Floor = CheckFloor(ref CupPos);

        //CupPos는 해당 층에서의 인덱스로 바뀜
        if (Floor == 7)
            return;
        if (CupPos != 0) // 좌측체크
        {
            string LeftName = Cups[Originalindex - 1].GetComponent<Image>().sprite.name;
            string CurrName = Cups[Originalindex].GetComponent<Image>().sprite.name;
            if (LeftName != "Blank")
            {
                int NextIndex = Originalindex + 7 - Floor;
                if (false == Cups[NextIndex].interactable)
                {
                    ++m_Actables[LeftName[0] - '0'];
                    ++m_Actables[CurrName[0] - '0'];
                    if (LeftName == CurrName)
                    {
                        --m_Actables[LeftName[0] - '0'];
                    }
                    Cups[NextIndex].interactable = true;
                }
            }
        }
        if (CupPos != 7 - Floor) //우측
        {
            string RightName = Cups[Originalindex + 1].GetComponent<Image>().sprite.name;
            string CurrName = Cups[Originalindex].GetComponent<Image>().sprite.name;
            if (RightName != "Blank")
            {
                int NextIndex = Originalindex + (7 - Floor) + 1;
                if (false == Cups[NextIndex].interactable)
                {
                    ++m_Actables[RightName[0] - '0'];
                    ++m_Actables[CurrName[0] - '0'];
                    if (RightName == CurrName)
                    {
                        --m_Actables[RightName[0] - '0'];
                    }

                    Cups[NextIndex].interactable = true;
                }
            }
        }

        Debug.Log($"m_Actables0 : {m_Actables[0]}, m_Actables1 : {m_Actables[1]}, m_Actables2 : {m_Actables[2]}");
    }

    public void SkipTurn()
    {
        m_SendPacket.Type = DATATYPE.DATATYPE_TURN;
        m_SendPacket.DataSize = CSocket.HEADERSIZE_DEFAULT;
        m_Socket.SendMessage(m_SendPacket);
        m_TextManager.EndTurn();
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
    }

    private bool ActableCheck()
    {
        if (m_Remain == 0)
            return false;

        for (int i = 0; i < FloorFirstIndex[1]; ++i)
        {
            if (Cups[i].interactable == true)
                return true;
        }



        return true;
    }
}