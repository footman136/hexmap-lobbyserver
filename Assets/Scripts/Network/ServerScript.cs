/// <summary>
/// Server Script.
/// Created By 蓝鸥3G 2014.08.23
/// https://www.cnblogs.com/daxiaxiaohao/p/4402063.html
/// </summary>
/// 
/// 
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Xml.Xsl;

public class ServerScript : MonoBehaviour {

    MicrosoftServer _server;

    private const int BUFF_SIZE = 1024;
    private const int PLAYER_MAX_COUNT = 128;
    [SerializeField] private  int PORT = 9999;
    byte[] ReceiveBuffer = new byte[BUFF_SIZE];

    public event Action<SocketAsyncEventArgs, byte[], int> Received;
    public event Action<SocketAsyncEventArgs, SocketAction> Completed; 
    
    public int ClientCount => _server.ClientCount;
    public int MaxClientCount => _server.MaxClientCount;
    public string Address => _server.Address;
    public int Port => _server.Port; 
    
    // Use this for initialization
    void Start()
    {
        //初始化服务器
        _server = new MicrosoftServer(PLAYER_MAX_COUNT, BUFF_SIZE, OnReceive);
        _server.Init();
        // 获得主机相关信息
        IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
        IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], PORT);
        _server.Start(localEndPoint);
        _server.Completed += OnComplete;
       
        string msg = $"Server is listening at address - {_server.Address}:{_server.Port} ...";
        Debug.Log(msg);
    }

    void OnDestroy()
    {
        _server.Completed -= OnComplete;
        _server.Stop();
        Debug.Log("Server Stopped.");
    }

    void OnReceive(SocketAsyncEventArgs args, byte[] content, int offset, int size)
    {
        try
        {
            Array.Copy(content, offset, ReceiveBuffer, 0, size);        
            Received?.Invoke(args, ReceiveBuffer, size);
        }
        catch (Exception e)
        {
            string dataStr = System.Text.Encoding.UTF8.GetString(content);
            Debug.LogError($"Server OnReceive() Exceptioon - offset:{offset} - size:{size} - data:{dataStr} - {e}");
            throw;
        }
    }

    void OnComplete(SocketAsyncEventArgs args, SocketAction action)
    {
        try
        {
            Completed?.Invoke(args, action);
        }
        catch (Exception e)
        {
            Debug.LogError($"Server OnComplete() Exception - {e}");
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
            Debug.LogError($"Server SendMsg() Exception - {e}");
            throw;
        }
    }
    public void SendMsg(SocketAsyncEventArgs args, string dataStr)
    {
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataStr);
        _server.Send(args, dataBytes, dataBytes.Length);
    }
}