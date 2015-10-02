using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using AHPU.Framework;
using AHPU.Habbo;
using Azure.Messages.Parsers;

namespace AHPU
{
    internal class Program
    {
        private static readonly XmlSerializer SessionSerializer = new XmlSerializer(typeof (HabboActionScript));

        private static void Main()
        {
            Console.WriteLine("AHPU By Xdr v1.1 ... ");

            var actionOldScript = new HabboActionScript("old.txt");
            if (!File.Exists("Cache/" + actionOldScript.Release + ".xml"))
                actionOldScript.LoadHabboMessages();
            else
            {
                using (var reader = new StreamReader("Cache/" + actionOldScript.Release + ".xml"))
                {
                    actionOldScript = (HabboActionScript) SessionSerializer.Deserialize(reader);
                    reader.Close();
                }
            }
            var actionNewScript = new HabboActionScript("new.txt");
            if (!File.Exists("Cache/" + actionNewScript.Release + ".xml"))
                actionNewScript.LoadHabboMessages();
            else
            {
                using (var reader = new StreamReader("Cache/" + actionNewScript.Release + ".xml"))
                {
                    actionNewScript = (HabboActionScript) SessionSerializer.Deserialize(reader);
                    reader.Close();
                }
            }

            Task.Start();
            Thread.Sleep(3000);
            while (Task.Queue.Any()) Thread.Sleep(1000);

            if (!File.Exists("Cache/" + actionOldScript.Release + ".xml"))
            {
                Directory.CreateDirectory("Cache/");
                using (var fs = new FileStream("Cache/" + actionOldScript.Release + ".xml", FileMode.Create))
                {
                    SessionSerializer.Serialize(fs, actionOldScript);
                    fs.Close();
                }
            }

            if (!File.Exists("Cache/" + actionNewScript.Release + ".xml"))
            {
                using (var fs = new FileStream("Cache/" + actionNewScript.Release + ".xml", FileMode.Create))
                {
                    SessionSerializer.Serialize(fs, actionNewScript);
                    fs.Close();
                }
            }

            Console.WriteLine();
            Comparer.Compare(actionOldScript, actionNewScript);

            #region Updater
            if (File.Exists("Azure/" + actionOldScript.Release + ".incoming"))
            {
                Directory.CreateDirectory("Azure/Updated/");

                LibraryParser.Register("Azure/" + actionOldScript.Release + ".incoming",
                    "Azure/Updated/" + actionNewScript.Release + ".incoming", Comparer.IncomingIds, actionNewScript.Release);

                Console.WriteLine("Updated: Azure/" + actionOldScript.Release + ".incoming to Azure/Updated/" +
                                  actionNewScript.Release + ".incoming");
            }
            if (File.Exists("Azure/" + actionOldScript.Release + ".outgoing"))
            {
                Directory.CreateDirectory("Azure/Updated/");

                LibraryParser.Register("Azure/" + actionOldScript.Release + ".outgoing",
                    "Azure/Updated/" + actionNewScript.Release + ".outgoing", Comparer.OutgoingIds, actionNewScript.Release);
                Console.WriteLine("Updated: Azure/" + actionOldScript.Release + ".outgoing to Azure/Updated/" +
                                  actionNewScript.Release + ".outgoing");
            }
            if (File.Exists("Uber/Events.cs"))
            {
                Directory.CreateDirectory("Uber/Updated/");

                LibraryParser.Register("Uber/Events.cs",
                    "Uber/Updated/Events.cs", Comparer.IncomingIds, actionNewScript.Release);

                Console.WriteLine("Updated: Uber/Events.cs to Uber/Updated/Events.cs");
            }
            if (File.Exists("Uber/Composers.cs"))
            {
                Directory.CreateDirectory("Uber/Updated/");

                LibraryParser.Register("Uber/Composers.cs",
                    "Uber/Updated/Composers.cs", Comparer.OutgoingIds, actionNewScript.Release);
                Console.WriteLine("Updated: Uber/Composers.cs to Uber/Updated/Composers.cs");
            }
            #endregion

            Console.WriteLine();
            Console.WriteLine("Waitings inputs of ids ...");

            while (true)
            {
                var str = Console.ReadLine();
                int packetId;
                var found = false;

                if (string.IsNullOrWhiteSpace(str) || !int.TryParse(str, out packetId))
                {
                    Console.WriteLine("Invalid input. Must be a valid number.");
                    continue;
                }

                if (Comparer.OutgoingIds.ContainsKey(packetId))
                {
                    Console.WriteLine("Outgoing: " + string.Join(", ", Comparer.OutgoingIds[packetId]));
                    found = true;
                }
                if (Comparer.IncomingIds.ContainsKey(packetId))
                {
                    Console.WriteLine("Incoming: " + string.Join(", ", Comparer.IncomingIds[packetId]));
                    found = true;
                }

                if (!found)
                    Console.WriteLine("Id not found!!!");
                Console.WriteLine();
            }
        }
    }
}