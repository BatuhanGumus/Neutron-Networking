using System;
using System.Collections.Generic;

namespace Neutron
{
    public class InputManager
    {
        private Dictionary<string, Action> Functions;
        
        public InputManager()
        {
            Functions = new Dictionary<string, Action>()
            {
                {"ListAllPlayers", ListAllPlayers},
                {"ListWaitingPlayers", ListWaitingPlayers},
                {"ListRooms", ListRooms},
            };
        }
        
        public void Input(string input)
        {
            if(Functions.ContainsKey(input))
            {
                Functions[input].Invoke();
            }
            else
            {
                Console.WriteLine($"invalid input: {input}");
            }
        }

        private void ListAllPlayers()
        {
            string outText = "";
            if (ServerData.RoomManager.Players.Count == 0)
            {
                outText = "No player logged in";
            }
            else
            {
                foreach (var pair in ServerData.RoomManager.Players)
                {
                    outText += pair.Value.Username + ", ";
                }
            }
            
            Console.WriteLine(outText);
        }
        
        private void ListWaitingPlayers()
        {
            string outText = "";
            if (ServerData.RoomManager.Players.Count == 0)
            {
                outText = "No player logged in";
            }
            else
            {
                foreach (var pair in ServerData.RoomManager.Players)
                {
                   if(string.IsNullOrEmpty(pair.Value.InRoomId)) 
                       outText += pair.Value.Username + ", ";
                }
            }
            
            Console.WriteLine(outText);
        }

        private void ListRooms()
        {
            string outText = "";
            if (ServerData.RoomManager.Rooms.Count == 0)
            {
                outText = "No room created";
            }
            else
            {
                foreach (var room in ServerData.RoomManager.Rooms)
                {
                    outText += $"Room: \"{room.Value.Name}\" ({room.Key}) - ";
                    foreach (var player in room.Value.Players)
                    {
                        outText += $"{player.Username}, ";
                    }

                    outText += "\n";
                }
            }
            
            Console.WriteLine(outText);
        }
    }
}