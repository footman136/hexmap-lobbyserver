// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: MsgDefineRoom.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Protobuf.Room {

  /// <summary>Holder for reflection information generated from MsgDefineRoom.proto</summary>
  public static partial class MsgDefineRoomReflection {

    #region Descriptor
    /// <summary>File descriptor for MsgDefineRoom.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MsgDefineRoomReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNNc2dEZWZpbmVSb29tLnByb3RvEg1Qcm90b2J1Zi5Sb29tKtYECgRST09N",
            "EgwKCE1zZ1N0YXJ0EAASDwoJSGVhcnRCZWF0EKCcARIRCgtQbGF5ZXJFbnRl",
            "chChnAESDwoJRW50ZXJSb29tEKOcARIPCglMZWF2ZVJvb20QpZwBEg8KCVVw",
            "bG9hZE1hcBCnnAESEQoLRG93bmxvYWRNYXAQqZwBEhEKC0Rlc3Ryb3lSb29t",
            "EKucARIUCg5Eb3dubG9hZENpdGllcxC1nAESDQoHQ2l0eUFkZBC2nAESEAoK",
            "Q2l0eVJlbW92ZRC3nAESFAoORG93bmxvYWRBY3RvcnMQv5wBEg4KCEFjdG9y",
            "QWRkEMGcARIRCgtBY3RvclJlbW92ZRDCnAESDwoJQWN0b3JNb3ZlEMWcARIS",
            "CgxBY3RvckFpU3RhdGUQx5wBEhQKDlVwZGF0ZUFjdG9yUG9zEMmcARIVCg9V",
            "cGRhdGVBY3RvckluZm8Qy5wBEhIKDEFjdG9yUGxheUFuaRDNnAESEAoKVHJ5",
            "Q29tbWFuZBDPnAESEgoMSGFydmVzdFN0YXJ0ENOcARIRCgtIYXJ2ZXN0U3Rv",
            "cBDVnAESFQoPRG93bmxvYWRSZXNDZWxsENecARIPCglVcGRhdGVSZXMQ2ZwB",
            "EhcKEVVwZGF0ZUFjdGlvblBvaW50ENucARIQCgpGaWdodFN0YXJ0EN2cARIP",
            "CglGaWdodFN0b3AQ35wBEhAKClNwcmF5Qmxvb2QQ4ZwBEhAKCkFtbW9TdXBw",
            "bHkQ45wBEhQKDkNoYW5nZUFpUmlnaHRzEOecARIWChBBY3RvckFpU3RhdGVI",
            "aWdoEOmcASrhBQoKUk9PTV9SRVBMWRIRCg1Nc2dTdGFydFJlcGx5EAASFgoQ",
            "UGxheWVyRW50ZXJSZXBseRCinAESFAoORW50ZXJSb29tUmVwbHkQpJwBEhQK",
            "DkxlYXZlUm9vbVJlcGx5EKacARIUCg5VcGxvYWRNYXBSZXBseRConAESFgoQ",
            "RG93bmxvYWRNYXBSZXBseRCqnAESFgoQRGVzdHJveVJvb21SZXBseRCsnAES",
            "GQoTRG93bmxvYWRDaXRpZXNSZXBseRC2nAESEgoMQ2l0eUFkZFJlcGx5ELic",
            "ARIVCg9DaXR5UmVtb3ZlUmVwbHkQupwBEhkKE0Rvd25sb2FkQWN0b3JzUmVw",
            "bHkQwJwBEhMKDUFjdG9yQWRkUmVwbHkQwpwBEhYKEEFjdG9yUmVtb3ZlUmVw",
            "bHkQxJwBEhQKDkFjdG9yTW92ZVJlcGx5EMacARIXChFBY3RvckFpU3RhdGVS",
            "ZXBseRDInAESGQoTVXBkYXRlQWN0b3JQb3NSZXBseRDKnAESGgoUVXBkYXRl",
            "QWN0b3JJbmZvUmVwbHkQzJwBEhcKEUFjdG9yUGxheUFuaVJlcGx5EM6cARIV",
            "Cg9UcnlDb21tYW5kUmVwbHkQ0JwBEhcKEUhhcnZlc3RTdGFydFJlcGx5ENSc",
            "ARIWChBIYXJ2ZXN0U3RvcFJlcGx5ENacARIaChREb3dubG9hZFJlc0NlbGxS",
            "ZXBseRDYnAESFAoOVXBkYXRlUmVzUmVwbHkQ2pwBEhwKFlVwZGF0ZUFjdGlv",
            "blBvaW50UmVwbHkQ3JwBEhUKD0ZpZ2h0U3RhcnRSZXBseRDenAESFAoORmln",
            "aHRTdG9wUmVwbHkQ4JwBEhUKD1NwcmF5Qmxvb2RSZXBseRDinAESFQoPQW1t",
            "b1N1cHBseVJlcGx5EOScARIZChNDaGFuZ2VBaVJpZ2h0c1JlcGx5EOicARIb",
            "ChVBY3RvckFpU3RhdGVIaWdoUmVwbHkQ6pwBYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Protobuf.Room.ROOM), typeof(global::Protobuf.Room.ROOM_REPLY), }, null, null));
    }
    #endregion

  }
  #region Enums
  /// <summary>
  /// 客户端发送到房间
  /// </summary>
  public enum ROOM {
    /// <summary>
    /// proto3枚举的第一个成员必须是0
    /// </summary>
    [pbr::OriginalName("MsgStart")] MsgStart = 0,
    /// <summary>
    /// 心跳
    /// </summary>
    [pbr::OriginalName("HeartBeat")] HeartBeat = 20000,
    [pbr::OriginalName("PlayerEnter")] PlayerEnter = 20001,
    [pbr::OriginalName("EnterRoom")] EnterRoom = 20003,
    [pbr::OriginalName("LeaveRoom")] LeaveRoom = 20005,
    [pbr::OriginalName("UploadMap")] UploadMap = 20007,
    [pbr::OriginalName("DownloadMap")] DownloadMap = 20009,
    [pbr::OriginalName("DestroyRoom")] DestroyRoom = 20011,
    [pbr::OriginalName("DownloadCities")] DownloadCities = 20021,
    [pbr::OriginalName("CityAdd")] CityAdd = 20022,
    [pbr::OriginalName("CityRemove")] CityRemove = 20023,
    [pbr::OriginalName("DownloadActors")] DownloadActors = 20031,
    [pbr::OriginalName("ActorAdd")] ActorAdd = 20033,
    [pbr::OriginalName("ActorRemove")] ActorRemove = 20034,
    /// <summary>
    /// 部队移动,此指令已废弃,被ActorAiState取代
    /// </summary>
    [pbr::OriginalName("ActorMove")] ActorMove = 20037,
    [pbr::OriginalName("ActorAiState")] ActorAiState = 20039,
    /// <summary>
    /// 因为AI在客户端进行,所以要从客户端同步坐标到服务器
    /// </summary>
    [pbr::OriginalName("UpdateActorPos")] UpdateActorPos = 20041,
    /// <summary>
    /// 更新单元的属性
    /// </summary>
    [pbr::OriginalName("UpdateActorInfo")] UpdateActorInfo = 20043,
    /// <summary>
    /// 仅播放动画, 不改变AI状态机
    /// </summary>
    [pbr::OriginalName("ActorPlayAni")] ActorPlayAni = 20045,
    /// <summary>
    /// 尝试发送指令,如果成功消耗行动点ActionPoint
    /// </summary>
    [pbr::OriginalName("TryCommand")] TryCommand = 20047,
    /// <summary>
    /// 采集
    /// </summary>
    [pbr::OriginalName("HarvestStart")] HarvestStart = 20051,
    /// <summary>
    /// 停止采集
    /// </summary>
    [pbr::OriginalName("HarvestStop")] HarvestStop = 20053,
    /// <summary>
    /// 更新资源变更
    /// </summary>
    [pbr::OriginalName("DownloadResCell")] DownloadResCell = 20055,
    [pbr::OriginalName("UpdateRes")] UpdateRes = 20057,
    /// <summary>
    /// 更新行动点
    /// </summary>
    [pbr::OriginalName("UpdateActionPoint")] UpdateActionPoint = 20059,
    /// <summary>
    /// 战斗
    /// </summary>
    [pbr::OriginalName("FightStart")] FightStart = 20061,
    [pbr::OriginalName("FightStop")] FightStop = 20063,
    /// <summary>
    /// 飙血
    /// </summary>
    [pbr::OriginalName("SprayBlood")] SprayBlood = 20065,
    /// <summary>
    /// 补充弹药
    /// </summary>
    [pbr::OriginalName("AmmoSupply")] AmmoSupply = 20067,
    /// <summary>
    /// 改变AI权限
    /// </summary>
    [pbr::OriginalName("ChangeAiRights")] ChangeAiRights = 20071,
    /// <summary>
    /// 高等级AI状态(不同于之前的AI状态, 之前的状态属于AI状态机的状态)
    /// </summary>
    [pbr::OriginalName("ActorAiStateHigh")] ActorAiStateHigh = 20073,
  }

  /// <summary>
  /// 房间发送到客户端
  /// </summary>
  public enum ROOM_REPLY {
    /// <summary>
    /// proto3枚举的第一个成员必须是0
    /// </summary>
    [pbr::OriginalName("MsgStartReply")] MsgStartReply = 0,
    [pbr::OriginalName("PlayerEnterReply")] PlayerEnterReply = 20002,
    [pbr::OriginalName("EnterRoomReply")] EnterRoomReply = 20004,
    [pbr::OriginalName("LeaveRoomReply")] LeaveRoomReply = 20006,
    [pbr::OriginalName("UploadMapReply")] UploadMapReply = 20008,
    [pbr::OriginalName("DownloadMapReply")] DownloadMapReply = 20010,
    [pbr::OriginalName("DestroyRoomReply")] DestroyRoomReply = 20012,
    [pbr::OriginalName("DownloadCitiesReply")] DownloadCitiesReply = 20022,
    [pbr::OriginalName("CityAddReply")] CityAddReply = 20024,
    [pbr::OriginalName("CityRemoveReply")] CityRemoveReply = 20026,
    [pbr::OriginalName("DownloadActorsReply")] DownloadActorsReply = 20032,
    [pbr::OriginalName("ActorAddReply")] ActorAddReply = 20034,
    [pbr::OriginalName("ActorRemoveReply")] ActorRemoveReply = 20036,
    [pbr::OriginalName("ActorMoveReply")] ActorMoveReply = 20038,
    [pbr::OriginalName("ActorAiStateReply")] ActorAiStateReply = 20040,
    [pbr::OriginalName("UpdateActorPosReply")] UpdateActorPosReply = 20042,
    [pbr::OriginalName("UpdateActorInfoReply")] UpdateActorInfoReply = 20044,
    [pbr::OriginalName("ActorPlayAniReply")] ActorPlayAniReply = 20046,
    [pbr::OriginalName("TryCommandReply")] TryCommandReply = 20048,
    [pbr::OriginalName("HarvestStartReply")] HarvestStartReply = 20052,
    [pbr::OriginalName("HarvestStopReply")] HarvestStopReply = 20054,
    [pbr::OriginalName("DownloadResCellReply")] DownloadResCellReply = 20056,
    [pbr::OriginalName("UpdateResReply")] UpdateResReply = 20058,
    [pbr::OriginalName("UpdateActionPointReply")] UpdateActionPointReply = 20060,
    [pbr::OriginalName("FightStartReply")] FightStartReply = 20062,
    [pbr::OriginalName("FightStopReply")] FightStopReply = 20064,
    [pbr::OriginalName("SprayBloodReply")] SprayBloodReply = 20066,
    [pbr::OriginalName("AmmoSupplyReply")] AmmoSupplyReply = 20068,
    /// <summary>
    /// 改变AI权限
    /// </summary>
    [pbr::OriginalName("ChangeAiRightsReply")] ChangeAiRightsReply = 20072,
    /// <summary>
    /// 高等级AI状态(不同于之前的AI状态, 之前的状态属于AI状态机的状态)
    /// </summary>
    [pbr::OriginalName("ActorAiStateHighReply")] ActorAiStateHighReply = 20074,
  }

  #endregion

}

#endregion Designer generated code
