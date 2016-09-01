using FinalWar;
using System.IO;
using System.Collections.Generic;

public class BattleUnit
{
    private IUnit mPlayer;
    private IUnit oPlayer;

    private Battle battle;

    internal void Init(IUnit _mPlayer, IUnit _oPlayer,List<int> _mCards,List<int> _oCards,int _mapID,bool _isVsAi)
    {
        mPlayer = _mPlayer;
        oPlayer = _oPlayer;

        battle = new Battle();

        battle.ServerSetCallBack(SendData);

        battle.ServerStart(_mapID, _mCards, _oCards);
    }

    internal void RefreshData(IUnit _player)
    {
        battle.ServerRefreshData(_player == mPlayer);
    }
    
    internal void ReceiveData(PlayerUnit _playerUnit,byte[] _bytes)
    {
        battle.ServerGetPackage(_bytes, _playerUnit == mPlayer);
    }

    private void SendData(bool _isMine,MemoryStream _ms)
    {
        if (_isMine)
        {
            mPlayer.SendData(_ms);
        }
        else
        {
            oPlayer.SendData(_ms);
        }
    }
}
