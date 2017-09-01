using FinalWar;
using System.IO;
using System.Collections.Generic;
using Connection;

internal class BattleUnit
{
    private UnitBase mPlayer;
    private UnitBase oPlayer;

    private Battle_server battle;

    internal bool isBattle { private set; get; }

    internal BattleUnit(bool _isBattle)
    {
        isBattle = _isBattle;

        battle = new Battle_server(isBattle);

        battle.ServerSetCallBack(SendData, BattleOver);
    }

    internal void Init(UnitBase _mPlayer, UnitBase _oPlayer, IList<int> _mCards, IList<int> _oCards, int _mapID, bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        battle.ServerStart(_mapID, _mCards, _oCards, _isVsAi);
    }

    internal void ReceiveData(UnitBase _playerUnit, BinaryReader _br)
    {
        battle.ServerGetPackage(_br, _playerUnit == mPlayer);
    }

    private void SendData(bool _isMine, bool _isPush, MemoryStream _ms)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(true);

                bw.Write(_ms.GetBuffer(), 0, (int)_ms.Length);

                if (_isMine)
                {
                    mPlayer.SendData(_isPush, ms);
                }
                else if (oPlayer != null)
                {
                    oPlayer.SendData(_isPush, ms);
                }
            }
        }
    }

    private void BattleOver(Battle.BattleResult _result)
    {
        BattleManager.Instance.BattleOver(this);
    }
}
