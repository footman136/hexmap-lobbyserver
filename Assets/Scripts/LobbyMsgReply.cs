using System;
using LitJson;
using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf;
// https://blog.csdn.net/u014308482/article/details/52958148
using Protobuf.Lobby;
using static MsgDefine;

// https://github.com/LitJSON/litjson
public class LobbyMsgReply
{
    public static LobbyManager _chat;

    /// <summary>
    /// 处理服务器接收到的消息
    /// </summary>
    /// <param name="args"></param>
    /// <param name="content"></param>
    public static void ProcessMsg(SocketAsyncEventArgs args, byte[] data, int size)
    {
        if (size <= 4)
        {
            Debug.Log($"ProcessMsg Error - invalid data size:{size}");
            return;
        }

        byte[] recvHeader = new byte[4];
        Array.Copy(data, 0, recvHeader, 0, 4);
        byte[] recvData = new byte[size - 4];
        Array.Copy(data, 4, recvData, 0, size - 4);

        int msgId = BitConverter.ToInt32(recvHeader,0);
        switch ((MSG) msgId)
        {
            case MSG.PLAYER_ENTER:
                PLAYER_ENTER(args, recvData);
                break;
        }
    }

    static void PLAYER_ENTER(SocketAsyncEventArgs args, byte[] data)
    {
        PlayerEnter pe = PlayerEnter.Parser.ParseFrom(data);
        Debug.Log($"MSG : PlayerEnter - Account:<{pe.Account}> - TokenId:<{pe.TokenId}>");
        
        PlayerEnterReply per = new PlayerEnterReply()
        {
            Ret = false,
        };
        // 返回消息
        _chat.SendMsg(args, MSG_REPLY.PLAYER_ENTER_REPLY, per.ToByteArray());
    }
}
