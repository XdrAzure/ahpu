using System;
using System.Collections.Concurrent;
using AHPU.Habbo;

namespace AHPU.Framework
{
    class Task
    {
        public static ConcurrentQueue<QueueData> Queue = new ConcurrentQueue<QueueData>();

        public static void Start()
        {
            var speed = 8;
            while (speed > 0)
            {
                System.Threading.Tasks.Task.Factory.StartNew(Proccess);
                speed--;
            }
        }

        private static void Proccess()
        {
            QueueData queueData;
            while (Queue.TryDequeue(out queueData))
            {
                try
                {
                    queueData.Habbo.DefinePacket(queueData.Packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }

    struct QueueData
    {
        public Packet Packet;
        public HabboActionScript Habbo;
    }
}
