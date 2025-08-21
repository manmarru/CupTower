using System.Threading;
using UnityEngine;

public class Level_Matching : MonoBehaviour
{
    public CSocket m_Socket;
    private bool m_Mached = false;
    public GameManager Manager;

    void Start()
    {
        Thread RecvThread = new Thread(() => WaitForUser());
        RecvThread.IsBackground = true;
        RecvThread.Start();
    }

    void Update()
    {
        if (true == m_Mached)
        {
            Manager.NextScene();
            m_Mached = false;
        }
    }

    private void WaitForUser()
    {
        byte[] RecvData = new byte[4];
        m_Mached = m_Socket.Recv(4, RecvData); // 999를 받기는 할건데 안 씀
        Debug.Log(true == m_Mached ? "Matched" : "MatchingSign Error?");
    }
}
