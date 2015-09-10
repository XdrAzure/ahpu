using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using AHPU.Framework;

namespace AHPU.Habbo
{
    [Serializable]
    [XmlRoot("AHPU")]
    public class HabboActionScript
    {
        [XmlIgnore]
        private string _bufferStr, _stringName, _integerName, _booleanName, _shortName, _byteName, _floatName;
        public string Release;
        public string OutgoingDicName, IncomingDicName;

        public SerializableDictionary<int, Packet> OutgoingPackets = new SerializableDictionary<int, Packet>();
        public SerializableDictionary<int, Packet> IncomingPackets = new SerializableDictionary<int, Packet>();

        public HabboActionScript()
        {

        }
        public HabboActionScript(string pathStr)
        {
            _bufferStr = File.ReadAllText(pathStr);

            if(_bufferStr.IndexOf("RELEASE") != -1)
                Release = "RELEASE" + _bufferStr.Split(new[] { "var k:String = \"RELEASE" }, StringSplitOptions.None)[1].Split('"')[0];
            else if (_bufferStr.IndexOf("PRODUCTION") != -1)
                Release = "PRODUCTION" + _bufferStr.Split(new[] { "var k:String = \"PRODUCTION" }, StringSplitOptions.None)[1].Split('"')[0];

            LoadHabboMessageComposer();
        }

        public void LoadHabboMessages()
        {
            if (!_bufferStr.Contains("class HabboMessages "))
                throw new KeyNotFoundException("class HabboMessages ");

            var habboMessagesClassStr =
                _bufferStr.Split(new[] { "class HabboMessages " }, StringSplitOptions.None)[1].Split(new[] { "   }" },
                    StringSplitOptions.None)[0];
            var constSplit = habboMessagesClassStr.Split(new[] { "const " }, StringSplitOptions.None);

            OutgoingDicName = constSplit[1].Split(new[] { ":Map" }, StringSplitOptions.None)[0];
            IncomingDicName = constSplit[2].Split(new[] { ":Map" }, StringSplitOptions.None)[0];

            LoadHabboIds(habboMessagesClassStr, IncomingDicName, IncomingPackets);
            LoadHabboIds(habboMessagesClassStr, OutgoingDicName, OutgoingPackets);

            _bufferStr = _bufferStr.Replace(habboMessagesClassStr, string.Empty);
        }

        private void LoadHabboIds(string hMCS, string dN, Dictionary<int, Packet> dic)
        {
            var split = hMCS.Split(new[] { dN + '[' }, StringSplitOptions.None);
            if (!split.Any())
                throw new NullReferenceException("Invalid HabboMessages.");

            foreach (var splitStr in split.Skip(1))
            {
                var line = splitStr.Split(';')[0];
                var packetIdStr = line.Split(']')[0];
                var delegateFunctionName = line.Split(new[] { "= " }, StringSplitOptions.None)[1];

                var packetId = packetIdStr.Contains('x') ? Convert.ToInt32(packetIdStr, 16) : Convert.ToInt32(packetIdStr);
                dic.Add(packetId, new Packet(delegateFunctionName));

                if(delegateFunctionName.StartsWith("_Safe"))
                    Task.Queue.Enqueue(new QueueData() { Habbo = this, Packet = dic[packetId] });
            }
        }

        private void LoadHabboMessageComposer()
        {
            _stringName = GetFunctionByPosition(GetPositions(".readUTF());", _bufferStr)[0]).Split(new []{ "function "}, StringSplitOptions.None)[1].Split(':')[0];
            _integerName = GetFunctionByPosition(GetPositions(".readInt());", _bufferStr)[0]).Split(new[] { "function " }, StringSplitOptions.None)[1].Split(':')[0];
            _booleanName = GetFunctionByPosition(GetPositions(".readBoolean());", _bufferStr)[0]).Split(new[] { "function " }, StringSplitOptions.None)[1].Split(':')[0];
            _shortName = GetFunctionByPosition(GetPositions(".readShort());", _bufferStr)[0]).Split(new[] { "function " }, StringSplitOptions.None)[1].Split(':')[0];
            _byteName = GetFunctionByPosition(GetPositions(".readByte());", _bufferStr)[0]).Split(new[] { "function " }, StringSplitOptions.None)[1].Split(':')[0];
            _floatName = GetFunctionByPosition(GetPositions(".readFloat());", _bufferStr)[0]).Split(new[] { "function " }, StringSplitOptions.None)[1].Split(':')[0];
        }

