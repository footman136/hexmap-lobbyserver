using System;
using LitJson;
using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf;
// https://blog.csdn.net/u014308482/article/details/52958148
using Protobuf.Lobby;

// https://github.com/LitJSON/litjson
public class LobbyMsgReply
{
    /// <summary>
    /// 处理服务器接收到的消息
    /// </summary>
    /// <param name="args"></param>
    /// <param name="content"></param>
    public static void ProcessMsg(SocketAsyncEventArgs args, byte[] data, int size)
    {
        try
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

            int msgId = BitConverter.ToInt32(recvHeader, 0);
            switch ((LOBBY) msgId)
            {
                case LOBBY.PlayerEnter:
                    PLAYER_ENTER(args, recvData);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception - LobbyMsgReply - {e}");
        }
    }

    static void PLAYER_ENTER(SocketAsyncEventArgs args, byte[] data)
    {
        PlayerEnter pe = PlayerEnter.Parser.ParseFrom(data);

        // 写入redis
        if (!LobbyManager.Instance.Redis.CSRedis.Exists(pe.Account))
        {
            LobbyManager.Instance.Redis.CSRedis.HSet(pe.Account, "TokenId", pe.TokenId.ToString());
            Debug.Log($"MSG： 新用户创建！Account:<{pe.Account}> - TokenId:<{pe.TokenId}>");
        }
        else
        {
            Debug.Log($"MSG: 老用户登录！Account:<{pe.Account}> - TokenId:<{pe.TokenId}>");
        }

        PlayerEnterReply per = new PlayerEnterReply()
        {
            Ret = false,
        };
        // 返回消息
        LobbyManager.Instance.SendMsg(args, LOBBY_REPLY.PlayerEnterReply, per.ToByteArray());
    }
}
