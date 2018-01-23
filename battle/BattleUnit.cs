using FinalWar;
using System.IO;
using System.Collections.Generic;

internal class BattleUnit
{
    private const long MAX_TICK = 1200;

    private PlayerUnit mPlayer;
    private PlayerUnit oPlayer;

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

    internal void Init(PlayerUnit _mPlayer, PlayerUnit _oPlayer, IList<int> _mCards, IList<int> _oCards, int _mapID, int _maxRoundNum, bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        mTick = oTick = BattleManager.Instance.tick;

        isVsAi = _isVsAi;

        battle.ServerStart(_mapID, _maxRoundNum, _mCards, _oCards, isVsAi);
    }

    internal void ReceiveData(PlayerUnit _playerUnit, BinaryReader _br)
    {
        bool isMine = _playerUnit == mPlayer;

        bool b = battle.ServerGetPackage(_br, isMine);

        if (mPlayer != null)
        {
            if (b)
            {
                if (isMine)
                {
                    mTick = BattleManager.Instance.tick;
                }
                else
                {
                    oTick = BattleManager.Instance.tick;
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
        mTick = oTick = BattleManager.Instance.tick;

        if (_result != Battle.BattleResult.NOT_OVER)
        {
            PlayerUnit m = mPlayer;

            PlayerUnit o = oPlayer;

            mPlayer = oPlayer = null;

            BattleManager.Instance.BattleOver(this, m, o);
        }
    }

    internal void Logout(PlayerUnit _playerUnit)
    {
        battle.ServerQuitBattleReal(_playerUnit == mPlayer);
    }

    internal bool CheckDoAutoAction(PlayerUnit _player)
    {
        if (_player == mPlayer)
        {
            if (BattleManager.Instance.tick - mTick > MAX_TICK)
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
            if (BattleManager.Instance.tick - oTick > MAX_TICK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal void DoAutoAction(PlayerUnit _player)
    {
        battle.ServerDoActionReal(_player == mPlayer);
    }
}
