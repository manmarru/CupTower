using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Android.Types;
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
    public const int HEADERSIZE_DEFAULT = 8;
    public const int DATASIZE_GAMEACT = 8;

    public static Socket m_Client;
    public bool m_StopLoop = false;
    byte[] RecvBuffer = new byte[512];
    byte[] SendBuffer = new byte[512];

    public void Connecting(string Address)
    {
        Debug.Log(Address);
        m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Client.Connect(new IPEndPoint(IPAddress.Parse(Address), 8000));

        Recv(4, RecvBuffer);
        int Message = BitConverter.ToInt32(RecvBuffer, 0); // 입장메시지 받기
        Debug.Log(Message);
    }

    public void RecvMessage(ref PACKET packet)
    {
        if (false == Recv(4, RecvBuffer)) // 타입
            return;
        packet.Type = (DATATYPE)BinaryPrimitives.ReadInt32BigEndian(RecvBuffer);
        Debug.Log(packet.Type);

        if (false == Recv(4, RecvBuffer)) // 사이즈
            return;
        packet.DataSize = BinaryPrimitives.ReadInt32BigEndian(RecvBuffer);
        Debug.Log(packet.DataSize);

        if (false == Recv(packet.DataSize, packet.Data)) // 데이터
            return;


        //여기서부터 코드를 외부로 옮겨야한다.


        // switch (packet.Type)
        // {
        //     case DATATYPE.DATATYPE_CHAT:
        //         {
        //             string Message = Encoding.UTF8.GetString(RecvBuffer, 0, packet.DataSize);
        //             System.Console.WriteLine(Message);
        //             break;
        //         }
        //     case DATATYPE.DATATYPE_GAME:
        //         {
        //             int CupPos = BinaryPrimitives.ReadInt32BigEndian(RecvBuffer);
        //             int GameAct = BinaryPrimitives.ReadInt32BigEndian(RecvBuffer.AsSpan(4, 4));
        //             Debug.Log($"CupPos : {CupPos}\nGameAct : {GameAct}");
        //             break;
        //         }
        //     default:
        //         Debug.Log("데이터 타입 오류!");
        //         return;
        // }
    }

    public void SendMessage(PACKET packet)
    {
        BinaryPrimitives.WriteInt32BigEndian(SendBuffer.AsSpan(0, 4), (int)packet.Type);
        BinaryPrimitives.WriteInt32BigEndian(SendBuffer.AsSpan(4, 4), packet.DataSize);
        Buffer.BlockCopy(packet.Data, 0, SendBuffer, HEADERSIZE_DEFAULT, packet.DataSize);

        int SendedSize = 0;
        int TotalSize = HEADERSIZE_DEFAULT + packet.DataSize;

        while (SendedSize < TotalSize)
        {
            int send = m_Client.Send(SendBuffer, SendedSize, TotalSize - SendedSize, SocketFlags.None);
            if (send <= 0)
            {
                Debug.Log("Disconnected While Calling SendMessage");
                return;
            }
            SendedSize += send;
        }
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
