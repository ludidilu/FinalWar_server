using System;
using System.Net.Sockets;
using System.IO;

public class ServerUnit<T> where T : IUnit, new()
{
    private const int KICK_TICK_LONG = 100000;

    private const int HEAD_LENGTH = 2;

    private const int UID_LENGTH = 4;

    private Socket socket;

    private ushort bodyLength;

    private byte[] headBuffer = new byte[HEAD_LENGTH];

    private byte[] bodyBuffer = new byte[ushort.MaxValue];

    internal T unit { get; private set; }

    private bool isReceiveHead = true;

    private int lastTick;

    internal void Init(Socket _socket, int _tick)
    {
        socket = _socket;

        lastTick = _tick;
    }

    internal void SetUnit(T _unit)
    {
        unit = _unit;

        unit.Init(SendData);
    }

    internal void Kick()
    {
        Log.Write("One user be kicked");

        if(unit != null)
        {
            unit.Init(null);
        }

        //socket.Shutdown(SocketShutdown.Both);

        socket.Close();
    }

    internal int CheckLogin(int _tick)
    {
        if (_tick - lastTick > KICK_TICK_LONG)
        {
            Kick();

            return -1;
        }
        else if (socket.Available >= UID_LENGTH)
        {
            byte[] bytes = new byte[UID_LENGTH];

            socket.Receive(bytes, UID_LENGTH, SocketFlags.None);

            int uid = BitConverter.ToInt32(bytes, 0);

            if(uid < 1)
            {
                Kick();

                return -1;
            }
            else
            {
                return uid;
            }
        }
        else
        {
            return 0;
        }
    } 

    internal bool Update(int _tick)
    {
        if(_tick - lastTick > KICK_TICK_LONG)
        {
            Kick();

            return true;
        }

        if (isReceiveHead)
        {
            ReceiveHead(_tick);
        }
        else
        {
            ReceiveBody(_tick);
        }

        return false;
    }

    private void ReceiveHead(int _tick)
    {
        if (socket.Available >= HEAD_LENGTH)
        {
            socket.Receive(headBuffer, HEAD_LENGTH, SocketFlags.None);

            isReceiveHead = false;

            bodyLength = BitConverter.ToUInt16(headBuffer, 0);

            ReceiveBody(_tick);
        }
    }

    private void ReceiveBody(int _tick)
    {
        if (socket.Available >= bodyLength)
        {
            lastTick = _tick;

            socket.Receive(bodyBuffer, bodyLength, SocketFlags.None);

            isReceiveHead = true;
           
            byte[] resultBytes = new byte[bodyLength];

            Array.Copy(bodyBuffer, 0, resultBytes, 0, bodyLength);

            unit.ReceiveData(resultBytes);

            ReceiveHead(_tick);
        }
    }

    public void SendData(MemoryStream _ms)
    {
        int length = HEAD_LENGTH + (int)_ms.Length;

        byte[] bytes = new byte[length];

        Array.Copy(BitConverter.GetBytes((ushort)_ms.Length), bytes, HEAD_LENGTH);

        Array.Copy(_ms.GetBuffer(), 0, bytes, HEAD_LENGTH, _ms.Length);

        socket.BeginSend(bytes, 0, length, SocketFlags.None, SendCallBack, null);
    }
    
    private void SendCallBack(IAsyncResult result)
    {

    }
}

