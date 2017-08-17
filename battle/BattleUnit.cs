﻿using FinalWar;
using System.IO;
using System.Collections.Generic;

internal class BattleUnit
{
    private IUnit mPlayer;
    private IUnit oPlayer;

    private Battle_server battle;

    internal BattleUnit()
    {
        battle = new Battle_server();

        battle.ServerSetCallBack(SendData, BattleOver);
    }

    internal void Init(IUnit _mPlayer, IUnit _oPlayer, List<int> _mCards, List<int> _oCards, int _mapID, bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        battle.ServerStart(_mapID, _mCards, _oCards, _isVsAi);
    }

    internal void RefreshData(IUnit _player)
    {
        battle.ServerRefreshData(_player == mPlayer);
    }

    internal void ReceiveData(IUnit _playerUnit, byte[] _bytes)
    {
        battle.ServerGetPackage(_bytes, _playerUnit == mPlayer);
    }

    private void SendData(bool _isMine, MemoryStream _ms)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((short)0);

                short length = (short)_ms.Length;

                bw.Write(length);

                bw.Write(_ms.GetBuffer(), 0, length);

                if (_isMine)
                {
                    mPlayer.SendData(ms);
                }
                else if (oPlayer != null)
                {
                    oPlayer.SendData(ms);
                }
            }
        }
    }

    private void BattleOver(Battle.BattleResult _result)
    {
        BattleManager.Instance.BattleOver(this);
    }
}
