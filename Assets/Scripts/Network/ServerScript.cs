/// <summary>
/// Server Script.
/// Created By 蓝鸥3G 2014.08.23
/// https://www.cnblogs.com/daxiaxiaohao/p/4402063.html
/// </summary>
/// 
/// 
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class ServerScript : MonoBehaviour {

    MicrosoftServer _server;

    private const int BUFF_SIZE = 1024;
    private const int PLAYER_MAX_COUNT = 128;
    [SerializeField] private  int PORT = 9999;
    byte[] ReceiveBuffer = new byte[BUFF_SIZE];

    public event Action<SocketAsyncEventArgs, byte[], int> Received;
    public event Action<SocketAsyncEventArgs, ServerSocketAction> Completed; 
    
    public int ClientCount => _server.ClientCount;
    public int MaxClientCount => _server.MaxClientCount;

    public string Address
    {
        set { _server.Address = value; }
        get { return _server.Address; }
    }

    public int Port
    {
        set { _server.Port = value; }
        get { return _server.Port; }
    }

    public bool IsReady;

    void Awake()
    {
        //初始化服务器
        _server = new MicrosoftServer(PLAYER_MAX_COUNT, BUFF_SIZE, OnReceive);
        _server.Init();
    }
    
    // Use this for initialization
    void Start()
    {
        // 获得主机相关信息
        IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
        IPAddress ipAddress = null;
        for(int i=0; i<addressList.Length; ++i)
        {
            var addr = addressList[i];
            Log($"Address {i} : {addr.ToString()}");
        }

        foreach (var addr in addressList)
        {
            if (addr.AddressFamily.ToString() == "InterNetwork")
            {
                ipAddress = addr;
                break;
            }
        }

        { // 在外网云服务器上, 无法像上面那样获取服务器地址,只能用0.0.0.0来作为地址监听. Nov.19.2019. Liu Gang.
            //string addrStr = "0.0.0.0";
            IPAddress.TryParse(_server.Address, out ipAddress);
        }

        if (ipAddress != null)
        {
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _server.Port);
            //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            _server.Start(localEndPoint);
            _server.Completed += OnComplete;

            string msg = $"Server is listening at address - {_server.Address}:{_server.Port} ...";
            Log(msg);
            IsReady = true;
        }
        else
        {
            string msg = $"Server address is not found!";
            Log(msg);
            IsReady = false;
        }
    }

    void OnDestroy()
    {
        _server.Completed -= OnComplete;
        //_server.Stop();
        Log("Server Stopped.");
    }

    private void OnReceive(SocketAsyncEventArgs args, byte[] content, int offset, int size)
    {
        try
        {
            Array.Copy(content, offset, ReceiveBuffer, 0, size);        
            Received?.Invoke(args, ReceiveBuffer, size);
        }
        catch (Exception e)
        {
            string dataStr = System.Text.Encoding.UTF8.GetString(content);
            Debug.LogError($"ServerSciprt OnReceive Exceptioon - offset:{offset} - size:{size} - data:{dataStr} - {e}");
            throw;
        }
    }

    private void OnComplete(SocketAsyncEventArgs args, ServerSocketAction action)
    {
        try
        {
            Completed?.Invoke(args, action);
        }
        catch (Exception e)
        {
            Debug.LogError($"ServerScript OnComplete Exception - {e}");
            throw;
        }
    }

    public void SendMsg(SocketAsyncEventArgs args, byte[] data, int size)
    {
        try
        {
            _server.Send(args, data, size);
        }
        catch (Exception e)
        {
            Debug.LogError($"ServerScript SendMsg Exception - size:{size} - {e}");
            throw;
        }
    }
    public void SendMsg(SocketAsyncEventArgs args, string dataStr)
    {
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataStr);
        _server.Send(args, dataBytes, dataBytes.Length);
    }

    public void Log(string msg)
    {
        _server.Log(msg);
    }

    public void CloseASocket(SocketAsyncEventArgs args)
    {
        _server.CloseASocket(args);
    }
}