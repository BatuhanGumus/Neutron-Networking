using System;
using System.Collections.Generic;
using Riptide;

namespace Neutron
{
    public class RoomInfo : IMessageSerializable
    {
        public List<Player> Players = new List<Player>();
        public ushort MasterClientId;
        public string Id = "66666";
        public string Name = "New Room";
        public int MaxPlayers = 4;
        public bool IsVisible = true;
        public bool IsOpen = true;

        public void Serialize(Message message)
        {
            message.AddSerializables(Players.ToArray());
            message.AddUShort(MasterClientId);
            message.AddString(Id);
            message.AddString(Name);
            message.AddInt(MaxPlayers);
            message.AddBool(IsVisible);
            message.AddBool(IsOpen);
        }

        public void Deserialize(Message message)
        {
            Players = new List<Player>(message.GetSerializables<Player>());
            MasterClientId = message.GetUShort();
            Id = message.GetString();
            Name = message.GetString();
            MaxPlayers = message.GetInt();
            IsVisible = message.GetBool();
            IsOpen = message.GetBool();
        }
    }
}