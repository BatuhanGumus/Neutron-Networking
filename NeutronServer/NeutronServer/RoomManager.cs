using System;
using System.Collections.Generic;

namespace Neutron
{
    public class RoomManager
    {
        public Dictionary<ushort, Player> Players => _players;
        private Dictionary<ushort, Player> _players = new Dictionary<ushort, Player>();
        
        public Dictionary<ushort, Player> Roomless => _roomless;
        private Dictionary<ushort, Player> _roomless = new Dictionary<ushort, Player>();
        
        public Dictionary<string, RoomInfo> Rooms => _rooms;
        private Dictionary<string, RoomInfo> _rooms = new Dictionary<string, RoomInfo>();
        
        public void RegisterPlayer(Player player)
        {
            _players.Add(player.Id, player);
            _roomless.Add(player.Id, player);
            
            Console.WriteLine($"Player \"{player.Username}\" ({player.Id}) registered");
        }
        
        public void DeregisterPlayer(ushort playerId)
        {
            if (_players.TryGetValue(playerId, out Player player))
            {
                DeregisterPlayer(player);
            }
            else
            {
                Console.WriteLine($"Player {playerId} does not exist to deregister");
            }
        }

        public void DeregisterPlayer(Player player)
        {
            if (string.IsNullOrEmpty(player.InRoomId))
            {
                LeaveRoom(player);
            }
            
            _players.Remove(player.Id);
            _roomless.Remove(player.Id);
            
            Console.WriteLine($"Player \"{player.Username}\" ({player.Id}) deregistered");
        }

        public RoomInfo CreateRoom(ushort masterId, string roomName)
        {
            if (_players.TryGetValue(masterId, out Player master))
            {
                return CreateRoom(master, roomName);
            }
            
            Console.WriteLine($"Player {masterId} does not exist to create a room");
            return null;
        }

        public RoomInfo CreateRoom(Player master, string roomName)
        {
            RoomInfo room = new RoomInfo();
            room.Id = CreateRoomGuid();
            room.MasterClientId = master.Id;
            room.Name = roomName;
            room.Players.Add(master);
            master.InRoomId = room.Id;
            _rooms.Add(room.Id, room);
            _roomless.Remove(master.Id);
            
            Console.WriteLine($"Created new room \"{room.Name}\" ({room.Id}) with owner \"{master.Username}\"");

            return room;
        }
        
        public RoomInfo DestroyRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out RoomInfo room))
            {
                foreach (Player player in room.Players)
                {
                    player.InRoomId = "";
                    _roomless.Add(player.Id, player);
                }
                _rooms.Remove(roomId);
                Console.WriteLine($"Destroyed room {room.Name} ({roomId})");
                return room;
            }
            
            Console.WriteLine($"Room {roomId} does not exist to destroy");
            return null;
        }

        public RoomInfo JoinRoom(ushort playerId, string roomId)
        {
            if(_players.TryGetValue(playerId, out Player player))
            {
                return JoinRoom(player, roomId);
            }
            
            Console.WriteLine($"Player {playerId} does not exist to join room {roomId}");
            return null;
        }
        
        public RoomInfo JoinRoom(Player player, string roomId)
        {
            if (_rooms.TryGetValue(roomId, out RoomInfo room))
            {
                if (room.Players.Count < room.MaxPlayers)
                {
                    room.Players.Add(player);
                    player.InRoomId = room.Id;
                    _roomless.Remove(player.Id);
                    Console.WriteLine($"Player {player.Username} joined room {room.Name} ({roomId})");
                    return room;
                }
                
                Console.WriteLine($"Player {player.Username} tried to join room {roomId} but it was full");
                return null;
            }
            
            Console.WriteLine($"Room {roomId} does not exist for player {player.Username} to join");
            return null;
        }

        public RoomInfo LeaveRoom(ushort playerId)
        {
            if (_players.TryGetValue(playerId, out Player player))
            {
                return LeaveRoom(player);
            }
            
            Console.WriteLine($"Player {playerId} does not exist to leave a room");
            return null;
        }
        
        public RoomInfo LeaveRoom(Player player)
        {
            if (string.IsNullOrEmpty(player.InRoomId))
            {
                Console.WriteLine($"Player {player.Username} tried to leave a room but they were not in one");
                return null;
            }

            string roomId = player.InRoomId;
            if (_rooms.TryGetValue(roomId, out RoomInfo room))
            {
                room.Players.Remove(player);
                player.InRoomId = "";
                _roomless.Add(player.Id, player);

                if (RoomIsEmpty(room) || room.MasterClientId == player.Id)
                {
                    DestroyRoom(roomId);
                }
                
                Console.WriteLine($"Player {player.Username} left room {room.Name} ({roomId})");
                return room;
            }
            
            Console.WriteLine($"Room {player.InRoomId} does not exist for player {player.Username} to leave");
            return null;
        }

        public bool RoomIsEmpty(RoomInfo roomInfo)
        {
            return roomInfo.Players.Count == 0;
        }

        private readonly char[] _guidCharacters = new[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
        };

        private char GetRandomGuidChar()
        {
            return Char.ToUpper(_guidCharacters[ServerData.Random.Next(_guidCharacters.Length)]);
        }
        
        public string CreateRoomGuid()
        {
            bool isNotUnique = true;
            string id = "";
            do
            {
                id = "";
                for (int i = 0; i < 5; i++)
                {
                    id += GetRandomGuidChar();
                }

                isNotUnique = _rooms.ContainsKey(id);

            } while (isNotUnique);

            return id;
        }
    }
}