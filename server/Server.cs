using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;


internal class Server<T> where T : IUnit, new()
{
    private Socket socket;

    private LinkedList<ServerUnit<T>> noLoginList = new LinkedList<ServerUnit<T>>();

    private Dictionary<int, ServerUnit<T>> loginDic = new Dictionary<int, ServerUnit<T>>();

    private Dictionary<int, T> logoutDic = new Dictionary<int, T>();

    private int tick = 0;

    internal void Start(string _path, int _port, int _maxConnections)
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

        ServerUnit<T> serverUnit = new ServerUnit<T>();

        lock (noLoginList)
        {
            noLoginList.AddLast(serverUnit);

            serverUnit.Init(clientSocket, tick);
        }

        BeginAccept();
    }

    internal void Update()
    {
        lock (noLoginList)
        {
            tick++;

            LinkedListNode<ServerUnit<T>> node = noLoginList.First;

            while (node != null)
            {
                LinkedListNode<ServerUnit<T>> nextNode = node.Next;

                ServerUnit<T> serverUnit = node.Value;

                int uid = serverUnit.CheckLogin(tick);

                if (uid == -1)
                {
                    noLoginList.Remove(node);
                }
                else if (uid > 0)
                {
                    Console.WriteLine("One user login   uid:" + uid);

                    noLoginList.Remove(node);

                    if (loginDic.ContainsKey(uid))
                    {
                        ServerUnit<T> oldServerUnit = loginDic[uid];

                        oldServerUnit.Kick();

                        serverUnit.SetUnit(oldServerUnit.unit);

                        loginDic[uid] = serverUnit;
                    }
                    else if (logoutDic.ContainsKey(uid))
                    {
                        T unit = logoutDic[uid];

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

                node = nextNode;
            }
        }

        LinkedList<KeyValuePair<int, ServerUnit<T>>> kickList = null;

        Dictionary<int, ServerUnit<T>>.Enumerator enumerator = loginDic.GetEnumerator();

        while (enumerator.MoveNext())
        {
            bool kick = enumerator.Current.Value.Update(tick);

            if (kick)
            {
                if (kickList == null)
                {
                    kickList = new LinkedList<KeyValuePair<int, ServerUnit<T>>>();
                }

                kickList.AddLast(enumerator.Current);
            }
        }

        if (kickList != null)
        {
            LinkedList<KeyValuePair<int, ServerUnit<T>>>.Enumerator enumerator2 = kickList.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                KeyValuePair<int, ServerUnit<T>> pair = enumerator2.Current;

                loginDic.Remove(pair.Key);

                logoutDic.Add(pair.Key, pair.Value.unit);
            }
        }
    }
}

