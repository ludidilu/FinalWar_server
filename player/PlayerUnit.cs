using System.IO;
using Connection;
using System;

internal class PlayerUnit : UnitBase
{
    public override void Login(Action<MemoryStream> _callBack)
    {
        BattleManager.Instance.PlayerEnter(this, _callBack);
    }

    public override void ReceiveData(byte[] _bytes)
    {
        BattleManager.Instance.ReceiveData(this, _bytes);
    }
}