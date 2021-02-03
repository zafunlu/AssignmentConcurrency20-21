using Sequential;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
//todo [Assignment]: add required namespaces

namespace Concurrent
{
    public class ConcurrentServer : SequentialServer
    {
        // todo [Assignment]: implement required attributes specific for concurrent server
        public List<Thread> workerThreads;
        public ConcurrentServer(Setting settings) : base(settings)
        {
            // todo [Assignment]: implement required code
            workerThreads = new List<Thread>();
        }
        public override void prepareServer()
        {
            Console.WriteLine("[Server] is ready to start ...");
            try
            {
                localEndPoint = new IPEndPoint(this.ipAddress, settings.serverPortNumber);
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(settings.serverListeningQueue);
                Console.WriteLine("Waiting for incoming connections ... ");
                while (true)
                {
                    Socket connection = listener.Accept();
                    this.numOfClients++;
                    var t = new Thread(() => {
                        handleClient(connection);
                    });
                    t.Start();
                    workerThreads.Add(t);
                }
            }catch (Exception e){ Console.Out.WriteLine("[Server] Preparation: {0}",e.Message); }
        }
        public override string processMessage(String msg)
        {
            string replyMsg = Message.confirmed;

            try
            {
                switch (msg)
                {
                    case Message.terminate:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("[Server] received from the client -> {0} ", msg);
                        Console.ResetColor();
                        Console.WriteLine("[Server] END : number of clients communicated -> {0} ", this.numOfClients);
                        break;
                    default:
                        replyMsg = Message.confirmed;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("[Server] received from the client -> {0} ", msg);
                        Console.ResetColor();
                        break;
                }
            }catch (Exception e){   Console.Out.WriteLine("[Server] Process Message {0}", e.Message);    }

            return replyMsg;
        }
    }
}