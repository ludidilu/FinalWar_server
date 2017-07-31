﻿using FinalWar;
using System.IO;
using System.Collections.Generic;

internal class BattleUnit
{
    private IUnit mPlayer;
    private IUnit oPlayer;

    private Battle battle;

    internal BattleUnit()
    {
        battle = new Battle();

        battle.ServerSetCallBack(SendData, BattleOver);
    }

    internal void Init(IUnit _mPlayer, IUnit _oPlayer, List<int> _mCards, List<int> _oCards, int _mapID, bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        MapSDS mapSDS = StaticData.GetData<MapSDS>(_mapID);

        Dictionary<int, int> heros = null;

        if (mapSDS.heroID.Length > 0)
        {
            heros = new Dictionary<int, int>();

            for (int i = 0; i < mapSDS.heroID.Length; i++)
            {
                heros.Add(mapSDS.heroPos[i], mapSDS.heroID[i]);
            }
        }

        if (_mCards.Count > Battle.DECK_CARD_NUM)
        {
            _mCards = _mCards.GetRange(0, Battle.DECK_CARD_NUM);
        }

        if (_oCards.Count > Battle.DECK_CARD_NUM)
        {
            _oCards = _oCards.GetRange(0, Battle.DECK_CARD_NUM);
        }

        battle.ServerStart(_mapID, heros, _mCards, _oCards, _isVsAi);
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
                else
                {
                    oPlayer.SendData(ms);
                }
            }
        }
    }

    private void BattleOver()
    {
        BattleManager.Instance.BattleOver(this);
    }
}
