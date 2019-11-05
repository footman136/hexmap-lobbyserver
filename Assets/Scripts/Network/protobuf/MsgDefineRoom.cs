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
            "ChNNc2dEZWZpbmVSb29tLnByb3RvEg1Qcm90b2J1Zi5Sb29tKqUCCgRST09N",
            "EgwKCE1zZ1N0YXJ0EAASDwoJSGVhcnRCZWF0EKCcARIRCgtQbGF5ZXJFbnRl",
            "chChnAESDwoJRW50ZXJSb29tEKOcARIPCglMZWF2ZVJvb20QpZwBEg8KCVVw",
            "bG9hZE1hcBCnnAESEQoLRG93bmxvYWRNYXAQqZwBEhEKC0Rlc3Ryb3lSb29t",
            "EKucARISCgxDcmVhdGVBVHJvb3AQtZwBEhMKDURlc3Ryb3lBVHJvb3AQt5wB",
            "Eg8KCVRyb29wTW92ZRC/nAESEgoMVHJvb3BBaVN0YXRlEMGcARISCgxBc2tG",
            "b3JDaXRpZXMQw5wBEg0KB0NpdHlBZGQQxZwBEhAKCkNpdHlSZW1vdmUQx5wB",
            "Eg8KCVVwZGF0ZVBvcxDJnAEq5QIKClJPT01fUkVQTFkSEQoNTXNnU3RhcnRS",
            "ZXBseRAAEhYKEFBsYXllckVudGVyUmVwbHkQopwBEhQKDkVudGVyUm9vbVJl",
            "cGx5EKScARIUCg5MZWF2ZVJvb21SZXBseRDGmgwSFAoOVXBsb2FkTWFwUmVw",
            "bHkQqJwBEhYKEERvd25sb2FkTWFwUmVwbHkQqpwBEhYKEERlc3Ryb3lSb29t",
            "UmVwbHkQrJwBEhcKEUNyZWF0ZUFUcm9vcFJlcGx5ELacARIYChJEZXN0cm95",
            "QVRyb29wUmVwbHkQuJwBEhQKDlRyb29wTW92ZVJlcGx5EMCcARIXChFUcm9v",
            "cEFpU3RhdGVSZXBseRDCnAESFwoRQXNrRm9yQ2l0aWVzUmVwbHkQxJwBEhIK",
            "DENpdHlBZGRSZXBseRDGnAESFQoPQ2l0eVJlbW92ZVJlcGx5EMicARIUCg5V",
            "cGRhdGVQb3NSZXBseRDKnAFiBnByb3RvMw=="));
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
    [pbr::OriginalName("HeartBeat")] HeartBeat = 20000,
    [pbr::OriginalName("PlayerEnter")] PlayerEnter = 20001,
    [pbr::OriginalName("EnterRoom")] EnterRoom = 20003,
    [pbr::OriginalName("LeaveRoom")] LeaveRoom = 20005,
    [pbr::OriginalName("UploadMap")] UploadMap = 20007,
    [pbr::OriginalName("DownloadMap")] DownloadMap = 20009,
    [pbr::OriginalName("DestroyRoom")] DestroyRoom = 20011,
    [pbr::OriginalName("CreateATroop")] CreateAtroop = 20021,
    [pbr::OriginalName("DestroyATroop")] DestroyAtroop = 20023,
    [pbr::OriginalName("TroopMove")] TroopMove = 20031,
    [pbr::OriginalName("TroopAiState")] TroopAiState = 20033,
    [pbr::OriginalName("AskForCities")] AskForCities = 20035,
    [pbr::OriginalName("CityAdd")] CityAdd = 20037,
    [pbr::OriginalName("CityRemove")] CityRemove = 20039,
    [pbr::OriginalName("UpdatePos")] UpdatePos = 20041,
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
    [pbr::OriginalName("LeaveRoomReply")] LeaveRoomReply = 200006,
    [pbr::OriginalName("UploadMapReply")] UploadMapReply = 20008,
    [pbr::OriginalName("DownloadMapReply")] DownloadMapReply = 20010,
    [pbr::OriginalName("DestroyRoomReply")] DestroyRoomReply = 20012,
    [pbr::OriginalName("CreateATroopReply")] CreateAtroopReply = 20022,
    [pbr::OriginalName("DestroyATroopReply")] DestroyAtroopReply = 20024,
    [pbr::OriginalName("TroopMoveReply")] TroopMoveReply = 20032,
    [pbr::OriginalName("TroopAiStateReply")] TroopAiStateReply = 20034,
    [pbr::OriginalName("AskForCitiesReply")] AskForCitiesReply = 20036,
    [pbr::OriginalName("CityAddReply")] CityAddReply = 20038,
    [pbr::OriginalName("CityRemoveReply")] CityRemoveReply = 20040,
    [pbr::OriginalName("UpdatePosReply")] UpdatePosReply = 20042,
  }

  #endregion

}

#endregion Designer generated code
