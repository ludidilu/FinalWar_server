using System.Collections.Generic;
using System.IO;
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

    private const int PVP_BATTLE_ID = 2;

    private const bool processBattle = true;

    private Queue<BattleUnit> battleUnitPool1 = new Queue<BattleUnit>();

    private Queue<BattleUnit> battleUnitPool2 = new Queue<BattleUnit>();

    private Dictionary<PlayerUnit, BattleUnit> battleDic = new Dictionary<PlayerUnit, BattleUnit>();

    private Dictionary<PlayerUnit, BattleUnit> tmpDic = new Dictionary<PlayerUnit, BattleUnit>();

    private PlayerUnit lastPlayer = null;

    internal long tick { private set; get; }

    internal byte[] Login(PlayerUnit _playerUnit)
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

    internal void Logout(PlayerUnit _playerUnit)
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

    internal void ReceiveData(PlayerUnit _playerUnit, byte[] _bytes)
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
                        battleDic[_playerUnit].ReceiveData(_playerUnit, br);
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
                        ReceiveActionData(_playerUnit, br);
                    }
                }
            }
        }
    }

    private void ReceiveActionData(PlayerUnit _playerUnit, BinaryReader _br)
    {
        PackageData data = (PackageData)_br.ReadInt16();

        BattleUnit battleUnit;

        BattleSDS battleSDS;

        switch (data)
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

                    PlayerUnit tmpPlayer = lastPlayer;

                    lastPlayer = null;

                    battleDic.Add(_playerUnit, battleUnit);

                    battleDic.Add(tmpPlayer, battleUnit);

                    battleSDS = StaticData.GetData<BattleSDS>(PVP_BATTLE_ID);

                    battleUnit.Init(_playerUnit, tmpPlayer, battleSDS.mCards, battleSDS.oCards, battleSDS.mapID, battleSDS.maxRoundNum, false);

                    ReplyClient(_playerUnit, false, PlayerState.BATTLE);

                    ReplyClient(tmpPlayer, true, PlayerState.BATTLE);
                }

                break;

            case PackageData.PVE:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = null;
                }

                int battleID = _br.ReadInt32();

                battleUnit = GetBattleUnit(processBattle);

                battleDic.Add(_playerUnit, battleUnit);

                battleSDS = StaticData.GetData<BattleSDS>(battleID);

                battleUnit.Init(_playerUnit, null, battleSDS.mCards, battleSDS.oCards, battleSDS.mapID, battleSDS.maxRoundNum, true);

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

    private void ReplyClient(PlayerUnit _playerUnit, bool _isPush, PlayerState _playerState)
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

    internal void BattleOver(BattleUnit _battleUnit, PlayerUnit _mPlayer, PlayerUnit _oPlayer)
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
        tick = _tick;

        IEnumerator<KeyValuePair<PlayerUnit, BattleUnit>> enumerator = battleDic.GetEnumerator();

        while (enumerator.MoveNext())
        {
            KeyValuePair<PlayerUnit, BattleUnit> pair = enumerator.Current;

            if (pair.Value.CheckDoAutoAction(pair.Key))
            {
                tmpDic.Add(pair.Key, pair.Value);
            }
        }

        if (tmpDic.Count > 0)
        {
            enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<PlayerUnit, BattleUnit> pair = enumerator.Current;

                pair.Value.DoAutoAction(pair.Key);
            }

            tmpDic.Clear();
        }
    }
}