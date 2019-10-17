﻿using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
// https://github.com/LitJSON/litjson
using LitJson;
using Protobuf.Lobby;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    public ServerScript _server;
    public RedisManager _redis;
    public RedisManager Redis => _redis;
    
    private string receive_str;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("LobbyManager must be Singleton! 必须是单例！！！");
        }
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _server.Received += OnReceive;
        _server.Completed += OnComplete;
    }

    private void OnDestroy()
    {
        _server.Received -= OnReceive;
        _server.Completed -= OnComplete;
    }

    void OnReceive(SocketAsyncEventArgs args, byte[] content, int size)
    {
        receive_str = System.Text.Encoding.UTF8.GetString(content);
        LobbyMsgReply.ProcessMsg(args, content, size);
    }

    void OnComplete(SocketAsyncEventArgs args, SocketAction action)
    {
        switch (action)
        {
            case SocketAction.Listen:
                receive_str = $"Server started! {_server.Address}:{_server.Port}";
                Debug.Log(receive_str);
                break;
            case SocketAction.Accept:
                receive_str = $"Server accepted a client! Total Count :{_server.ClientCount}/{_server.MaxClientCount}";
                Debug.Log(receive_str);
                break;
            case SocketAction.Send:
            {
                int size = args.BytesTransferred;
                Debug.Log($"Server send a message. {size} bytes");
            }
                break;
            case SocketAction.Receive:
            {
                int size = args.BytesTransferred;
                Debug.Log($"Server receive a message. {size} bytes");
            }
                break;
            case SocketAction.Drop:
                receive_str = $"Server drop a client! Total Count :{_server.ClientCount}/{_server.MaxClientCount}";
                Debug.Log(receive_str);
                break;
            case SocketAction.Close:
                receive_str = "Server Stopped!";
                Debug.Log(receive_str);
                break;
            case SocketAction.Error:
                receive_str = System.Text.Encoding.UTF8.GetString(args.Buffer);
                Debug.LogError(receive_str);
                break;
        }
    }

    void OnGUI()
    {
        if (receive_str != null)
        {
            var style = GUILayout.Width(600);
            GUILayout.Label (receive_str, style);
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
}
