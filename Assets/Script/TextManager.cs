using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private const float TURNTIME = 30f;

    public TextMeshProUGUI Timer;
    public TextMeshProUGUI TurnChecker;
    public TextMeshProUGUI m_RoundText;
    public TextMeshProUGUI m_EndText;

    public bool m_TimeOut = true;
    public string TurnPlayer = "0";

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
                TurnChecker.text = $"Player {TurnPlayer}'s Turn";
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
        m_TimeOut = true;
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
            m_TimeOut = false;
        }
    }

    public void SetRoundText(int round)
    {
        m_RoundText.text = $"Round {round}";
    }

    public void EndText(bool Win)
    {
        m_EndText.text = Win == true ? "You Win!" : " You Loose!";
        m_EndText.gameObject.SetActive(true);
        TurnChecker.gameObject.SetActive(false);

        m_Pause = true;
    }
}
