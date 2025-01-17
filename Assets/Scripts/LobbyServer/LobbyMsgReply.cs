﻿using System;
using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf;
// https://blog.csdn.net/u014308482/article/details/52958148
using Protobuf.Lobby;
using PlayerEnter = Protobuf.Lobby.PlayerEnter;
using PlayerEnterReply = Protobuf.Lobby.PlayerEnterReply;

// https://github.com/LitJSON/litjson
public class LobbyMsgReply
{
    private static SocketAsyncEventArgs _args;
    
    #region 消息分发

    /// <summary>
    /// 处理服务器接收到的消息
    /// </summary>
    /// <param name="args"></param>
    /// <param name="content"></param>
    public static void ProcessMsg(SocketAsyncEventArgs args, byte[] bytes, int size)
    {
        try
        {
            if (size < 4)
            {
                ServerLobbyManager.Instance.Log($"ProcessMsg Error - invalid data size:{size}");
                return;
            }
            _args = args;
            
            byte[] recvData = new byte[size - 4];
            Array.Copy(bytes, 4, recvData, 0, size - 4);

            // 每接收到任意一条消息,都刷新心跳时间,而不仅仅是心跳消息
            HEART_BEAT(null);

            int msgId = ParseMsgId(bytes);
            switch ((LOBBY) msgId)
            {
                case LOBBY.PlayerEnter:
                    PLAYER_ENTER(recvData);
                    break;
                case LOBBY.PlayerLeave:
                    PLAYER_LEAVE(recvData);
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
    
    private static int ParseMsgId(byte[] bytes)
    {
        byte[] recvHeader = new byte[4];
        Array.Copy(bytes, 0, recvHeader, 0, 4);
        int msgId = BitConverter.ToInt32(recvHeader, 0);
        return msgId;
    }
    
    #endregion
    
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
                ServerLobbyManager.Instance.Log($"MSG：Create new user successful! Account:<{input.Account}> - TokenId:<{input.TokenId}>"); // 创建新用户成功！
            }
            else
            {
                ServerLobbyManager.Instance.Log($"MSG：New user create failed! Account:<{input.Account}> - TokenId:<{input.TokenId}>"); // 新用户创建失败！
            }
        }
        else
        {
            ret = true;
        }

        if (ret)
        {
            //检测是否重复登录,如果发现曾经有人登录,则将前面的人踢掉
            var alreadyLoggedIn = ServerLobbyManager.Instance.FindDuplicatedPlayer(input.TokenId);
            if (alreadyLoggedIn != null)
            {
                PlayerInfo oldPlayer = ServerLobbyManager.Instance.GetPlayer(alreadyLoggedIn);
                if (oldPlayer != null)
                {
                    ServerLobbyManager.Instance.RemovePlayer(alreadyLoggedIn);
                    PlayerLeaveReply output2 = new PlayerLeaveReply()
                    {
                        TokenId = oldPlayer.Enter.TokenId,
                        IsKicked = true,
                        Ret = true,
                    };
                    ServerLobbyManager.Instance.SendMsg(alreadyLoggedIn, LOBBY_REPLY.PlayerLeaveReply, output2.ToByteArray());
                    string msg = "Kicked out the same user that logged in previously."; // 踢掉之前登录的本用户.
                    ServerLobbyManager.Instance.Log($"MSG: PLAYER_ENTER WARNING - " + msg + $" - {oldPlayer.Enter.Account}");
                }
            }
                
            PlayerInfo pi = new PlayerInfo()
            {
                Enter = input,
                IsOnLine = true,
                IsInRoom = false,
                HasRoom = false,
            };
            
            ServerLobbyManager.Instance.AddPlayer(_args, pi);
        }

        {
            PlayerEnterReply output = new PlayerEnterReply()
            {
                Ret = true,
            };
            // 返回登录成功消息
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.PlayerEnterReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log($"MSG: PLAYER_ENTER OK - Old user logged in successful! Account:<{input.Account}> - TokenId:<{input.TokenId}>"); // 老用户登录成功！
        }
    }

