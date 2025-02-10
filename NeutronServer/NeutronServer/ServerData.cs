using System;

namespace Neutron
{
    public static class ServerData
    {
        public static Random Random;
        public static NeutronServer NeutronServer;
        public static InputManager InputManager;
        public static RoomManager RoomManager;
        
        public static void Init()
        {
            Random = new Random();
            NeutronServer = new NeutronServer();
            InputManager = new InputManager();
            RoomManager = new RoomManager();
        }
    }
}