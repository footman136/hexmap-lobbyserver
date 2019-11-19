using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
// https://github.com/LitJSON/litjson
using LitJson;
using Protobuf.Lobby;

public class ServerLobbyManager : MonoBehaviour
{
    public static ServerLobbyManager Instance { get; private set; }
    
    [Header("Basic Attributes"), Space(5)]
    public ServerScript _server;
    public RedisManager Redis;
    
    [Space(), Header("Debug"), Space(5)]
    public bool IsCheckHeartBeat;
    
    [HideInInspector]
    public CsvDataManager CsvDataManager;
    
    private string receive_str;

    // 玩家的集合，Key是玩家的TokenId，因为真正的账号系统我们不一定能够获得玩家的账号名
    public Dictionary<SocketAsyncEventArgs, PlayerInfo> Players { set; get; }
    
    // 房间服务器的集合，Key是RoomServer的唯一ID
    public Dictionary<SocketAsyncEventArgs, RoomServerInfo> RoomServers { set; get; }

    // 房间的集合，Key是房间的唯一ID
    public Dictionary<long, RoomInfo> Rooms { set; get; } 
    
    private const float _heartBeatTimeInterval = 45f; // 心跳时间间隔,服务器检测用的间隔比客户端实际间隔要多一些, LobbyServer的间隔要再多一些,因为客户端与大厅之间的消息少

