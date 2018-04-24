using Connection;
using System.IO;
using System;
using FinalWar_proto;

internal class PlayerUnit : UnitBase
{
    private int uid = -1;

    public void Logout()
    {
        uid = -1;
    }

    public override void Kick()
    {
        base.Kick();

        if (uid != -1)
        {
            PlayerUnitManager.Instance.Logout(uid);

            BattleManager.Instance.Logout(uid);

            Logout();
        }
    }

    public override void ReceiveData(byte[] _bytes)
    {
        if (uid != -1)
        {
            BattleManager.Instance.ReceiveData(uid, _bytes);
        }
        else
        {
            using (MemoryStream ms = new MemoryStream(_bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    try
                    {
                        CsPackageTag tag = (CsPackageTag)br.ReadInt32();

                        if (tag != CsPackageTag.Login)
                        {

                        }
                        else
                        {
                            LoginMessage message = LoginMessage.Parser.ParseFrom(ms);

                            uid = int.Parse(message.Name);

                            PlayerUnitManager.Instance.Login(uid, this);

                            BattleManager.Instance.Login(uid);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString());
                    }
                }
            }
        }
    }
}