using System;
using System.Linq;
using Riptide;

namespace Neutron
{
    public enum ClientToServerMessageId : ushort
    {
        Ping = 1,
        ClientToClientData = 2,
        SignIn,
        CreateAndJoinRoom,
        GetRooms,
        JoinRoom,
        LeaveRoom,
        SpawnObject,
    }
    
    public enum ServerToClientMessageId : ushort
    {
        Pong = 1,
        ClientToClientData = 2,
        RoomsUpdated,
        CurrentRoomUpdated,
        RoomJoinSuccess,
        MasterClientLeftRoom,
        PlayerJoinedRoom,
        PlayerLeftRoom,
        RemoteSpawnObject,
    }
    
    public enum RoomMessagingType  : ushort
    {
        All = 1,
        Master = 2,
        Other = 3,
    }
    
    public static class MessageManager
    {
        private static void SendMessageToRoom(RoomInfo room, Message message)
        {
            foreach (var player in room.Players)
            {
                NeutronServer.Server.Send(message, player.Id);
            }
        }
        
        private static void SendMessageToRoom(ushort clientId, RoomInfo room, Message message)
        {
            foreach (var player in room.Players)
            {
                if(clientId != player.Id)
                    NeutronServer.Server.Send(message, player.Id);
            }
        }

        private static void SendMessageToRoomless(Message message)
        {
            foreach (var player in ServerData.RoomManager.Roomless)
            {
                NeutronServer.Server.Send(message, player.Key);
            }
        }

        private static void SendMessageToRoomMaster(RoomInfo room, Message message)
        {
            NeutronServer.Server.Send(message, room.MasterClientId);
        }

        private static void RoomsUpdated()
        {
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClientMessageId.RoomsUpdated);
            message.AddSerializables(ServerData.RoomManager.Rooms.Values.ToArray());
            SendMessageToRoomless(message);
        }

        private static void SendRoomsTo(ushort asker)
        {
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClientMessageId.RoomsUpdated);
            message.AddSerializables(ServerData.RoomManager.Rooms.Values.ToArray());
            NeutronServer.Server.Send(message, asker);
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.Ping)]
        private static void Ping(ushort clientId, Message message)
        {
            Console.WriteLine($"ping request received from: {clientId}");
            Message pongMessage = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientMessageId.Pong);
            pongMessage.AddInt(message.GetInt());
            NeutronServer.Server.Send(pongMessage, clientId);
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.SignIn)]
        private static void SignIn(ushort clientId, Message message)
        {
            Player player = new Player();
            player.Id = clientId;
            player.Username = message.GetString();
            ServerData.RoomManager.RegisterPlayer(player);
            
            SendRoomsTo(clientId);
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.CreateAndJoinRoom)]
        private static void CreateRoom(ushort clientId, Message message)
        {
            var roomName = message.GetString();
            ServerData.RoomManager.CreateRoom(clientId, roomName);

            RoomsUpdated();
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.GetRooms)]
        private static void GetRooms(ushort clientId, Message message)
        {
           SendRoomsTo(clientId);
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.JoinRoom)]
        private static void JoinRoom(ushort clientId, Message message)
        {
            var roomId = message.GetString();
            ServerData.RoomManager.JoinRoom(clientId, roomId);
            
            RoomsUpdated();
        }

        [MessageHandler((ushort)ClientToServerMessageId.LeaveRoom)]
        private static void LeaveRoom(ushort clientId, Message message)
        {
            ServerData.RoomManager.LeaveRoom(clientId);
        }
        
        [MessageHandler((ushort)ClientToServerMessageId.ClientToClientData)]
        private static void ClientToClientData(ushort clientId, Message message)
        {
            ushort eventType = message.GetUShort();
            string data = message.GetString();
            RoomMessagingType messagingType = (RoomMessagingType)message.GetUShort();
            string roomId = message.GetString();
            
            RoomInfo room = ServerData.RoomManager.Rooms[roomId];
            Message roomMessage = Message.Create(message.SendMode, (ushort)ServerToClientMessageId.ClientToClientData);
            
            roomMessage.AddUShort(clientId);
            roomMessage.AddUShort(eventType);
            roomMessage.AddString(data);

            switch (messagingType)
            {
                case RoomMessagingType.All:
                    SendMessageToRoom(room, roomMessage);
                    break;
                case RoomMessagingType.Other:
                    SendMessageToRoom(clientId, room, roomMessage);
                    break;
                case RoomMessagingType.Master:
                    SendMessageToRoomMaster(room, roomMessage);
                    break;
            }
        }
    }
}