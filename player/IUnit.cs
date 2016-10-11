using System;
using System.IO;

internal interface IUnit
{
    void Init(Action<MemoryStream> _sendDataCallBack);
    void ReceiveData(byte[] _bytes);
    void SendData(MemoryStream _ms);
}
