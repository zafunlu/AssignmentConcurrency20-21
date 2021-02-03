using System;
using System.Threading;
using Sequential;

namespace Concurrent
{
    public class ConcurrentClient : SimpleClient
    {
        public Thread workerThread;

        public ConcurrentClient(int id, Setting settings) : base(id, settings)
        {
            // todo [Assignment]: implement required code
            workerThread = new Thread(run);
            workerThread.Start();
        }
        public void run()
        {
            String[] votingList = settings.votingList.Split(settings.commands_sep);
            Random rnd = new Random();

            cmd = votingList[rnd.Next(votingList.Length)];
            cmd_message = "ClientId="+client_id.ToString()+settings.command_msg_sep+cmd;
            this.prepareClient();
            this.communicate();
        }
    }
    public class ConcurrentClientsSimulator : SequentialClientsSimulator
    {
        private ConcurrentClient[] clients;

        public ConcurrentClientsSimulator() : base()
        {
            configure();
            Console.Out.WriteLine("\n[ClientSimulator] Concurrent simulator is going to start with {0}", settings.experimentNumberOfClients);
            clients = new ConcurrentClient[settings.experimentNumberOfClients];
        }

        public void ConcurrentSimulation()
        {
            try
            {
                // todo [Assignment]: implement required code
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i] = new ConcurrentClient(i, settings);
                }

                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].workerThread.Join();
                }
            }
            catch (Exception e)
            { Console.Out.WriteLine("[Concurrent Simulator] {0}", e.Message); }
        }
    }
}