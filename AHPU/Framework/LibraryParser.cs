using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azure.Messages.Parsers
{
    /// <summary>
    /// Class LibraryParser.
    /// </summary>
    internal static class LibraryParser
    {
        internal static void Register(string path, string toPath, Dictionary<int, List<int>> dic, string release)
        {
            File.WriteAllText(toPath, string.Empty);

            foreach (
                var line in
                    File.ReadAllLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                {
                    File.AppendAllText(toPath, line + Environment.NewLine);
                    continue;
                }

                var array = line.Split('=');
                File.AppendAllText(toPath, array[0] + "=");

                var punctuation = false;
                if (array[1].IndexOf('/') != -1) array[1] = array[1].Split('/')[0];
                if (array[1].IndexOf(';') != -1)
                {
                    punctuation = true;
                    array[1] = array[1].Split(';')[0];
                }
                int packetId;
                string packetIdStr = array[1].Replace(" ", string.Empty).ToLower();
                
                if(packetIdStr.IndexOf('x') != -1)
                    packetIdStr = Convert.ToInt32(packetIdStr, 16).ToString();
                if (!int.TryParse(packetIdStr, out packetId))
                {
                    File.AppendAllText(toPath, "-1" + (punctuation ? ";" : string.Empty) + "//invalid format: " + array[1] + Environment.NewLine);
                    continue;
                }
                if (!dic.ContainsKey(packetId))
                {
                    File.AppendAllText(toPath, "-1" + (punctuation ? ";" : string.Empty) + "//error 404" + Environment.NewLine);
                    continue;
                }

                File.AppendAllText(toPath, (punctuation ? " " : string.Empty) + string.Join(",", dic[packetId]));
                File.AppendAllText(toPath, (punctuation ? "; // " + release : string.Empty) + Environment.NewLine);
            }
        }
    }
}