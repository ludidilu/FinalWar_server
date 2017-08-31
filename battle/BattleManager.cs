using System.Collections.Generic;
using System.IO;

internal class BattleManager
{
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

    private const int mapID = 2;

    private const bool isBattle = false;

    private Queue<BattleUnit> battleUnitPool1 = new Queue<BattleUnit>();

    private Queue<BattleUnit> battleUnitPool2 = new Queue<BattleUnit>();

    private Dictionary<BattleUnit, List<IUnit>> battleList = new Dictionary<BattleUnit, List<IUnit>>();

    private Dictionary<IUnit, BattleUnit> battleListWithPlayer = new Dictionary<IUnit, BattleUnit>();

    private IUnit lastPlayer = null;

    internal void PlayerEnter(IUnit _playerUnit)
    {
        if (battleListWithPlayer.ContainsKey(_playerUnit))
        {
            battleListWithPlayer[_playerUnit].RefreshData(_playerUnit);
        }
        else
        {
            if (_playerUnit == lastPlayer)
            {
                ReplyClient(_playerUnit, 1);
            }
            else
            {
                ReplyClient(_playerUnit, 2);
            }
        }
    }

    internal void ReceiveData(IUnit _playerUnit, byte[] _bytes)
    {
        using (MemoryStream ms = new MemoryStream(_bytes))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                short type = br.ReadInt16();

                switch (type)
                {
                    case 0:

                        if (battleListWithPlayer.ContainsKey(_playerUnit))
                        {
                            short length = br.ReadInt16();

                            byte[] bytes = br.ReadBytes(length);

                            battleListWithPlayer[_playerUnit].ReceiveData(_playerUnit, bytes);
                        }
                        else
                        {
                            if (_playerUnit == lastPlayer)
                            {
                                ReplyClient(_playerUnit, 1);
                            }
                            else
                            {
                                ReplyClient(_playerUnit, 2);
                            }
                        }

                        break;

                    case 1:

                        short actionType = br.ReadInt16();

                        ReceiveActionData(_playerUnit, actionType);

                        break;
                }
            }
        }
    }

    private void ReceiveActionData(IUnit _playerUnit, short _type)
    {
        BattleUnit battleUnit;

        IList<int> mCards;

        IList<int> oCards;

        switch (_type)
        {
            case 0:

                if (lastPlayer == null)
                {
                    lastPlayer = _playerUnit;

                    ReplyClient(_playerUnit, 1);
                }
                else
                {
                    battleUnit = GetBattleUnit(isBattle);

                    IUnit tmpPlayer = lastPlayer;

                    lastPlayer = null;

                    battleListWithPlayer.Add(_playerUnit, battleUnit);

                    battleListWithPlayer.Add(tmpPlayer, battleUnit);

                    battleList.Add(battleUnit, new List<IUnit>() { _playerUnit, tmpPlayer });

                    mCards = StaticData.GetData<TestCardsSDS>(1).cards;

                    oCards = StaticData.GetData<TestCardsSDS>(2).cards;

                    battleUnit.Init(_playerUnit, tmpPlayer, mCards, oCards, mapID, false);
                }

                break;

            case 1:

                battleUnit = GetBattleUnit(isBattle);

                battleListWithPlayer.Add(_playerUnit, battleUnit);

                battleList.Add(battleUnit, new List<IUnit>() { _playerUnit });

                mCards = StaticData.GetData<TestCardsSDS>(1).cards;

                oCards = StaticData.GetData<TestCardsSDS>(2).cards;

                battleUnit.Init(_playerUnit, null, mCards, oCards, mapID, true);

                break;

            case 2:

                if (lastPlayer == _playerUnit)
                {
                    lastPlayer = null;

                    ReplyClient(_playerUnit, 2);
                }

                break;
        }
    }

    private void ReplyClient(IUnit _playerUnit, short _type)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(_type);

                _playerUnit.SendData(ms);
            }
        }
    }

    internal void BattleOver(BattleUnit _battleUnit)
    {
        List<IUnit> tmpList = battleList[_battleUnit];

        for (int i = 0; i < tmpList.Count; i++)
        {
            battleListWithPlayer.Remove(tmpList[i]);
        }

        battleList.Remove(_battleUnit);

        ReleaseBattleUnit(_battleUnit);
    }

    private BattleUnit GetBattleUnit(bool _isBattle)
    {
        BattleUnit battleUnit;

        if (_isBattle && battleUnitPool1.Count > 0)
        {
            battleUnit = battleUnitPool1.Dequeue();
        }
        else if (!_isBattle && battleUnitPool2.Count > 0)
        {
            battleUnit = battleUnitPool2.Dequeue();
        }
        else
        {
            battleUnit = new BattleUnit(_isBattle);
        }

        return battleUnit;
    }

    private void ReleaseBattleUnit(BattleUnit _battleUnit)
    {
        if (_battleUnit.isBattle)
        {
            battleUnitPool1.Enqueue(_battleUnit);
        }
        else
        {
            battleUnitPool2.Enqueue(_battleUnit);
        }
    }
}

