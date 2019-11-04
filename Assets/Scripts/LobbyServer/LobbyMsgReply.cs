using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Google.Protobuf;
// https://blog.csdn.net/u014308482/article/details/52958148
using Protobuf.Lobby;
using UnityEngine.Experimental.PlayerLoop;
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
                ServerLobbyManager.Instance.Log($"ProcessMsg Error - invalid data size:{size}");
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
                case LOBBY.HeartBeat:
                    HEART_BEAT(recvData);
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
                case LOBBY.AskJoinRoom:
                    ASK_JOIN_ROOM(recvData);
                    break;
                case LOBBY.DestroyRoom:
                    DESTROY_ROOM(recvData);
                    break;
                case LOBBY.UpdateRoomInfo:
                    UPDATE_ROOM_INFO(recvData);
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
        string tableName = $"ACCOUNT:{input.TokenId.ToString()}";
        if (!ServerLobbyManager.Instance.Redis.CSRedis.Exists(tableName))
        {
            ret = ServerLobbyManager.Instance.Redis.CSRedis.HSet(tableName, "Account", input.Account);
            if (ret)
            {
                ret = ServerLobbyManager.Instance.Redis.CSRedis.HSet(tableName, "TokenId", input.TokenId);
                ServerLobbyManager.Instance.Log($"MSG：创建新用户！Account:<{input.Account}> - TokenId:<{input.TokenId}>");
            }
            else
            {
                ServerLobbyManager.Instance.Log($"MSG：新用户创建失败！Account:<{input.Account}> - TokenId:<{input.TokenId}>");
            }
        }
        else
        {
            ret = true;
            ServerLobbyManager.Instance.Log($"MSG: 老用户登录！Account:<{input.Account}> - TokenId:<{input.TokenId}>");
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
            ServerLobbyManager.Instance.Players[_args] = pi;
        }

        PlayerEnterReply output = new PlayerEnterReply()
        {
            Ret = ret,
        };
        // 返回消息
        ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.PlayerEnterReply, output.ToByteArray());
    }

    private static void HEART_BEAT(byte[] byts)
    {
        var pi = ServerLobbyManager.Instance.GetPlayer(_args);
        if (pi != null)
        {
            pi.HeartBeatTime = DateTime.Now;
        }
        else
        {
            var rsi = ServerLobbyManager.Instance.GetRoomServer(_args);
            if(rsi != null)
            {
                rsi.HeartBeatTime = DateTime.Now;
            }
                
        }
    }

    static void ASK_ROOM_LIST(byte[] bytes)
    {
        AskRoomList input = AskRoomList.Parser.ParseFrom(bytes);
        
        // 从redis里读取房间信息
        {
            
            AskRoomListReply output = new AskRoomListReply();
            output.Ret = true;
            string[] tableNames = ServerLobbyManager.Instance.Redis.CSRedis.Keys("MAP:*");
            foreach (string tableName in tableNames)
            {
                long createrId = ServerLobbyManager.Instance.Redis.CSRedis.HGet<long>(tableName, "Creator");
                RoomInfo roomInfo = new RoomInfo()
                {
                    RoomId = ServerLobbyManager.Instance.Redis.CSRedis.HGet<long>(tableName, "RoomId"),
                    MaxPlayerCount = ServerLobbyManager.Instance.Redis.CSRedis.HGet<int>(tableName, "MaxPlayerCount"),
                    RoomName = ServerLobbyManager.Instance.Redis.CSRedis.HGet<string>(tableName, "RoomName"),
                    Creator = createrId,
                    IsRunning = false,
                };
                roomInfo.IsRunning = ServerLobbyManager.Instance.Rooms.ContainsKey(roomInfo.RoomId);
                if (roomInfo.IsRunning)
                {
                    roomInfo.CurPlayerCount = ServerLobbyManager.Instance.Rooms[roomInfo.RoomId].CurPlayerCount;
                }

                output.Rooms.Add(roomInfo);
            }

            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskRoomListReply, output.ToByteArray());
        }

    }

    static void ASK_CREATE_ROOM(byte[] bytes)
    {
        AskCreateRoom input = AskCreateRoom.Parser.ParseFrom(bytes);
        RoomServerLogin theRoomServer = null; 
        // 
        foreach (var keyValue in ServerLobbyManager.Instance.RoomServers)
        {
            RoomServerInfo roomServerInfo = keyValue.Value;
            RoomServerLogin roomServer = roomServerInfo.Login;
            if (ServerLobbyManager.Instance.Rooms.Count < roomServer.MaxRoomCount
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
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskCreateRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log("MSG: 没有空余的房间服务器！");
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
            
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskCreateRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log($"MSG: 找到空余的房间服务器，可以创建房间 - {theRoomServer.Address}:{theRoomServer.Port}");
        }
    }

    static void ASK_JOIN_ROOM(byte[] bytes)
    {
        AskJoinRoom input = AskJoinRoom.Parser.ParseFrom(bytes);
        RoomServerLogin theRoomServer = null;
        //
        foreach (var keyValue in ServerLobbyManager.Instance.RoomServers)
        {
            RoomServerInfo roomServerInfo = keyValue.Value;
            RoomServerLogin roomServer = roomServerInfo.Login;
            if (ServerLobbyManager.Instance.Rooms.Count < roomServer.MaxRoomCount
                && input.MaxPlayerCount < roomServer.MaxPlayerPerRoom)
            {
                theRoomServer = roomServer;
            }
        }
        
        if (theRoomServer == null)
        {
            AskJoinRoomReply output = new AskJoinRoomReply()
            {
                Ret = false,    
            };
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskJoinRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log("MSG: 没有空余的房间服务器！");
        }
        else
        {
            AskJoinRoomReply output = new AskJoinRoomReply()
            {
                Ret = true,
                RoomServerAddress = theRoomServer.Address,
                RoomServerPort = theRoomServer.Port,
                RoomId = input.RoomId,
            };
            
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskJoinRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log($"MSG: 找到空余的房间服务器，可以加入房间 - {theRoomServer.Address}:{theRoomServer.Port}");
        }
    }

    static void DESTROY_ROOM(byte[] bytes)
    {
        DestroyRoom input = DestroyRoom.Parser.ParseFrom(bytes);
        string tableName = $"MAP:{input.RoomId}";
        bool ret = false;
        string roomName = "";
        if (ServerLobbyManager.Instance.Redis.CSRedis.Exists(tableName))
        {
            roomName = ServerLobbyManager.Instance.Redis.CSRedis.HGet<string>(tableName, "RoomName");
            ServerLobbyManager.Instance.Redis.CSRedis.Del(tableName);
            ret = true;
        }

        DestroyRoomReply output = new DestroyRoomReply()
        {
            Ret = ret,
            RoomName = roomName,
        };
        ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.DestroyRoomReply, output.ToByteArray());
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
        ServerLobbyManager.Instance.RoomServers[_args] = roomServerInfo;
        
        RoomServerLoginReply output = new RoomServerLoginReply()
        {
            Ret = true,
        };
        // 返回消息
        ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.RoomServerLoginReply, output.ToByteArray());
        ServerLobbyManager.Instance.Log($"MSG: 房间服务器登录成功！地址:{input.ServerName} - MaxRoomCount:{input.MaxRoomCount} - MaxPlayerPerRoom:{input.MaxPlayerPerRoom}");
    }

    static void UPDATE_ROOM_INFO(byte[] bytes)
    {
        UpdateRoomInfo input = UpdateRoomInfo.Parser.ParseFrom(bytes);
        if (!input.IsRemove)
        {
            RoomInfo roomInfo = new RoomInfo()
            {
                RoomName = input.RoomName,
                RoomId = input.RoomId,
                CurPlayerCount = input.CurPlayerCount,
                MaxPlayerCount = input.MaxPlayerCount,
                IsRunning = input.IsRunning,
                Creator = input.Creator,
            };
            ServerLobbyManager.Instance.Rooms[input.RoomId] = roomInfo;
            var rsi = ServerLobbyManager.Instance.GetRoomServer(_args);
            if (rsi != null)
            {
                rsi.Rooms.Add(input.RoomId);
            }
        }
        else
        {// 删除这个房间
            ServerLobbyManager.Instance.Rooms.Remove(input.RoomId);
        }

        UpdateRoomInfoReply output = new UpdateRoomInfoReply()
        {
            Ret = true,
        };
        ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.UpdateRoomInfoReply, output.ToByteArray());
    }
#endregion
}
