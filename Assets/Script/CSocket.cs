using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Android.Gradle.Manifest;
using Unity.Android.Types;
using UnityEditor.PackageManager;
using UnityEngine;


public enum DATATYPE
{
    DATATYPE_CHAT,
    DATATYPE_GAME
}

public struct PACKET
{
    public DATATYPE Type;
    public int DataSize;
    public byte[] Data;
}

public class CSocket : MonoBehaviour
{
    public const int HEADERSIZE = 4;

    static Socket m_Client;
    public bool m_StopLoop = false;
    byte[] RecvBuffer = new byte[512];
    byte[] SendBuffer = new byte[512];

    void Start()
    {

    }

    void Update()
    {

    }

    public void Connecting(string Address)
    {
        Debug.Log(Address);
        m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Client.Connect(new IPEndPoint(IPAddress.Parse(Address), 8000));

        Recv(4, RecvBuffer);
        int Message = BitConverter.ToInt32(RecvBuffer, 0); // 입장메시지 받기
        Debug.Log(Message);
    }

    public void RecvMessage()
    {
        DATATYPE Type = new DATATYPE();
        while (m_StopLoop == false)
        {
            if (false == Recv(HEADERSIZE, RecvBuffer)) // 타입
                return;
            Type = (DATATYPE)BitConverter.ToInt32(RecvBuffer, 0);
            System.Console.WriteLine(Type);

            if (false == Recv(4, RecvBuffer)) // 사이즈
                return;
            int DataSize = BitConverter.ToInt32(RecvBuffer, 0);
            System.Console.WriteLine(DataSize);

            if (false == Recv(DataSize, RecvBuffer)) // 데이터
                return;

            switch (Type)
            {
                case DATATYPE.DATATYPE_CHAT:
                    {
                        string Message = Encoding.UTF8.GetString(RecvBuffer, 0, DataSize);
                        System.Console.WriteLine(Message);
                        break;
                    }
                case DATATYPE.DATATYPE_GAME:
                    {
                        int GameAct = BitConverter.ToInt32(RecvBuffer, 0);
                        System.Console.WriteLine($"GameAct : {GameAct}");
                        break;
                    }
                default:
                    System.Console.WriteLine("데이터 타입 오류!");
                    return;
            }
        }
    }

    public void SendMessage(PACKET packet)
    {
        // DataType
        byte[] typeBytes = BitConverter.GetBytes((int)packet.Type);
        m_Client.Send(typeBytes, 0, 4, SocketFlags.None);

        // DataSize
        byte[] sizeBytes = BitConverter.GetBytes(packet.DataSize);
        m_Client.Send(sizeBytes, 0, 4, SocketFlags.None);

        // Data
        m_Client.Send(packet.Data, 0, packet.DataSize, SocketFlags.None);
    }

    public bool Recv(int DataSize, byte[] ReturnData)
    {
        int RecvLength = 0;
        while (RecvLength < DataSize)
        {
            int bytes = m_Client.Receive(ReturnData, RecvLength, DataSize - RecvLength, SocketFlags.None);
            if (0 == bytes)
                return false;
            RecvLength += bytes;
        }

        return true;
    }

    public void Shutdown()
    {
        m_StopLoop = true;
        m_Client.Shutdown(SocketShutdown.Both);
    }

    public void Release()
    {
        m_Client.Close();
    }
}
