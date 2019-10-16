using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using LitJson; // https://github.com/LitJSON/litjson


public class ChatManager : MonoBehaviour
{
    public ServerScript _server;
    
    private string receive_str;
    
    // Start is called before the first frame update
    void Start()
    {
        _server.Received += OnReceive;
        _server.Completed += OnComplete;

        MsgExplain._chat = this;
    }

    private void OnDestroy()
    {
        _server.Received -= OnReceive;
        _server.Completed -= OnComplete;
    }

    void OnReceive(SocketAsyncEventArgs args, byte[] content, int size)
    {
        receive_str = System.Text.Encoding.UTF8.GetString(content);
        ProcessMsg(args, content, size);
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
    /// 处理服务器接收到的消息
    /// </summary>
    /// <param name="args"></param>
    /// <param name="content"></param>
    void ProcessMsg(SocketAsyncEventArgs args, byte[] content, int size)
    {
        var message = System.Text.Encoding.UTF8.GetString (content, 0, size);
        var dataJson = JsonMapper.ToObject(message);
        int cmdId = Int32.Parse(dataJson["cmd_id"].ToString());
        switch ((MsgExplain.CMD)cmdId)
        {
            case MsgExplain.CMD.PLAYER_ENTER:
                MsgExplain.PLAYER_ENTER(args, dataJson);
                break;
            case MsgExplain.CMD.NORMAL_MESSAGE:
                MsgExplain.NORMAL_MESSAGE(args, dataJson);
                break;
        }
    }

    public void SendMsg(SocketAsyncEventArgs args, string message)
    {
        _server.SendMsg(args, message);
    }
}
