using System;
using LitJson;
using UnityEngine;
using System.Net.Sockets;

// https://github.com/LitJSON/litjson
public class MsgExplain
{
    public enum CMD
    {
        PLAYER_ENTER=10001,
        NORMAL_MESSAGE = 11000,
    }
    
    public static ChatManager _chat;

    static void ErrorCommand(int cmdInput, int cmdShouldBe)
    {
        Debug.LogError($"MsgExplain Error - This Cmd is not my Cmd!{cmdInput}/{cmdShouldBe}");
    }
    
    public static void PLAYER_ENTER(SocketAsyncEventArgs args, JsonData dataJson)
    {
        int cmdId = Int32.Parse(dataJson["cmd_id"].ToString());
        if (cmdId != (int)CMD.PLAYER_ENTER)
        {
            ErrorCommand(cmdId, (int)CMD.NORMAL_MESSAGE);
            return;
        }
        
        _chat.SendMsg(args, dataJson["username"].ToString());
    }
    public static void NORMAL_MESSAGE(SocketAsyncEventArgs args, JsonData dataJson)
    {
        int cmdId = Int32.Parse(dataJson["cmd_id"].ToString());
        if (cmdId != (int)CMD.NORMAL_MESSAGE)
        {
            ErrorCommand(cmdId, (int)CMD.NORMAL_MESSAGE);
            return;
        }
        _chat.SendMsg(args, dataJson["message"].ToString());
    }
}
