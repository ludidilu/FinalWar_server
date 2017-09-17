using FinalWar;
using System.IO;
using System.Collections.Generic;
using Connection;

internal class BattleUnit
{
    private const long MAX_TICK = 3000;

    private UnitBase mPlayer;
    private UnitBase oPlayer;

    private long mTick;
    private long oTick;

    private Battle_server battle;

    private bool isVsAi;

    internal bool isBattle { private set; get; }

    internal BattleUnit(bool _isBattle)
    {
        isBattle = _isBattle;

        battle = new Battle_server(isBattle);

        battle.ServerSetCallBack(SendData, BattleOver);
    }

    internal void Init(UnitBase _mPlayer, UnitBase _oPlayer, IList<int> _mCards, IList<int> _oCards, int _mapID, bool _isVsAi, long _tick)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        mTick = oTick = _tick;

        isVsAi = _isVsAi;

        battle.ServerStart(_mapID, _mCards, _oCards, isVsAi);
    }

    internal void ReceiveData(UnitBase _playerUnit, BinaryReader _br, long _tick)
    {
        bool isMine = _playerUnit == mPlayer;

        bool b = battle.ServerGetPackage(_br, isMine, true);

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

    private void BattleOver(Battle.BattleResult _result)
    {
        mPlayer = oPlayer = null;

        BattleManager.Instance.BattleOver(this);
    }

    internal void Logout(UnitBase _playerUnit)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(PackageTag.C2S_QUIT);

                ms.Position = 0;

                using (BinaryReader br = new BinaryReader(ms))
                {
                    battle.ServerGetPackage(br, _playerUnit == mPlayer, false);
                }
            }
        }
    }

    internal void Update(long _tick)
    {
        if (_tick - mTick > MAX_TICK)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    mTick = _tick;

                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(battle.roundNum);

                    bw.Write(0);

                    bw.Write(0);

                    ms.Position = 0;

                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        battle.ServerGetPackage(br, true, false);
                    }
                }
            }
        }

        if (!isVsAi && (_tick - oTick > MAX_TICK))
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    oTick = _tick;

                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(battle.roundNum);

                    bw.Write(0);

                    bw.Write(0);

                    ms.Position = 0;

                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        battle.ServerGetPackage(br, false, false);
                    }
                }
            }
        }
    }
}
