using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo
{
    public long RoomId;
    public string Name;

    public int MaxPlayerCount;
    public int PlayerCount;

    // 房间最大生命周期（秒）
    public int MaxLifeTime;
    public DateTime CreateTime;
    
}
