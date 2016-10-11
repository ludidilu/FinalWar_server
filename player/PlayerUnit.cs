using System;
using System.IO;

internal class PlayerUnit : IUnit
{
    private Action<MemoryStream> sendDataCallBack;

    public void Init(Action<MemoryStream> _sendDataCallBack)
    {
        sendDataCallBack = _sendDataCallBack;

        if(sendDataCallBack != null)
        {
            BattleManager.Instance.PlayerEnter(this);
        }
    }

    public void ReceiveData(byte[] _bytes)
    {
        BattleManager.Instance.ReceiveData(this, _bytes);
    }

    public void SendData(MemoryStream _ms)
    {
        if(sendDataCallBack != null)
        {
            sendDataCallBack(_ms);
        }
    }
}

