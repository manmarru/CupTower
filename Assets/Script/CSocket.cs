using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public enum DATATYPE
{
    DATATYPE_DEBUG,
    DATATYPE_GAME,
    DATATYPE_TURN,
    DATATYPE_ENDGAME,
    DATATYPE_GAMESET,
    DATATYPE_USERINFO
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
    public const int DATASIZE_NODATA = 0;

    public static Socket m_Client;
    private static int UserNum;
    public bool m_StopLoop = false;
    private byte[] RecvBuffer = new byte[512]; // static으로 할까?
    private byte[] SendBuffer = new byte[512];

    public int GetUserNum()
    {
        return UserNum;
    }

    public void Connecting(string Address)
    {
        Debug.Log(Address);
        m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Client.Connect(new IPEndPoint(IPAddress.Parse(Address), 25565));

        Recv(4, RecvBuffer);
        UserNum = BitConverter.ToInt32(RecvBuffer, 0); // 입장메시지 받기

        Debug.Log($"UserNum{UserNum}");
    }

    public void RecvMessage(ref PACKET packet)
    {
        if (false == Recv(4, RecvBuffer)) // 타입
            return;
        packet.Type = (DATATYPE)BinaryPrimitives.ReadInt32BigEndian(RecvBuffer);

        if (false == Recv(4, RecvBuffer)) // 사이즈
            return;
        packet.DataSize = BinaryPrimitives.ReadInt32BigEndian(RecvBuffer);
        //Debug.Log($"Type : {packet.Type}, Size : {packet.DataSize}");

        if (false == Recv(packet.DataSize, packet.Data)) // 데이터
            return;
    }

    public void SendMessage(PACKET packet)
    {
        lock (SendBuffer)
        {
            BinaryPrimitives.WriteInt32BigEndian(SendBuffer.AsSpan(0, 4), (int)packet.Type);
            BinaryPrimitives.WriteInt32BigEndian(SendBuffer.AsSpan(4, 4), packet.DataSize);
            if (packet.DataSize != 0)
            {
                Buffer.BlockCopy(packet.Data, 0, SendBuffer, HEADERSIZE_DEFAULT, packet.DataSize);
            }
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

    public void Close()
    {
        m_Client.Close();
    }
}
