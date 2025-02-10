using System;
using System.Diagnostics;
using System.Threading;

namespace Neutron
{
    class ServerEngine
    {
        static bool serverRunning = true;
        
        const int updatePerSec = 50;
        const int targetFrameTime = 1000 / updatePerSec;
        
        static void Main()
        {
            ServerData.Init();
            
            Thread inputThread = new Thread(ListenForInput);
            inputThread.Start();

            ServerData.NeutronServer.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (serverRunning)
            {
                ServerData.NeutronServer.Tick();

                long deltaTime = stopwatch.ElapsedMilliseconds;
                if (targetFrameTime > deltaTime)
                {
                    Thread.Sleep((int)(targetFrameTime - deltaTime));
                }

                stopwatch.Restart();
            }
        }
        
        static void ListenForInput()
        {
            while (serverRunning)
            {
                // Read user input from the command line
                string input = Console.ReadLine();
    
                // Process the input
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    serverRunning = false; // Signal the main loop to exit
                }
                else
                {
                    ServerData.InputManager.Input(input);
                }
            }
        }
    }
}