        private List<int> GetPositions(string h, string str)
        {
            int start = 0, end = str.Length, at = 0;
            var positions = new List<int>();

            while ((start <= end) && (at > -1))
            {
                var count = end - start;
                at = str.IndexOf(h, start, count, StringComparison.Ordinal);
                if (at == -1) break;
                positions.Add(at);
                start = at + 1;
            }

            return positions;
        } 
        private string GetFunctionByPosition(int pos)
        {
            return GetFunctionByPosition(pos, _bufferStr);
        }

        private string GetFunctionByPosition(int pos, string str)
        {
            var startP = str.LastIndexOf(" function ", pos, StringComparison.Ordinal);
            var endP = str.IndexOf(Environment.NewLine + "        }" + Environment.NewLine, pos, StringComparison.Ordinal);
            return str.Substring(startP, endP - startP);
        }

        private string GetNearTopFunctionByPosition(int pos, string str)
        {
            var endP = str.LastIndexOf(Environment.NewLine + "        }" + Environment.NewLine, pos, StringComparison.Ordinal);
            var startClass = str.LastIndexOf(Environment.NewLine + "    {" + Environment.NewLine, pos, StringComparison.Ordinal);
            if (startClass > endP) return string.Empty;

            var startP = str.LastIndexOf(" function ", endP, StringComparison.Ordinal);
            return str.Substring(startP, endP - startP);
        }

        private string GetNearBottomFunctionByPosition(int pos, string str)
        {
            var startP = str.IndexOf(" function ", pos, StringComparison.Ordinal);
            var endClass = str.IndexOf(Environment.NewLine + "    }" + Environment.NewLine, pos, StringComparison.Ordinal);
            if (startP > endClass) return string.Empty;

            var endP = str.IndexOf(Environment.NewLine + "        }" + Environment.NewLine, startP, StringComparison.Ordinal);

            return str.Substring(startP, endP - startP);
        }

        private string GetClassByPosition(int pos)
        {
            var startP = _bufferStr.LastIndexOf(" class ", pos, StringComparison.Ordinal);
            var endP = _bufferStr.IndexOf(Environment.NewLine + "    }" + Environment.NewLine, pos, StringComparison.Ordinal);
            return _bufferStr.Substring(startP, endP - startP);
        }

        private string GetClassNameByPosition(int pos)
        {
            var startP = _bufferStr.LastIndexOf(" class ", pos, StringComparison.Ordinal);
            var endP = _bufferStr.LastIndexOf(Environment.NewLine + "    {" + Environment.NewLine, pos, StringComparison.Ordinal);
            var classLine = _bufferStr.Substring(startP, endP - startP);

            return classLine.Split(new[] { " class " }, StringSplitOptions.None)[1].Split(new[] { " ", Environment.NewLine }, StringSplitOptions.None)[0];
        }

