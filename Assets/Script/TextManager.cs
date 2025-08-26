using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private const float TURNTIME = 30f;

    public TextMeshProUGUI Timer;
    public TextMeshProUGUI TurnChecker;

    public bool m_TimeOut = true;
    public string TurnPlayer = "0";

    private float m_CurrentTurnTime;

    void Update()
    {
        Update_Timer();
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
        Timer.enabled = false;
        TurnChecker.text = $"Player {TurnPlayer}'s Turn";
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
}
