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

    private List<int> mCards = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

    private List<int> oCards = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

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
                lastPlayer = null;
            }

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((short)0);

                    _playerUnit.SendData(ms);
                }
            }
        }
    }

    internal void ReceiveData(IUnit _playerUnit,byte[] _bytes)
    {
        if (battleListWithPlayer.ContainsKey(_playerUnit))
        {
            battleListWithPlayer[_playerUnit].ReceiveData(_playerUnit, _bytes);
        }
        else
        {
            ReceiveActionData(_playerUnit, _bytes);
        }
    }

    private void ReceiveActionData(IUnit _playerUnit, byte[] _bytes)
    {
        using (MemoryStream ms = new MemoryStream(_bytes))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                BattleUnit battleUnit;

                int type = br.ReadInt16();

                switch (type)
                {
                    case 0:

                        if (lastPlayer == null)
                        {
                            lastPlayer = _playerUnit;
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

                            battleUnit.Init(_playerUnit, tmpPlayer, mCards, oCards, 1, false);
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

                        battleUnit.Init(_playerUnit, null, mCards, oCards, 1, false);

                        break;
                }
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