    private static void PLAYER_LEAVE(byte[] bytes)
    {
        PlayerLeave input = PlayerLeave.Parser.ParseFrom(bytes);
        PlayerInfo pi = ServerLobbyManager.Instance.GetPlayer(_args);
        if (pi == null)
        {
            string msg = $"Do not find myself!"; // 没有找到自己!
            ServerLobbyManager.Instance.Log("LobbyMsgReply PLAYER_LEAVE Error - "+msg);
            return;
        }
        if (input.TokenId != pi.Enter.TokenId)
        {
            string msg = $"It's not me that leaves, it must be yourself!"; // 离开的不是自己, 必须是自己!
            ServerLobbyManager.Instance.Log("LobbyMsgReply PLAYER_LEAVE Error - "+msg);
            return;
        }
        
        ServerLobbyManager.Instance.RemovePlayer(_args);
        PlayerLeaveReply output = new PlayerLeaveReply()
        {
            TokenId = pi.Enter.TokenId,
            IsKicked = false,
            Ret = true,
        };
        ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.PlayerLeaveReply, output.ToByteArray());
        ServerLobbyManager.Instance.Log($"MSG: User leaves the lobby! Account:<{pi.Enter.Account}> - TokenId:<{pi.Enter.TokenId}>"); // 用户离开大厅！
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

        ServerLobbyManager.Instance.Log($"LobbyMsgReply ASK_ROOM_LIST - Received!");
        
        // 从redis里读取房间信息
        {
            
            AskRoomListReply output = new AskRoomListReply();
            output.Ret = true;
            string[] tableNames = ServerLobbyManager.Instance.Redis.CSRedis.Keys("MAP:*");
            ServerLobbyManager.Instance.Log($"ASK_ROOM_LIST : RoomCount:{tableNames.Length}");
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
                ServerLobbyManager.Instance.Log($"RoomInfo : {roomInfo}");
            }

            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskRoomListReply, output.ToByteArray());
        }

    }

    static void ASK_CREATE_ROOM(byte[] bytes)
    {
        AskCreateRoom input = AskCreateRoom.Parser.ParseFrom(bytes);
        RoomServerLogin theRoomServer = null;
        
        ServerLobbyManager.Instance.Log($"LobbyMsgReply ASK_CREATE_ROOM - Received!");
        
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
            ServerLobbyManager.Instance.Log("MSG: There is not enough free room-servers!"); // 没有空余的房间服务器！
        }
        else
        {
            AskCreateRoomReply output = new AskCreateRoomReply()
            {
                Ret = true,
                RoomServerAddress = theRoomServer.AddressReal, // 发给客户端的是从外部连接的地址
                RoomServerPort = theRoomServer.Port,
                MaxPlayerCount = input.MaxPlayerCount,
                RoomName = input.RoomName,
            };
            
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskCreateRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log($"MSG: Find a free room-server, you can create the room! - {theRoomServer.Address}:{theRoomServer.Port}"); // 找到空余的房间服务器，可以创建房间
        }
    }

    static void ASK_JOIN_ROOM(byte[] bytes)
    {
        AskJoinRoom input = AskJoinRoom.Parser.ParseFrom(bytes);
        RoomServerLogin theRoomServer = null;
        
        ServerLobbyManager.Instance.Log($"LobbyMsgReply ASK_JOIN_ROOM - Received!");
        
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
            ServerLobbyManager.Instance.Log("MSG: There is not enough free room-servers!"); // 没有空余的房间服务器！
        }
        else
        {
            AskJoinRoomReply output = new AskJoinRoomReply()
            {
                Ret = true,
                RoomServerAddress = theRoomServer.AddressReal, // 发给客户端的是从外部连接的地址
                RoomServerPort = theRoomServer.Port,
                RoomId = input.RoomId,
            };
            
            ServerLobbyManager.Instance.SendMsg(_args, LOBBY_REPLY.AskJoinRoomReply, output.ToByteArray());
            ServerLobbyManager.Instance.Log($"MSG: Find a free room-server, you can join the room! - {theRoomServer.Address}:{theRoomServer.Port}"); // 找到空余的房间服务器，可以加入房间
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
        ServerLobbyManager.Instance.Log($"MSG: Room-server Logged in! Address:{input.ServerName} - Address Real:{input.AddressReal} - MaxRoomCount:{input.MaxRoomCount} - MaxPlayerPerRoom:{input.MaxPlayerPerRoom}"); // 房间服务器登录成功！
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
