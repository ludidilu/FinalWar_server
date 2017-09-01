using System.IO;
using Connection;

internal class PlayerUnit : UnitBase
{
    public override MemoryStream Login()
    {
        return BattleManager.Instance.PlayerEnter(this);
    }

    public override void ReceiveData(byte[] _bytes)
    {
        BattleManager.Instance.ReceiveData(this, _bytes);
    }
}