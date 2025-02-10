using System;
using System.Diagnostics;
using System.Threading;
using Riptide;
using Riptide.Utils;

namespace Neutron
{
    public class NeutronServer
    {
        public static Server Server;
    
        public void Start()
        {
            RiptideLogger.Initialize(
                Console.WriteLine, 
                Console.WriteLine, 
                Console.WriteLine, 
                Console.WriteLine, 
                false);
            
            Server = new Server();
            Server.Start(7777, 100);
        }
        
        public void Tick()
        {
            Server.Update();
        }
    }
}