using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
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

    private bool m_TimeOut = false;
    public int m_TurnPlayer = 0;
    public int[] m_Score = new int[3];

    private bool m_Toggle = false;
    private bool TurnCheck;
    private float m_CurrentTurnTime;
    private bool m_Pause;

    void Update()
    {
        if (true == m_Pause)
        {
            return;
        }

        Update_Timer();
        if (true == m_Toggle)
        {
            if (TurnCheck)
            {
                TurnChecker.text = $"Player {m_TurnPlayer}'s Turn";
            }

            m_Toggle = false;
        }
    }

    public void StartTurn()
    {
        m_CurrentTurnTime = TURNTIME;
        m_TimeOut = false;
        Timer.enabled = true;
        TurnChecker.text = "My Turn!";
    }

    public void EndTurn()
    {
        m_TimeOut = false;
        m_CurrentTurnTime = TURNTIME;
        m_Toggle = true;
    }

    private void Update_Timer()
    {
        if (true == m_TimeOut)
        {
            return;
        }

        m_CurrentTurnTime -= Time.deltaTime;
        Timer.text = ((int)m_CurrentTurnTime).ToString();

        if (m_CurrentTurnTime < 0)
        {
            m_CurrentTurnTime = 0f;
            m_TimeOut = true;
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
        if (PlayerNum != 0)
        {
            Vector3 Temp = m_Player[0].transform.position;
            m_Player[0].transform.position = m_Player[PlayerNum].transform.position;
            m_Player[PlayerNum].transform.position = Temp;
        }
    }

    public void add_Score()
    {
        ++m_Score[m_TurnPlayer];
        m_PlayerScore[m_TurnPlayer].text = $"{m_Score[m_TurnPlayer]}";
    }

    public bool GetTimeOut()
    {
        return m_TimeOut;
    }

    public void Set_TimeOut(bool timeOut)
    {
        m_TimeOut = timeOut;
    }
}