    #region 初始化
    
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("LobbyManager is Singleton! Cannot be created again!");
        }
        Instance = this;
        Players = new Dictionary<SocketAsyncEventArgs, PlayerInfo>();
        Rooms = new Dictionary<long, RoomInfo>();
        RoomServers = new Dictionary<SocketAsyncEventArgs, RoomServerInfo>();
        
        // 读取数据表
        CsvDataManager = gameObject.AddComponent<CsvDataManager>();
        CsvDataManager.LoadDataAll();
        
        var csv = CsvDataManager.Instance.GetTable("server_config_lobby");
        if (csv != null)
        { // 房间服务器监听地址, 按理说监听地址, 不能由外部指定, 但是对于云服务器, 是没有本地地址的, 只能外部指定
            _server.Address = csv.GetValue(1, "LobbyServerAddress");
            _server.Port = csv.GetValueInt(1, "LobbyServerPort");
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _server.Received += OnReceive;
        _server.Completed += OnComplete;
        
        StartCoroutine(WaitForReady());
    }

    private void OnDestroy()
    {
        _server.Received -= OnReceive;
        _server.Completed -= OnComplete;
    }

    IEnumerator WaitForReady()
    {
        while (!_server.IsReady)
        {
            yield return null;
        }
        receive_str = $"Server started! {_server.Address}:{_server.Port}";
        if (IsCheckHeartBeat)
        {
            StartCheckHeartBeat();
        }
    }

    void OnGUI()
    {
        if (receive_str != null)
        {
            var style = GUILayout.Width(600) ;
            GUILayout.Label (receive_str, style);
            GUILayoutOption[] style2 = new GUILayoutOption[2] {style, GUILayout.Height(60)};
            string msg = $"----RoomServer Count:{RoomServers.Count} - Player Count:{Players.Count}/{_server.MaxClientCount} - Room Count:{Rooms.Count}";
            GUILayout.Label (msg, style2);
        }
    }
    
    public void Log(string msg)
    {
        receive_str = msg;
        _server.Log(msg);
    }
    
    #endregion

    #region 检测心跳

    private void StartCheckHeartBeat()
    {
        InvokeRepeating(nameof(CheckHeartBeat), _heartBeatTimeInterval, _heartBeatTimeInterval);
    }

    private void StopCheckHeartBeat()
    {
        CancelInvoke(nameof(CheckHeartBeat));
    }

    private void CheckHeartBeat()
    {
        var now = DateTime.Now;
        List<SocketAsyncEventArgs> delPlayerList = new List<SocketAsyncEventArgs>();
        List<SocketAsyncEventArgs> delRoomServerList = new List<SocketAsyncEventArgs>();
        foreach (var keyValue in Players)
        {
            var ts = now - keyValue.Value.HeartBeatTime;
            if (ts.TotalSeconds > _heartBeatTimeInterval)
            { // 该客户端(玩家)超时没有心跳了,干掉
                delPlayerList.Add(keyValue.Key);
                _server.Log($"长时间没有检测到心跳,将客户端踢出! - {keyValue.Value.Enter.Account}");
            }
        }
        foreach (var args in delPlayerList)
        {
            _server.CloseASocket(args);
        }
        foreach (var keyValue in RoomServers)
        {
            var ts = now - keyValue.Value.HeartBeatTime;
            if (ts.TotalSeconds > _heartBeatTimeInterval)
            { // 该客户端(房间服务器)超时没有心跳了,干掉
                delRoomServerList.Add(keyValue.Key);
            }
        }
        foreach (var args in delRoomServerList)
        {
            _server.CloseASocket(args);
        }
    }
    
    #endregion

    #region 收发消息
    
    void OnReceive(SocketAsyncEventArgs args, byte[] content, int size)
    {
        receive_str = System.Text.Encoding.UTF8.GetString(content);
        LobbyMsgReply.ProcessMsg(args, content, size);
    }

    void OnComplete(SocketAsyncEventArgs args, ServerSocketAction action)
    {
        switch (action)
        {
            case ServerSocketAction.Listen:
                // 暂时不会有代码走到这里。
                Log($"Server started! {_server.Address}:{_server.Port}");
                break;
            case ServerSocketAction.Accept:
                Log($"Server accepted a client! Total Count :{_server.ClientCount}/{_server.MaxClientCount}");
                break;
            case ServerSocketAction.Send:
            {
                int size = args.BytesTransferred;
                Log($"Server send a message. {size} bytes");
            }
                break;
            case ServerSocketAction.Receive:
            {
                int size = args.BytesTransferred;
                Log($"Server receive a message. {size} bytes");
            }
                break;
            case ServerSocketAction.Drop:
                DropAClient(args);
                Log($"Server drop a client! Total Count :{_server.ClientCount}/{_server.MaxClientCount}");
                break;
            case ServerSocketAction.Close:
                Log("Server Stopped!");
                break;
            case ServerSocketAction.Error:
                receive_str = System.Text.Encoding.UTF8.GetString(args.Buffer);
                Debug.LogError(receive_str);
                break;
        }
    }

    /// <summary>
    /// 新增的发送消息函数，增加了消息ID，会把前面的消息ID（4字节）和后面的消息内容组成一个包再发送
    /// </summary>
    /// <param name="msgId">消息ID，注意这是服务器返回给客户端的消息</param>
    /// <param name="???"></param>
    public void SendMsg(SocketAsyncEventArgs args, LOBBY_REPLY msgId, byte[] data)
    {
        byte[] sendData = new byte[data.Length + 4];
        byte[] sendHeader = System.BitConverter.GetBytes((int)msgId);
        
        Array.Copy(sendHeader, 0, sendData, 0, 4);
        Array.Copy(data, 0, sendData, 4, data.Length);
        _server.SendMsg(args, sendData, sendData.Length);
    }

    public void DropAClient(SocketAsyncEventArgs args)
    {
        if (Players.ContainsKey(args))
        {
            Log($"MSG: 玩家离开大厅服务器 - {Players[args].Enter.Account} - PlayerCount:{Players.Count-1}/{_server.MaxClientCount}");
            Players.Remove(args);
        }
        else if(RoomServers.ContainsKey(args))
        {
            Log($"MSG: 房间服务器离开大厅服务器 - {RoomServers[args].Login.ServerName} - RoomServerCount:{RoomServers.Count-1}");
            // 该房间服务器所带来的房间数也都要清理一下
            RemoveRoomsInARoomServer(args);
            RoomServers.Remove(args);
        }
        else
        {
            Log("MSG: Server - Remove Player or RoomServer failed - Player or RoomServer not found!");
        }
    }
    
    #endregion
    
    #region 玩家/房间服务器/房间-操作
    public PlayerInfo GetPlayer(SocketAsyncEventArgs args)
    {
        if (Players.ContainsKey(args))
        {
            PlayerInfo pi = Players[args];
            return pi;
        }

        return null;
    }

    public void AddPlayer(SocketAsyncEventArgs args, PlayerInfo pi)
    {
        Players[args] = pi;
    }

    public void RemovePlayer(SocketAsyncEventArgs args)
    {
        if (Players.ContainsKey(args))
        {
            Players.Remove(args);
        }
        else
        {
            _server.Log("ServerLobbyManager RemovePlayer Error - Player Info not found!");
        }
    }

    public RoomServerInfo GetRoomServer(SocketAsyncEventArgs args)
    {
        if (RoomServers.ContainsKey(args))
        {
            RoomServerInfo rsi = RoomServers[args];
            return rsi;
        }

        return null;
    }

    public void RemoveRoomsInARoomServer(SocketAsyncEventArgs args)
    {
        var rsi = GetRoomServer(args);
        if (rsi != null)
        {
            foreach (var roomId in rsi.Rooms)
            {
                Rooms.Remove(roomId);
            }

            rsi.Rooms.Clear();
        }
    }

    public SocketAsyncEventArgs FindDuplicatedPlayer(long tokenId)
    {
        foreach (var keyValue in Players)
        {
            var pe = keyValue.Value;
            if (pe.Enter.TokenId == tokenId)
            {
                return keyValue.Key;
            }
        }

        return null;
    }

    public void AddRoom(SocketAsyncEventArgs args, RoomInfo roomInfo)
    {
        Rooms[roomInfo.RoomId] = roomInfo;
        var rsi = GetRoomServer(args);
        rsi?.Rooms.Add(roomInfo.RoomId);
    }

    public void RemoveRoom(SocketAsyncEventArgs args, long roomId)
    {
        var rsi = GetRoomServer(args);
        rsi?.Rooms.Remove(roomId);
        Rooms.Remove(roomId);
    }
    
    #endregion

}
