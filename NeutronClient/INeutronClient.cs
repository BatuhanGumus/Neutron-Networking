using System;
using Riptide;
using Riptide.Utils;
using UnityEngine;

namespace Neutron
{
    public enum RoomMessagingType : ushort
    {
        All = 1,
        Master = 2,
        Other = 3,
    }

    public enum ClientStatus : ushort
    {
        Offline,
        Connected,
        SignedIn,
        InRoom,
    }

    public static class NeutronConst
    {
        public const string ServerIp = "127.0.0.1";
        public const string ServerPort = "7777";
    }

    public interface INeutronClient
    {
        event Action OnConnect;
        event Action OnDisconnect;
        event Action<RejectReason> OnConnectionFailed;
        event Action<RoomInfo[]> OnRoomsUpdated;
        event Action<ushort, ushort, string> OnClientToClientDataReceived;

        ClientStatus ClientStatus { get; }
        Player LocalPlayer { get; set; }
        RoomInfo CurrentRoom { get; }

        bool IsMasterClient
        {
            get
            {
                if (LocalPlayer == null || CurrentRoom == null)
                {
                    Debug.LogWarning("Cannot check room ownership before player and room are initialized.");
                    return false;
                }
                
                return LocalPlayer.Id == CurrentRoom.MasterClientId;
            }
        }

        void SetupDebugging(
            RiptideLogger.LogMethod debugMethod,
            RiptideLogger.LogMethod infoMethod,
            RiptideLogger.LogMethod warningMethod,
            RiptideLogger.LogMethod errorMethod,
            bool includeTimestamps,
            string timestampFormat = "HH:mm:ss");
        void Connect();
        void Disconnect();
        void Tick();
        void Ping();
        void SignIn();
        void CreateAndJoinRoom(string roomName);
        void GetRooms();
        void JoinRoom(string roomId);
        void LeaveRoom();

        void ClientToClientDataSend(ushort clientToClientEventId, string data, RoomMessagingType messagingType,
            MessageSendMode sendMode);
    }
}