using System;
using System.Buffers.Binary;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CupManager : MonoBehaviour
{
    private const int MAXCUPINDEX = 27;
    private const int MAXUSABLEINDEX = 11;
    private const int GAP = 55;

    public Button[] Cups;
    public Button[] UsableCups;

    public SpriteAtlas Atlas;
    public string ChangeImageIndex = "2";

    public CSocket m_Socket;
    private PACKET m_SendPacket = new PACKET();
    private PACKET m_RecvPacket = new PACKET();
    static ManualResetEvent PauseEvent = new ManualResetEvent(true);
    private bool m_Toggle = false;

    private int Remain = MAXUSABLEINDEX + 1;
    private int ActiveCups = 7;
    private int SelectIndex = -1; // 하단 버튼 선택
    private bool m_StopLoop = false;

    void Start()
    {
        m_SendPacket.Data = new byte[512];
        m_RecvPacket.Data = new byte[512];

        Thread RecvThread = new Thread(() => RecvMessage());
        RecvThread.IsBackground = true;
        RecvThread.Start();

        //for debug
        Debug.Log(Remain);
        for (int i = 0; i < 7; ++i)
        {
            Cups[i].interactable = true;
        }

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }
    }
    void Update()
    {
        if (true == m_Toggle)
        {
            StackCup();
            PauseEvent.Set();
            m_Toggle = false;
        }
    }

    public void ChangeImage()
    {
        if (SelectIndex == -1)
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

        //cups 원소들의 인덱스가 바뀌어버린다.

        SelectIndex = -1;
    }

    public void SelectBol()
    {
        //선택한 버튼의 인덱스를 저장하기
        GameObject selectButton = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < Remain; ++i)
        {
            if (UsableCups[i].gameObject == selectButton)
            {
                SelectIndex = i;
                ChangeImageIndex = selectButton.GetComponent<Image>().sprite.name;
                break;
            }
        }
    }

    public bool UseBol(int StackIndex)
    {
        //선택하고 있었던 버튼을 맨 뒤로 보내고 디폴트 이미지로 전환
        if (Remain <= 0)
        {
            Debug.Log("UseBol Failed : No Remaining!");
            return false;
        }

        m_SendPacket.Type = DATATYPE.DATATYPE_GAME;
        m_SendPacket.DataSize = CSocket.HEADERSIZE_DEFAULT + 8;
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(0, 4), StackIndex);
        //Debug.Log(UsableCups[SelectIndex].GetComponent<Image>().sprite.name);
        BinaryPrimitives.WriteInt32BigEndian(m_SendPacket.Data.AsSpan(4, 4), int.Parse(UsableCups[SelectIndex].GetComponent<Image>().sprite.name));
        m_Socket.SendMessage(m_SendPacket);


        UsableCups[SelectIndex].GetComponent<Image>().sprite = Atlas.GetSprite("Blank");

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = SelectIndex + 1; i <= MAXUSABLEINDEX; ++i)
        {
            (UsableCups[i], UsableCups[i - 1]) = (UsableCups[i - 1], UsableCups[i]);
        }

        for (int i = SelectIndex; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }


        --Remain;
        --ActiveCups;
        return true;
    }

    private int CheckFloor(ref int Index) // 인덱스로 층 체크하기
    {
        if (Index == MAXCUPINDEX)
        {
            Index = 0;
            return 6;
        }
        for (int i = 7; i >= 1; --i)
        {
            if (Index < i)
            {
                return 7 - i;
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

        int Originalindex = CupPos;
        int Floor = CheckFloor(ref CupPos);

        //CupPos는 해당 층에서의 인덱스로 바뀜
        if (Floor == 7)
            return;
        if (CupPos != 0) // 좌측체크
        {
            if (Cups[Originalindex - 1].GetComponent<Image>().sprite.name != "Blank")
            {
                int NextIndex = Originalindex + (6 - Floor);
                if (false == Cups[NextIndex].interactable)
                {
                    ++ActiveCups;
                    Cups[NextIndex].interactable = true;
                }
            }
        }
        if (CupPos != 6 - Floor) //우측
        {
            if (Cups[Originalindex + 1].GetComponent<Image>().sprite.name != "Blank")
            {
                int NextIndex = Originalindex + (6 - Floor) + 1;
                if (false == Cups[NextIndex].interactable)
                {
                    ++ActiveCups;
                    Cups[NextIndex].interactable = true;
                }
            }
        }
    }
}