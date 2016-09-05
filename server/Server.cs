using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;


public class Server<T> where T : IUnit, new()
{
    private Socket socket;

    private List<ServerUnit> noLoginList = new List<ServerUnit>();

    private Dictionary<int, ServerUnit> loginDic = new Dictionary<int, ServerUnit>();

    private Dictionary<int, IUnit> logoutDic = new Dictionary<int, IUnit>();

    private int tick = 0;

    public void Start(string _path, int _port, int _maxConnections)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Bind(new IPEndPoint(IPAddress.Parse(_path), _port));

        socket.Listen(_maxConnections);

        BeginAccept();
    }

    private void BeginAccept()
    {
        socket.BeginAccept(SocketAccept, null);
    }

    private void SocketAccept(IAsyncResult _result)
    {
        Socket clientSocket = socket.EndAccept(_result);

        Console.WriteLine("One user connect");

        ServerUnit serverUnit = new ServerUnit();

        lock (noLoginList)
        {
            noLoginList.Add(serverUnit);

            serverUnit.Init(clientSocket, tick);
        }

        BeginAccept();
    }

    internal void Update()
    {
        lock (noLoginList)
        {
            tick++;

            for (int i = noLoginList.Count - 1; i > -1; i--)
            {
                ServerUnit serverUnit = noLoginList[i];

                int uid = serverUnit.CheckLogin(tick);

                if (uid == -1)
                {
                    noLoginList.RemoveAt(i);
                }
                else if (uid != 0)
                {
                    Console.WriteLine("One user login   uid:" + uid);

                    noLoginList.RemoveAt(i);

                    if (loginDic.ContainsKey(uid))
                    {
                        ServerUnit oldServerUnit = loginDic[uid];

                        oldServerUnit.Kick();

                        serverUnit.SetUnit(oldServerUnit.unit);

                        loginDic[uid] = serverUnit;
                    }
                    else if (logoutDic.ContainsKey(uid))
                    {
                        IUnit unit = logoutDic[uid];

                        logoutDic.Remove(uid);

                        serverUnit.SetUnit(unit);

                        loginDic.Add(uid, serverUnit);
                    }
                    else
                    {
                        T unit = new T();

                        serverUnit.SetUnit(unit);

                        loginDic.Add(uid, serverUnit);
                    }
                }
            }
        }

        List<KeyValuePair<int, ServerUnit>> kickList = null;

        Dictionary<int, ServerUnit>.Enumerator enumerator = loginDic.GetEnumerator();

        while (enumerator.MoveNext())
        {
            bool kick = enumerator.Current.Value.Update(tick);

            if (kick)
            {
                if(kickList == null)
                {
                    kickList = new List<KeyValuePair<int, ServerUnit>>();
                }

                kickList.Add(enumerator.Current);
            }
        }

        if(kickList != null)
        {
            for(int i = 0; i < kickList.Count; i++)
            {
                KeyValuePair<int, ServerUnit> pair = kickList[i];

                loginDic.Remove(pair.Key);

                logoutDic.Add(pair.Key, pair.Value.unit);
            }
        }
    }
}

