using System.Collections.Generic;
using System.IO;

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

    public static BattleManager Instance;

    private const int PVP_BATTLE_ID = 2;

    private const bool processBattle = true;

    private Queue<BattleUnit> battleUnitPool1 = new Queue<BattleUnit>();

    private Queue<BattleUnit> battleUnitPool2 = new Queue<BattleUnit>();

    private Dictionary<int, BattleUnit> battleDic = new Dictionary<int, BattleUnit>();

    private int lastPlayer = -1;

    internal long tick { private set; get; }

    internal void Login(int _playerUnit)
    {
        PlayerState playerState;

        if (lastPlayer == _playerUnit)
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

        ReplyClient(_playerUnit, false, playerState);
    }

    internal void Logout(int _playerUnit)
    {
        if (lastPlayer == _playerUnit)
        {
            lastPlayer = -1;
        }
    }

    internal void ReceiveData(int _playerUnit, byte[] _bytes)
    {
        using (MemoryStream ms = new MemoryStream(_bytes))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                bool isBattle = br.ReadBoolean();

                if (isBattle)
                {
                    BattleUnit battleUnit;

                    if (battleDic.TryGetValue(_playerUnit, out battleUnit))
                    {
                        battleUnit.ReceiveData(_playerUnit, br);
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

    private void ReceiveActionData(int _playerUnit, BinaryReader _br)
    {
        PackageData data = (PackageData)_br.ReadInt16();

        BattleUnit battleUnit;

        BattleSDS battleSDS;

        switch (data)
        {
            case PackageData.PVP:

                if (lastPlayer == -1)
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

                    int tmpPlayer = lastPlayer;

                    lastPlayer = -1;

                    battleDic.Add(_playerUnit, battleUnit);

                    battleDic.Add(tmpPlayer, battleUnit);

                    battleSDS = StaticData.GetData<BattleSDS>(PVP_BATTLE_ID);

                    battleUnit.Init(_playerUnit, tmpPlayer, battleSDS.mCards, battleSDS.oCards, battleSDS.mapID, battleSDS.maxRoundNum, battleSDS.deckCardsNum, battleSDS.addCardsNum, battleSDS.addMoney, battleSDS.defaultHandCardsNum, battleSDS.defaultMoney, false);

                    ReplyClient(_playerUnit, false, PlayerState.BATTLE);

                    ReplyClient(tmpPlayer, true, PlayerState.BATTLE);
                }

                break;

            case PackageData.PVE:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = -1;
                }

                int battleID = _br.ReadInt32();

                battleUnit = GetBattleUnit(processBattle);

                battleDic.Add(_playerUnit, battleUnit);

                battleSDS = StaticData.GetData<BattleSDS>(battleID);

                battleUnit.Init(_playerUnit, -1, battleSDS.mCards, battleSDS.oCards, battleSDS.mapID, battleSDS.maxRoundNum, battleSDS.deckCardsNum, battleSDS.addCardsNum, battleSDS.addMoney, battleSDS.defaultHandCardsNum, battleSDS.defaultMoney, true);

                ReplyClient(_playerUnit, false, PlayerState.BATTLE);

                break;

            case PackageData.CANCEL_SEARCH:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = -1;
                }

                ReplyClient(_playerUnit, false, PlayerState.FREE);

                break;
        }
    }

    private void ReplyClient(int _uid, bool _isPush, PlayerState _playerState)
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

                SendData(_uid, _isPush, ms);
            }
        }
    }

    internal void BattleOver(BattleUnit _battleUnit, int _mPlayer, int _oPlayer)
    {
        if (_mPlayer != -1)
        {
            battleDic.Remove(_mPlayer);
        }

        if (_oPlayer != -1)
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

    public void SendData(int _uid, bool _isPush, MemoryStream _ms)
    {
        PlayerUnitManager.Instance.SendData(_uid, _isPush, _ms);
    }

    private Dictionary<int, BattleUnit> tmpDic = new Dictionary<int, BattleUnit>();

    internal void Update(long _tick)
    {
        tick = _tick;

        IEnumerator<KeyValuePair<int, BattleUnit>> enumerator = battleDic.GetEnumerator();

        while (enumerator.MoveNext())
        {
            KeyValuePair<int, BattleUnit> pair = enumerator.Current;

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
                KeyValuePair<int, BattleUnit> pair = enumerator.Current;

                pair.Value.DoAutoAction(pair.Key);
            }

            tmpDic.Clear();
        }
    }
}