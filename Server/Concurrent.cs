using Sequential;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
//todo [Assignment]: add required namespaces

namespace Concurrent
{
    public class ConcurrentServer : SequentialServer
    {
        // todo [Assignment]: implement required attributes specific for concurrent server
        public List<Thread> workerThreads;
        public Dictionary<string, int> votesList;
        private static Mutex mut = new Mutex();
        public ConcurrentServer(Setting settings) : base(settings)
        {
            // todo [Assignment]: implement required code
            workerThreads = new List<Thread>();
            votesList = new Dictionary<string, int>();
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
                while (this.numOfClients < this.settings.experimentNumberOfClients)
                {
                    Socket connection = listener.Accept();
                    this.numOfClients++;
                    var t = new Thread(() => {
                        handleClient(connection);
                    });
                    t.Start();
                    workerThreads.Add(t);
                    t.Join();
                }
                int highestVotedValue = votesList.Values.Max();
                bool cmd_executed = false;
                foreach (KeyValuePair<string, int> vote in votesList)
                {
                    if(vote.Value == highestVotedValue && !cmd_executed) {
                        Console.WriteLine("Executing command: " + vote.Key);
                        cmd_executed = true;
                    }
                }
            }catch (Exception e){ Console.Out.WriteLine("[Server] Preparation: {0}",e.Message); }
        }
        new public void handleClient(Socket con)
        {
            string data = "", reply = "";
            byte[] bytes = new byte[bufferSize];

            this.sendMessage(con, Message.ready);
            int numByte = con.Receive(bytes);
            data = Encoding.UTF8.GetString(bytes, 0, numByte);

            string vote = data.Substring(data.IndexOf('>') + 1);
            bool added = votesList.TryAdd(vote, 1);

            // Entering critical section
            mut.WaitOne();
            if(!added) {
                votesList[vote] = votesList[vote] + 1;
            }
            mut.ReleaseMutex();
            // Exiting critical section

            reply = processMessage(data);
            this.sendMessage(con, reply);
            
            
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