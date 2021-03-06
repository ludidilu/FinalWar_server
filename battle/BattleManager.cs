﻿using System.Collections.Generic;
using System.IO;

internal class BattleManager
{
    private static BattleManager _Instance;

    internal static BattleManager Instance
    {
        get
        {
            if(_Instance == null)
            {
                _Instance = new BattleManager();
            }

            return _Instance;
        }
    }

    private Queue<BattleUnit> battleUnitPool = new Queue<BattleUnit>();

    private List<int> mCards = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private List<int> oCards = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private const float aiFix = 1.3f;

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

    internal void ReceiveData(IUnit _playerUnit,byte[] _bytes)
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
                    if (battleUnitPool.Count > 0)
                    {
                        battleUnit = battleUnitPool.Dequeue();
                    }
                    else
                    {
                        battleUnit = new BattleUnit();
                    }

                    IUnit tmpPlayer = lastPlayer;

                    lastPlayer = null;

                    battleListWithPlayer.Add(_playerUnit, battleUnit);

                    battleListWithPlayer.Add(tmpPlayer, battleUnit);

                    battleList.Add(battleUnit, new List<IUnit>() { _playerUnit, tmpPlayer });

                    battleUnit.Init(_playerUnit, tmpPlayer, mCards, oCards, 1, false, 1, 1);
                }

                break;

            case 1:

                if (battleUnitPool.Count > 0)
                {
                    battleUnit = battleUnitPool.Dequeue();
                }
                else
                {
                    battleUnit = new BattleUnit();
                }

                battleListWithPlayer.Add(_playerUnit, battleUnit);

                battleList.Add(battleUnit, new List<IUnit>() { _playerUnit });

                battleUnit.Init(_playerUnit, null, mCards, oCards, 1, true, 1, aiFix);

                break;

            case 2:

                if(lastPlayer == _playerUnit)
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

        for(int i = 0; i < tmpList.Count; i++)
        {
            battleListWithPlayer.Remove(tmpList[i]);
        }

        battleList.Remove(_battleUnit);

        battleUnitPool.Enqueue(_battleUnit);
    }
}

