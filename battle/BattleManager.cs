using System.Collections.Generic;
using System.IO;
using Connection;
using System;

internal class BattleManager
{
    enum PlayerState
    {
        FREE,
        SEARCHING,
        BATTLE
    }

    internal enum PackageData
    {
        PVP,
        PVE,
        CANCEL_SEARCH
    }

    private static BattleManager _Instance;

    internal static BattleManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new BattleManager();
            }

            return _Instance;
        }
    }

    private const int testID = 2;

    private const bool processBattle = true;

    private Queue<BattleUnit> battleUnitPool1 = new Queue<BattleUnit>();

    private Queue<BattleUnit> battleUnitPool2 = new Queue<BattleUnit>();

    private Dictionary<UnitBase, BattleUnit> battleDic = new Dictionary<UnitBase, BattleUnit>();

    private Dictionary<UnitBase, BattleUnit> tmpDic = new Dictionary<UnitBase, BattleUnit>();

    private UnitBase lastPlayer = null;

    internal byte[] Login(UnitBase _playerUnit)
    {
        PlayerState playerState;

        if (_playerUnit == lastPlayer)
        {
            playerState = PlayerState.SEARCHING;
        }
        else
        {
            if (battleDic.ContainsKey(_playerUnit))
            {
                playerState = PlayerState.BATTLE;
            }
            else
            {
                playerState = PlayerState.FREE;
            }
        }

        return BitConverter.GetBytes((short)playerState);
    }

    internal void Logout(UnitBase _playerUnit)
    {
        if (lastPlayer == _playerUnit)
        {
            lastPlayer = null;
        }
        else
        {
            BattleUnit unit;

            if (battleDic.TryGetValue(_playerUnit, out unit))
            {
                unit.Logout(_playerUnit);
            }
        }
    }

    internal void ReceiveData(UnitBase _playerUnit, byte[] _bytes, long _tick)
    {
        using (MemoryStream ms = new MemoryStream(_bytes))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                bool isBattle = br.ReadBoolean();

                if (isBattle)
                {
                    if (battleDic.ContainsKey(_playerUnit))
                    {
                        battleDic[_playerUnit].ReceiveData(_playerUnit, br, _tick);
                    }
                    else
                    {
                        if (_playerUnit == lastPlayer)
                        {
                            ReplyClient(_playerUnit, false, PlayerState.SEARCHING);
                        }
                        else
                        {
                            ReplyClient(_playerUnit, false, PlayerState.FREE);
                        }
                    }
                }
                else
                {
                    if (battleDic.ContainsKey(_playerUnit))
                    {
                        ReplyClient(_playerUnit, false, PlayerState.BATTLE);
                    }
                    else
                    {
                        PackageData data = (PackageData)br.ReadInt16();

                        ReceiveActionData(_playerUnit, data, _tick);
                    }
                }
            }
        }
    }

    private void ReceiveActionData(UnitBase _playerUnit, PackageData _data, long _tick)
    {
        BattleUnit battleUnit;

        TestCardsSDS testCardSDS;

        switch (_data)
        {
            case PackageData.PVP:

                if (lastPlayer == null)
                {
                    lastPlayer = _playerUnit;

                    ReplyClient(_playerUnit, false, PlayerState.SEARCHING);
                }
                else if (lastPlayer == _playerUnit)
                {
                    ReplyClient(_playerUnit, false, PlayerState.SEARCHING);
                }
                else
                {
                    battleUnit = GetBattleUnit(processBattle);

                    UnitBase tmpPlayer = lastPlayer;

                    lastPlayer = null;

                    battleDic.Add(_playerUnit, battleUnit);

                    battleDic.Add(tmpPlayer, battleUnit);

                    testCardSDS = StaticData.GetData<TestCardsSDS>(testID);

                    battleUnit.Init(_playerUnit, tmpPlayer, testCardSDS.mCards, testCardSDS.oCards, testCardSDS.mapID, testCardSDS.maxRoundNum, false, _tick);

                    ReplyClient(_playerUnit, false, PlayerState.BATTLE);

                    ReplyClient(tmpPlayer, true, PlayerState.BATTLE);
                }

                break;

            case PackageData.PVE:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = null;
                }

                battleUnit = GetBattleUnit(processBattle);

                battleDic.Add(_playerUnit, battleUnit);

                testCardSDS = StaticData.GetData<TestCardsSDS>(testID);

                battleUnit.Init(_playerUnit, null, testCardSDS.mCards, testCardSDS.oCards, testCardSDS.mapID, testCardSDS.maxRoundNum, true, _tick);

                ReplyClient(_playerUnit, false, PlayerState.BATTLE);

                break;

            case PackageData.CANCEL_SEARCH:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = null;
                }

                ReplyClient(_playerUnit, false, PlayerState.FREE);

                break;
        }
    }

    private void ReplyClient(UnitBase _playerUnit, bool _isPush, PlayerState _playerState)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                if (_isPush)
                {
                    bw.Write(false);
                }

                bw.Write((short)_playerState);

                _playerUnit.SendData(_isPush, ms);
            }
        }
    }

    internal void BattleOver(BattleUnit _battleUnit, UnitBase _mPlayer, UnitBase _oPlayer)
    {
        battleDic.Remove(_mPlayer);

        if (_oPlayer != null)
        {
            battleDic.Remove(_oPlayer);
        }

        ReleaseBattleUnit(_battleUnit);
    }

    private BattleUnit GetBattleUnit(bool _processBattle)
    {
        BattleUnit battleUnit;

        if (_processBattle && battleUnitPool1.Count > 0)
        {
            battleUnit = battleUnitPool1.Dequeue();
        }
        else if (!_processBattle && battleUnitPool2.Count > 0)
        {
            battleUnit = battleUnitPool2.Dequeue();
        }
        else
        {
            battleUnit = new BattleUnit(_processBattle);
        }

        return battleUnit;
    }

    private void ReleaseBattleUnit(BattleUnit _battleUnit)
    {
        if (_battleUnit.processBattle)
        {
            battleUnitPool1.Enqueue(_battleUnit);
        }
        else
        {
            battleUnitPool2.Enqueue(_battleUnit);
        }
    }

    internal void Update(long _tick)
    {
        IEnumerator<KeyValuePair<UnitBase, BattleUnit>> enumerator = battleDic.GetEnumerator();

        while (enumerator.MoveNext())
        {
            KeyValuePair<UnitBase, BattleUnit> pair = enumerator.Current;

            if (pair.Value.CheckDoAutoAction(_tick, pair.Key))
            {
                tmpDic.Add(pair.Key, pair.Value);
            }
        }

        if (tmpDic.Count > 0)
        {
            enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<UnitBase, BattleUnit> pair = enumerator.Current;

                pair.Value.DoAutoAction(pair.Key);
            }

            tmpDic.Clear();
        }
    }
}