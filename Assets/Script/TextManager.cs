using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    private const float TURNTIME = 30f;

    public TextMeshProUGUI Timer;
    public TextMeshProUGUI TurnChecker;
    public TextMeshProUGUI m_RoundText;
    public TextMeshProUGUI m_EndText;
    public TextMeshProUGUI[] m_PlayerScore;
    public GameObject[] m_Player;
    public GameObject m_GameSetImage;
    public Button m_NextRoundButton;

    private bool m_StopTimer = false;
    public int m_TurnPlayer = 0;
    public int[] m_Score = new int[3];

    private float m_CurrentTurnTime;
    private bool m_Pause;

    void Start()
    {
        m_CurrentTurnTime = TURNTIME;
    }

    void Update()
    {
        if (true == m_Pause)
        {
            return;
        }

        if (false == m_StopTimer)
        {
            Update_Timer();
        }
    }

    public void Set_TurnText(bool Myturn)
    {
        if (true == Myturn)
        {
            TurnChecker.text = "My Turn!";
        }
        else
        {
            TurnChecker.text = $"Player {m_TurnPlayer}'s Turn";
        }
    }

    public void StartTurn()
    {
        m_CurrentTurnTime = TURNTIME;
        m_StopTimer = false;
        TurnChecker.text = "My Turn!";
    }

    public void ResetTimer()
    {
        m_StopTimer = true;
        m_CurrentTurnTime = TURNTIME;
    }

    private void Update_Timer()
    {
        m_CurrentTurnTime -= Time.deltaTime;
        Timer.text = ((int)m_CurrentTurnTime).ToString();

        if (m_CurrentTurnTime < 0)
        {
            m_CurrentTurnTime = 0f;
            m_StopTimer = true;
        }
    }

    public void SetRoundText(int round)
    {
        m_RoundText.text = $"Round {round}";
    }

    public void EndText(bool Win, bool FinalRound)
    {
        m_EndText.text = Win == true ? "You Win!" : " You Loose!";
        m_GameSetImage.SetActive(true);
        TurnChecker.gameObject.SetActive(false);
        m_CurrentTurnTime = TURNTIME;
        m_NextRoundButton.gameObject.SetActive(!FinalRound);

        m_Pause = true;
    }

    public void NextRound()
    {
        for (int i = 0; i < 3; ++i)
        {
            m_Score[i] = 0;
            m_PlayerScore[i].text = "0";
        }

        m_GameSetImage.SetActive(false);
        TurnChecker.gameObject.SetActive(true);
        m_CurrentTurnTime = TURNTIME;
        m_Pause = false;
    }

    public void Set_PlayerTurn(int PlayerNum)
    {
        m_TurnPlayer = PlayerNum;
    }

    public void Set_MyPlayer(int PlayerNum)
    {
        //const float DEFAULTY = 95f;
        //const float DEFAULTX = 319;

        if (PlayerNum != 0)
        {
            Vector3 Pos = m_Player[0].transform.Find("ScoreUIParent").localPosition;
            Pos.x *= -1;
            m_Player[0].transform.Find("ScoreUIParent").localPosition = Pos;

            Pos = m_Player[PlayerNum].transform.Find("ScoreUIParent").localPosition;
            Pos.x *= -1;
            m_Player[PlayerNum].transform.Find("ScoreUIParent").localPosition = Pos;

            Pos = m_Player[0].transform.localPosition;
            Pos.x *= -1;
            if (PlayerNum == 2)
            {
                Pos.y *= -1;
            }
            m_Player[0].transform.localPosition = Pos;

            Pos = m_Player[PlayerNum].transform.localPosition;
            Pos.x *= -1;
            if (PlayerNum == 2)
            {
                Pos.y *= -1;
            }
            m_Player[PlayerNum].transform.localPosition = Pos;
        }
    }

    public void add_Score()
    {
        ++m_Score[m_TurnPlayer];
        m_PlayerScore[m_TurnPlayer].text = $"{m_Score[m_TurnPlayer]}";
    }

    public bool Get_TimerStop()
    {
        return m_StopTimer;
    }

    public void Set_TimerStop(bool timeOut)
    {
        m_StopTimer = timeOut;
    }
}
