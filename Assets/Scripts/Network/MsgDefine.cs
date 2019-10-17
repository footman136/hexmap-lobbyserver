using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson; // https://github.com/LitJSON/litjson


/// <summary>
///    ————————————————
///    版权声明：本文为CSDN博主「末零」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
///    原文链接：https://blog.csdn.net/n_moling/article/details/71480931    
/// </summary>
public class MsgDefine
{
    /// <summary>
    /// 客户端发送给服务器的消息，消息ID一般为奇数
    /// </summary>
    public enum MSG
    {
        PLAYER_ENTER = 10001,
        CHAT_MESSAGE = 11000,
    }
    
    /// <summary>
    /// 客户端发送给服务器的消息，消息ID一般为偶数
    /// </summary>
    public enum MSG_REPLY
    {
        PLAYER_ENTER_REPLY = 10002,
    }
}
