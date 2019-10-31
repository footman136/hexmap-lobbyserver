using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf.Lobby;
using UnityEngine;
                                             
public class RoomServerInfo
{
    public RoomServerLogin Login;
    public DateTime HeartBeatTime;

    public List<long> Rooms = new List<long>(); // 本房间服务器所开启的所有房间的ID
}