        private void ParseVoid(Packet packet, string function)
        {
            packet.ConditionalCount += GetPositions("if", function).Count;
            packet.ConditionalElseCount += GetPositions("else", function).Count;
            packet.LocalCount += GetPositions("_local_", function).Count;
            packet.ArgCount += GetPositions("_arg_", function).Count;
            packet.EventsCount += GetPositions("events.dispatchEvent", function).Count;
            packet.ForCount += GetPositions("for (", function).Count;
            packet.ForeachCount += GetPositions("for each (", function).Count;
            packet.WhileCount += GetPositions("while (", function).Count;
            packet.SwitchCount += GetPositions("switch (", function).Count;
            packet.CaseCount += GetPositions("case ", function).Count;
            packet.DefaultCount += GetPositions("default:", function).Count;
            packet.PointCount += GetPositions("new Point(", function).Count;
            packet.IndexOfCount += GetPositions(".indexOf(", function).Count;
            packet.GetValueCount += GetPositions(".getValue(", function).Count;
            packet.IntegersCount += GetPositions(":int ", function).Count;
            packet.StringsCount += GetPositions(":String ", function).Count;
            packet.BoolsCount += GetPositions(":Boolean ", function).Count;
            packet.ArrayCount += GetPositions(":Array ", function).Count;
            packet.NewCount += GetPositions("new ", function).Count;
            packet.SendCount += GetPositions(".send(", function).Count;
            packet.ReturnNull += GetPositions("return;", function).Count;
            packet.ReturnFalse += GetPositions("return (false);", function).Count;
            packet.ReturnTrue += GetPositions("return (true);", function).Count;
            packet.ReturnTotal += GetPositions("return ", function).Count;
            packet.DotsCount += GetPositions(".", function).Count;
            packet.OrCount += GetPositions(" || ", function).Count;
            packet.AndCount += GetPositions(" && ", function).Count;
            packet.NotCount += GetPositions("!", function).Count;
            packet.BitAndCount += GetPositions(" & ", function).Count;
            packet.NullCount += GetPositions(" null", function).Count;

            using (var reader = new StringReader(function))
            {
                string line;
                int lineId = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("function " + packet.DelegateFunction + "("))
                    {
                        var voidStr = line.Substring(line.IndexOf("(", StringComparison.Ordinal));
                        voidStr = voidStr.Substring(1, voidStr.IndexOf(")", StringComparison.Ordinal) - 1);


                        if (!string.IsNullOrWhiteSpace(voidStr) && voidStr != "k:Function")
                            packet.Builders.AddRange(voidStr.Split(',').ToList());
                    }
                    if (line.Contains("function "))
                    {
                        var voidName = line.Substring(line.IndexOf("function ", StringComparison.Ordinal));
                        voidName = voidName.Substring(9, voidName.IndexOf("(", StringComparison.Ordinal) - 9);

                        if (!voidName.StartsWith("_Safe"))
                            packet.FunctionsNames.Add(voidName);
                    }
                    if(line.Contains(packet.DelegateFunction))
                        packet.Lines.Add(lineId + (char)7 + RemoveSafeStr(line));
                    if (line.Contains("super("))
                    {
                        if (line.Contains(","))
                            ParseClassMesage(packet, line);

                        var superStr = RemoveSafeStr(line.Replace(" ", string.Empty));
                        if (superStr != "super(k,);" && !packet.Supers.Contains(superStr))
                            packet.Supers.Add(superStr);
                    }

                    lineId++;
                }
            }

