using Riptide;

namespace Neutron
{
    public class Player  : IMessageSerializable
    {
        public ushort Id;
        public string Username = "";
        public string InRoomId = "";
        public void Serialize(Message message)
        {
            message.AddUShort(Id);
            message.AddString(Username);
            message.AddString(InRoomId);
        }

        public void Deserialize(Message message)
        {
            Id = message.GetUShort();
            Username = message.GetString();
            InRoomId = message.GetString();
        }
    }
}