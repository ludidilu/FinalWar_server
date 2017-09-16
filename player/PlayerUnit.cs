using Connection;

internal class PlayerUnit : UnitBase
{
    public override byte[] Login(long _tick)
    {
        return BattleManager.Instance.PlayerEnter(this);
    }

    public override void ReceiveData(byte[] _bytes, long _tick)
    {
        BattleManager.Instance.ReceiveData(this, _bytes, _tick);
    }
}