            var array = Regex.Matches(function, @"\""(.*?)\""");
            if (array.Count > 0)
            {
                foreach (var txt in array.Cast<Match>().Where(txt => !packet.Strings.Contains(txt.Groups[1].Value)))
                {
                    packet.Strings.Add(txt.Groups[1].Value);
                    function = function.Replace("\"" + txt.Groups[1].Value + "\"", string.Empty);
                }
            }

            var split = function.Split('.', ':');
            foreach (var str in split.Skip(1))
            {
                var call = str.Split(';', ' ', ')', '(')[0];
                if (call.Contains("_SafeStr_") || call.Contains(Environment.NewLine))
                    continue;

                if(!packet.Calls.Contains(call))
                    packet.Calls.Add(call);
            }
        }

        private void ParseClassMesage(Packet packet, string line)
        {
            var className = line.Split(',')[1].Replace(");", string.Empty).Replace(" ", string.Empty);
            var classPosition = GetPositions("class " + className + " implements", _bufferStr);

            if (!classPosition.Any()) return;
            var classStr = GetClassByPosition(classPosition[0] + 10);

            #region Readers
            var parsePosition = GetPositions(" parse(", classStr);
            if (!parsePosition.Any()) return;
            var voidStr = GetFunctionByPosition(parsePosition[0], classStr);

            using (var reader = new StringReader(voidStr))
            {
                string line2;
                while ((line2 = reader.ReadLine()) != null)
                {
                    if(line2.Contains(_stringName))
                        packet.Readers.Add("readString()");
                    else if (line2.Contains(_integerName))
                        packet.Readers.Add("readInteger()");
                    else if (line2.Contains(_booleanName))
                        packet.Readers.Add("readBoolean()");
                    else if (line2.Contains(_shortName))
                        packet.Readers.Add("readShort()");
                    else if (line2.Contains(_byteName))
                        packet.Readers.Add("readByte()");
                    else if (line2.Contains(_floatName))
                        packet.Readers.Add("readFloat()");
                    else if(line2.Contains(" else"))
                        packet.Readers.Add("else");
                    else if (line2.Contains("if ("))
                        packet.Readers.Add("if");
                    else if (line2.Contains("for ("))
                        packet.Readers.Add("for");
                    else if (line2.Contains("for each ("))
                        packet.Readers.Add("foreach");
                    else if (line2.Contains("while ("))
                        packet.Readers.Add("while");
                    else if (line2.Contains("switch ("))
                        packet.Readers.Add("switch");
                    else if (line2.Contains("case ("))
                        packet.Readers.Add("case");
                    else if (line2.Contains("new "))
                        packet.Readers.Add(RemoveSafeStr(line2.Substring(line2.IndexOf("new ", StringComparison.Ordinal))));
                    else if (line2.Contains(" return"))
                        packet.Readers.Add(RemoveSafeStr(line2.Substring(line2.IndexOf(" return", StringComparison.Ordinal) + 1)));
                }
            }
            #endregion
        }

        public string RemoveSafeStr(string str)
        {
            return Regex.Replace(string.Join(")", str.Split('\r')[0].Split(')')), @"_SafeStr_\d+", string.Empty);
        }

        public void DefinePacket(Packet packet)
        {
            var split1 = GetPositions(packet.DelegateFunction + "):void", _bufferStr);
            var split2 = GetPositions(packet.DelegateFunction + '(', _bufferStr);
            var split3 = GetPositions(packet.DelegateFunction + ';', _bufferStr);
            var split4 = GetPositions("(k as " + packet.DelegateFunction + ')', _bufferStr);
            var split6 = GetPositions("(event as " + packet.DelegateFunction + ')', _bufferStr);
            var split5 = GetPositions(packet.DelegateFunction + '(', _bufferStr);

            packet.References = split1.Count + split3.Count - 1;

            foreach (var splitStr in split1)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);

                var function = GetFunctionByPosition(splitStr);
                ParseVoid(packet, function);
            }


            foreach (var splitStr in split2)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);

                var function = GetNearTopFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearTopPacket.Add(nearPacket);
                }
                function = GetNearBottomFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearBottomPacket.Add(nearPacket);
                }

                function = GetFunctionByPosition(splitStr);
                var inside = RemoveSafeStr(function).Replace(" function ", string.Empty);
                if (!string.IsNullOrWhiteSpace(inside) && !packet.Open.Contains(inside) && !inside.StartsWith("//"))
                    packet.Open.Add(inside);
            }

            foreach (var splitStr in split3)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);
            }

            foreach (var splitStr in split4)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);

                var function = GetNearTopFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearTopPacket.Add(nearPacket);
                }
                function = GetNearBottomFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearBottomPacket.Add(nearPacket);
                }

                function = GetFunctionByPosition(splitStr);
                ParseVoid(packet, function);
            }

            foreach (var splitStr in split5)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);

                var function = GetNearTopFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearTopPacket.Add(nearPacket);
                }
                function = GetNearBottomFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearBottomPacket.Add(nearPacket);
                }

                function = GetFunctionByPosition(splitStr);
                ParseVoid(packet, function);
            }

            foreach (var splitStr in split6)
            {
                var className = GetClassNameByPosition(splitStr);
                if (!string.IsNullOrWhiteSpace(className) && !packet.Classes.Contains(className) &&
                    !className.StartsWith("_SafeStr")) packet.Classes.Add(className);

                var function = GetNearTopFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearTopPacket.Add(nearPacket);
                }
                function = GetNearBottomFunctionByPosition(splitStr, _bufferStr);
                if (!string.IsNullOrEmpty(function))
                {
                    var nearPacket = new Packet("NEARPACKET");
                    ParseVoid(nearPacket, function);

                    packet.NearBottomPacket.Add(nearPacket);
                }

                function = GetFunctionByPosition(splitStr);
                ParseVoid(packet, function);
            }

            packet.Classes.Sort();
            packet.Open.Sort();
            packet.Calls.Sort();
            packet.Readers.Sort();
            packet.Builders.Sort();
            packet.Lines.Sort();
            packet.Strings.Sort();
            packet.Supers.Sort();

            Console.WriteLine(packet.DelegateFunction + "   " + packet.References + "   " + string.Join(",", packet.Open));
            Console.WriteLine();
        }
    }
}
