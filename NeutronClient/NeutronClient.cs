using System;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using LogType = Riptide.Utils.LogType;

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
    
    public class NeutronClient : INeutronClient
    {
        private Dictionary<ServerToClientMessageId, Action<Message>> _messageDictionary;

        private void InitializeMessageDictionary()
        {
            _messageDictionary = new Dictionary<ServerToClientMessageId, Action<Message>>()
            {
                { ServerToClientMessageId.Pong, Pong},
                { ServerToClientMessageId.ClientToClientData, ClientToClientData},
                { ServerToClientMessageId.RoomsUpdated, RoomsUpdated},
                { ServerToClientMessageId.RemoteSpawnObject, RemoteSpawnObject},
            };
        }
        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<RejectReason> OnConnectionFailed;
        public event Action<RoomInfo[]> OnRoomsUpdated;
        public event Action<ushort, ushort, string> OnClientToClientDataReceived;

        public ClientStatus ClientStatus => _clientStatus;
        private ClientStatus _clientStatus = ClientStatus.Offline;
        
        public RoomInfo CurrentRoom => _currentRoom;
        private RoomInfo _currentRoom;
        
        public Player LocalPlayer { get; set; }
        
        protected Client _client;

        public NeutronClient()
        {
            InitializeMessageDictionary();
        }

#region Public Funtions

        public void SetupDebugging(
            RiptideLogger.LogMethod debugMethod, 
            RiptideLogger.LogMethod infoMethod,
            RiptideLogger.LogMethod warningMethod,
            RiptideLogger.LogMethod errorMethod, 
            bool includeTimestamps, 
            string timestampFormat = "HH:mm:ss")
        {
            RiptideLogger.Initialize(debugMethod, infoMethod, warningMethod, errorMethod, includeTimestamps, timestampFormat);
        }

        public void Connect()
        {
            _client = new Client();
            _client.Connect($"{NeutronConst.ServerIp}:{NeutronConst.ServerPort}");

            LocalPlayer = new Player();
            LocalPlayer.Id = _client.Id;
            
            SubscribeToClientEvents();
        }
        
        public void Disconnect()
        {
            if (_client != null)
            {
                UnsubscribeToClientEvents();
                _client.Disconnect();
            }
        }
        
        public void Tick()
        {
            _client.Update();
        }
        
        public void Ping()
        {
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.Ping);
            message.AddInt(DateTime.UtcNow.Millisecond);
            _client.Send(message);
        }
        
        public void SignIn()
        {
            if (String.IsNullOrEmpty(LocalPlayer.Username))
            {
                RiptideLogger.Log(LogType.Error, "Username null in UpdatePlayerInfo aborting network event");
            }

            if (_clientStatus == ClientStatus.Connected)
            {
                _clientStatus = ClientStatus.SignedIn;
            }
            
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.SignIn);
            message.AddString(LocalPlayer.Username);
            _client.Send(message);
        }
        
        public void CreateAndJoinRoom(string roomName)
        {
            if (String.IsNullOrEmpty(roomName))
            {
                RiptideLogger.Log(LogType.Error, "RoomName null in CreateAndJoinRoom aborting network event");
            }

            _clientStatus = ClientStatus.InRoom; //TODO have a event for successful
            
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.CreateAndJoinRoom);
            message.AddString(roomName);
            _client.Send(message);
        }

        public void GetRooms()
        {
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.GetRooms);
            _client.Send(message);
        }

        public void JoinRoom(string roomId)
        {
            if (String.IsNullOrEmpty(roomId))
            {
                RiptideLogger.Log(LogType.Error, "roomId null in JoinRoom aborting network event");
            }
            
            _clientStatus = ClientStatus.InRoom; //TODO have a event for successful
            
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.JoinRoom);
            message.AddString(roomId);
            _client.Send(message);
        }
        
        public void LeaveRoom()
        {
            /*
            if (String.IsNullOrEmpty(LocalPlayer.InRoomId))
            {
                Debug.LogError("Player is not in a room");
                return;
            }
            */
            
            _clientStatus = ClientStatus.SignedIn; //TODO have a event for successful
            
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerMessageId.LeaveRoom);
            _client.Send(message);
        }

        public void ClientToClientDataSend(ushort clientToClientEventId, string data, RoomMessagingType messagingType, MessageSendMode sendMode)
        {
            if (String.IsNullOrEmpty(LocalPlayer.InRoomId))
            {
                RiptideLogger.Log(LogType.Error, "ClientToClientMessage cannot be sent if the player is not in a room");
                return;
            }
            
            Message message = Message.Create(sendMode, ClientToServerMessageId.ClientToClientData);
            message.AddUShort(clientToClientEventId);
            message.AddString(data);
            message.AddUShort((ushort)messagingType);
            message.AddString(LocalPlayer.InRoomId);
            _client.Send(message);
        }

        #endregion

#region Internals

        private void SubscribeToClientEvents()
        {
            _client.Connected += ConnectedEvent;
            _client.Disconnected += DisconnectedEvent;
            _client.ClientConnected += ClientConnectedEvent;
            _client.ClientDisconnected += ClientDisconnectedEvent;
            _client.ConnectionFailed += ConnectionFailedEvent;
            _client.MessageReceived += MessageReceivedEvent;
        }
        
        private void UnsubscribeToClientEvents()
        {
            _client.Connected -= ConnectedEvent;
            _client.Disconnected -= DisconnectedEvent;
            _client.ClientConnected -= ClientConnectedEvent;
            _client.ClientDisconnected -= ClientDisconnectedEvent;
            _client.ConnectionFailed -= ConnectionFailedEvent;
            _client.MessageReceived -= MessageReceivedEvent;
        }
        
        
        private void ConnectedEvent(object sender, EventArgs args)
        {
            _clientStatus = ClientStatus.Connected;
            OnConnect?.Invoke();
        }
        
        private void DisconnectedEvent(object sender, DisconnectedEventArgs args)
        {
            OnDisconnect?.Invoke();
        }

        private void ClientConnectedEvent(object sender, ClientConnectedEventArgs args)
        {
            
        }
        
        private void ClientDisconnectedEvent(object sender, ClientDisconnectedEventArgs args)
        {
            
        }

        private void ConnectionFailedEvent(object sender, ConnectionFailedEventArgs args)
        {
            OnConnectionFailed?.Invoke(args.Reason);
        }

        private void MessageReceivedEvent(object sender, MessageReceivedEventArgs args)
        {
            if (_messageDictionary.TryGetValue((ServerToClientMessageId)args.MessageId, out var @event))
            {
                @event.Invoke(args.Message);
            }
        }

        #endregion
        
#region Server To Client Messages

        private void Pong(Message message)
        {
            RiptideLogger.Log(LogType.Debug, $"ping: {DateTime.UtcNow.Millisecond - message.GetInt()}");
        }
        
        private void RoomsUpdated(Message message)
        {
            RoomInfo[] rooms = message.GetSerializables<RoomInfo>();
            OnRoomsUpdated?.Invoke(rooms);
        }
        
        private void ClientToClientData(Message message)
        {
            ushort clientToClientEventId = message.GetUShort();
            ushort sender = message.GetUShort();
            string data = message.GetString();
            OnClientToClientDataReceived?.Invoke(clientToClientEventId, sender, data);
        }

        protected virtual void RemoteSpawnObject(Message message)
        {
            
        }

        #endregion
        
    }
}

