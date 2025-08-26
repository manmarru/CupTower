using System.Buffers.Binary;
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
        m_Socket.Recv(4, RecvData); // 999를 받기는 할건데 수신 여부만 체크
        if (BinaryPrimitives.ReadInt32BigEndian(RecvData) == 999)
        {
            Debug.Log("Matched");
            m_Mached = true;
        }
        else
        {
            Debug.Log("MatchingSign Error?");
        }
    }
}
