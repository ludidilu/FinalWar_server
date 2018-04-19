using FinalWar;
using System.IO;

internal class BattleUnit
{
    private const long MAX_TICK = 1200000;

    private int mPlayer;
    private int oPlayer;

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

    internal void Init(int _mPlayer, int _oPlayer, int[] _mCards, int[] _oCards, int _mapID, int _maxRoundNum, int _deckCardsNum, int _addCardsNum, int _addMoney, bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        mTick = oTick = BattleManager.Instance.tick;

        isVsAi = _isVsAi;

        battle.ServerStart(_mapID, _maxRoundNum, _deckCardsNum, _addCardsNum, _addMoney, _mCards, _oCards, isVsAi);
    }

    internal void ReceiveData(int _uid, BinaryReader _br)
    {
        bool isMine = _uid == mPlayer;

        bool b = battle.ServerGetPackage(_br, isMine);

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

                if (_isMine && mPlayer != -1)
                {
                    BattleManager.Instance.SendData(mPlayer, _isPush, ms);
                }
                else if (!_isMine && oPlayer != -1)
                {
                    BattleManager.Instance.SendData(oPlayer, _isPush, ms);
                }
            }
        }
    }

    private void BattleRoundOver(Battle.BattleResult _result)
    {
        mTick = oTick = BattleManager.Instance.tick;

        if (_result != Battle.BattleResult.NOT_OVER)
        {
            int m = mPlayer;

            int o = oPlayer;

            mPlayer = oPlayer = -1;

            BattleManager.Instance.BattleOver(this, m, o);
        }
    }

    internal void Logout(int _uid)
    {
        battle.ServerQuitBattleReal(_uid == mPlayer);
    }

    internal bool CheckDoAutoAction(int _player)
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

    internal void DoAutoAction(int _uid)
    {
        battle.ServerDoActionReal(_uid == mPlayer);
    }
}
