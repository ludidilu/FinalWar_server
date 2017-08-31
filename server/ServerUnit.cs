using System;
using System.Net.Sockets;
using System.IO;

internal class ServerUnit<T> where T : IUnit, new()
{
    private const int KICK_TICK_LONG = 100000;

    private const int HEAD_LENGTH = 2;

    private const int UID_LENGTH = 4;

    private Socket socket;

    private ushort bodyLength;

    private byte[] uidBuffer = new byte[UID_LENGTH];

    private byte[] headBuffer = new byte[HEAD_LENGTH];

    private byte[] bodyBuffer = new byte[short.MaxValue];

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

        if (unit != null)
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
            socket.Receive(uidBuffer, UID_LENGTH, SocketFlags.None);

            int uid = BitConverter.ToInt32(uidBuffer, 0);

            if (uid < 1)
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
        if (_tick - lastTick > KICK_TICK_LONG)
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

            unit.ReceiveData(bodyBuffer);

            ReceiveHead(_tick);
        }
    }

    internal void SendData(MemoryStream _ms)
    {
        int length = HEAD_LENGTH + (int)_ms.Length;

        Array.Copy(BitConverter.GetBytes((ushort)_ms.Length), bodyBuffer, HEAD_LENGTH);

        Array.Copy(_ms.GetBuffer(), 0, bodyBuffer, HEAD_LENGTH, _ms.Length);

        socket.BeginSend(bodyBuffer, 0, length, SocketFlags.None, SendCallBack, null);
    }

    private void SendCallBack(IAsyncResult _result)
    {
        socket.EndSend(_result);
    }
}

