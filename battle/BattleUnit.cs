using FinalWar;
using System.IO;
using System.Collections.Generic;
using Connection;

internal class BattleUnit
{
    private const long MAX_TICK = 1200;

    private UnitBase mPlayer;
    private UnitBase oPlayer;

    private long mTick;
    private long oTick;

    private Battle_server battle;

    private bool isVsAi;

    internal bool processBattle { private set; get; }

    internal BattleUnit(bool _processBattle)
    {
        processBattle = _processBattle;

        battle = new Battle_server(processBattle);

        battle.ServerSetCallBack(SendData, BattleRoundOver);
    }

    internal void Init(UnitBase _mPlayer, UnitBase _oPlayer, IList<int> _mCards, IList<int> _oCards, int _mapID, int _maxRoundNum, int _randomSeed, bool _isVsAi, long _tick)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        mTick = oTick = _tick;

        isVsAi = _isVsAi;

        battle.ServerStart(_mapID, _maxRoundNum, _mCards, _oCards, _randomSeed, isVsAi);
    }

    internal void ReceiveData(UnitBase _playerUnit, BinaryReader _br, long _tick)
    {
        bool isMine = _playerUnit == mPlayer;

        bool b = battle.ServerGetPackage(_br, isMine);

        if (mPlayer != null)
        {
            if (b)
            {
                if (isMine)
                {
                    mTick = _tick;
                }
                else
                {
                    oTick = _tick;
                }
            }
        }
    }

    private void SendData(bool _isMine, bool _isPush, MemoryStream _ms)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                if (_isPush)
                {
                    bw.Write(true);
                }

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

    private void BattleRoundOver(Battle.BattleResult _result)
    {
        if (_result != Battle.BattleResult.NOT_OVER)
        {
            UnitBase m = mPlayer;

            UnitBase o = oPlayer;

            mPlayer = oPlayer = null;

            BattleManager.Instance.BattleOver(this, m, o);
        }
    }

    internal void Logout(UnitBase _playerUnit)
    {
        battle.ServerQuitBattleReal(_playerUnit == mPlayer);
    }

    internal bool CheckDoAutoAction(long _tick, UnitBase _player)
    {
        if (_player == mPlayer)
        {
            if (_tick - mTick > MAX_TICK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (_tick - oTick > MAX_TICK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal void DoAutoAction(UnitBase _player)
    {
        battle.ServerDoActionReal(_player == mPlayer);
    }
}
