using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Google.Protobuf;
// https://blog.csdn.net/u014308482/article/details/52958148
using Protobuf.Lobby;
using Random = System.Random;

// https://github.com/LitJSON/litjson
public class LobbyMsgReply
{
    private static SocketAsyncEventArgs _args;
    
    /// <summary>
    /// 处理服务器接收到的消息
    /// </summary>
    /// <param name="args"></param>
    /// <param name="content"></param>
    public static void ProcessMsg(SocketAsyncEventArgs args, byte[] data, int size)
    {
        try
        {
            if (size < 4)
            {
                Debug.Log($"ProcessMsg Error - invalid data size:{size}");
                return;
            }

            _args = args;

            byte[] recvHeader = new byte[4];
            Array.Copy(data, 0, recvHeader, 0, 4);
            byte[] recvData = new byte[size - 4];
            Array.Copy(data, 4, recvData, 0, size - 4);

            int msgId = BitConverter.ToInt32(recvHeader, 0);
            switch ((LOBBY) msgId)
            {
                case LOBBY.PlayerEnter:
                    PLAYER_ENTER(recvData);
                    break;
                case LOBBY.AskRoomList:
                    ASK_ROOM_LIST(recvData);
                    break;
                case LOBBY.RoomServerLogin:
                    ROOM_SERVER_LOGIN(recvData);
                    break;
                case LOBBY.AskCreateRoom:
                    ASK_CREATE_ROOM(recvData);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception - LobbyMsgReply - {e}");
        }
    }
    
#region 客户端消息处理
    static void PLAYER_ENTER(byte[] bytes)
    {
        PlayerEnter input = PlayerEnter.Parser.ParseFrom(bytes);

        bool ret = false;
        // 写入redis
        if (!LobbyManager.Instance.Redis.CSRedis.Exists(input.TokenId.ToString()))
        {
            ret = LobbyManager.Instance.Redis.CSRedis.HSet(input.TokenId.ToString(), "Account", input.Account);
            if (ret)
            {
                Debug.Log($"MSG：新用户创建！Account:<{input.Account}> - TokenId:<{input.TokenId}>");
            }
        }
        else
        {
            ret = true;
            Debug.Log($"MSG: 老用户登录！Account:<{input.Account}> - TokenId:<{input.TokenId}>");
        }

        if (ret)
        {
            PlayerInfo pi = new PlayerInfo()
            {
                Enter = input,
                IsOnLine = true,
                IsInRoom = false,
                HasRoom = false,
            };
            LobbyManager.Instance.Players[_args] = pi;
        }

        PlayerEnterReply output = new PlayerEnterReply()
        {
            Ret = ret,
        };
        // 返回消息
        LobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.PlayerEnterReply, output.ToByteArray());
    }

    static void ASK_ROOM_LIST(byte[] bytes)
    {
        AskRoomList input = AskRoomList.Parser.ParseFrom(bytes);
        int playerCount = 11;
        string[] names = new string[11] {"Childhood", "Playground", "战狼", "中土战争", "霍比特人的小屋","血战钢锯岭","狂怒","钢铁雄狮","信长の野望","被人遗忘的星辰大海","小丑"};

        Random rand = new Random();

        AskRoomListReply output = new AskRoomListReply();
        for (int i = 0; i < playerCount; ++i)
        {
            RoomInfo roomInfo = new RoomInfo()
            {
                Name = names[i],
                CreateTime = 1000+rand.Next(1, 999),
                RoomId = 100+rand.Next(0,99),
                PlayerCount = rand.Next(0,20),
                MaxPlayerCount = 20,
            };
            output.Rooms.Add(roomInfo);
        }
        LobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskRoomListReply, output.ToByteArray());
    }

    static void ASK_CREATE_ROOM(byte[] bytes)
    {
        AskCreateRoom input = AskCreateRoom.Parser.ParseFrom(bytes);
        RoomServerLogin theRoomServer = null; 
        // 
        foreach (var keyValue in LobbyManager.Instance.RoomServers)
        {
            RoomServerInfo roomServerInfo = keyValue.Value;
            RoomServerLogin roomServer = roomServerInfo.Login;
            if (LobbyManager.Instance.Rooms.Count < roomServer.MaxRoomCount
                && input.MaxPlayerCount < roomServer.MaxPlayerPerRoom)
            {
                theRoomServer = roomServer;
            }
        }

        if (theRoomServer == null)
        {
            AskCreateRoomReply output = new AskCreateRoomReply()
            {
                Ret = false,    
            };
            LobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskCreateRoomReply, output.ToByteArray());
            Debug.Log("MSG: 没有空余的房间服务器！");
        }
        else
        {
            AskCreateRoomReply output = new AskCreateRoomReply()
            {
                Ret = true,
                RoomServerAddress = theRoomServer.Address,
                RoomServerPort = theRoomServer.Port,
                MaxPlayerCount = input.MaxPlayerCount,
                RoomName = input.RoomName,
            };
            
            LobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskCreateRoomReply, output.ToByteArray());
            Debug.Log($"MSG: 找到空余的房间服务器 - {theRoomServer.Address}:{theRoomServer.Port}");
        }
    }
#endregion
    
#region 房间服务器消息处理
    static void ROOM_SERVER_LOGIN(byte[] bytes)
    {
        RoomServerLogin input = RoomServerLogin.Parser.ParseFrom(bytes);
        RoomServerInfo roomServerInfo = new RoomServerInfo()
        {
            Login = input,
        };
        LobbyManager.Instance.RoomServers[_args] = roomServerInfo;
        
        RoomServerLoginReply output = new RoomServerLoginReply()
        {
            Ret = true,
        };
        Debug.Log($"MSG: 房间服务器登录成功！地址:{input.ServerName} - MaxRoomCount:{input.MaxRoomCount} - MaxPlayerPerRoom:{input.MaxPlayerPerRoom}");
        // 返回消息
        LobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.RoomServerLoginReply, output.ToByteArray());
    }
#endregion
}
