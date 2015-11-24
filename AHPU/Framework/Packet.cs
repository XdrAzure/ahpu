using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AHPU.Framework
{
    [Serializable]
    public class Packet
    {
        public string DelegateFunction;

        public int References,
                   LocalCount,
                   ArgCount,
                   ThisCount,
                   EventsCount,
                   KCount,
                   ForCount,
                   ForeachCount,
                   WhileCount,
                   ConditionalCount,
                   ConditionalNegativeCount,
                   ConditionalElseCount,
                   SwitchCount,
                   CaseCount,
                   DefaultCount,
                   NullCount,
                   PointCount,
                   IndexOfCount,
                   GetValueCount,
                   IntegersCount,
                   StringsCount,
                   BoolsCount,
                   ArrayCount,
                   NewCount,
                   SendCount,
                   ReturnNull,
                   ReturnFalse,
                   ReturnTrue,
                   ReturnTotal,
                   DotsCount,
                   OrCount,
                   AndCount,
                   NotCount,
                   BitAndCount,
                   Equal,
                   ComparatorEqual,
                   ComparatorNotEqual,
                   ComparatorHigher,
                   ComparatorLower,
                   ComparatorEqualOrHigher,
                   ComparatorEqualOrLower,
                   FalseCount,
                   TrueCount,
                   RestCount,
                   SumCount,
                   LengthCount,
                   AsCount,
                   IsCount,
                   InCount;

        [XmlArray("Classes"), XmlArrayItem(typeof (string), ElementName = "Classes")] public List<string> Classes =
            new List<string>();

        [XmlArray("Open"), XmlArrayItem(typeof (string), ElementName = "Open")] public List<string> Open =
            new List<string>();

        [XmlArray("Calls"), XmlArrayItem(typeof (string), ElementName = "Calls")] public List<string> Calls =
            new List<string>();

        [XmlArray("Strings"), XmlArrayItem(typeof (string), ElementName = "Strings")] public List<string> Strings =
            new List<string>();

        [XmlArray("Lines"), XmlArrayItem(typeof (string), ElementName = "Lines")] public List<string> Lines =
            new List<string>();

        [XmlArray("Supers"), XmlArrayItem(typeof (string), ElementName = "Supers")] public List<string> Supers =
            new List<string>();

        [XmlArray("Readers"), XmlArrayItem(typeof (string), ElementName = "Readers")] public List<string> Readers =
            new List<string>();

        [XmlArray("Builders"), XmlArrayItem(typeof (string), ElementName = "Builders")] public List<string> Builders =
            new List<string>();

        [XmlArray("FunctionsNames"), XmlArrayItem(typeof (string), ElementName = "FunctionsNames")] public List<string>
            FunctionsNames = new List<string>();

        [XmlArray("FunctionsOrders"), XmlArrayItem(typeof (string), ElementName = "FunctionsOrders")] public
            List<string> FunctionsOrders = new List<string>();

        [XmlArray("NearTopPacket"), XmlArrayItem(typeof (Packet), ElementName = "NearTopPacket")] public List<Packet>
            NearTopPacket = new List<Packet>();

        [XmlArray("NearBottomPacket"), XmlArrayItem(typeof (Packet), ElementName = "NearBottomPacket")] public
            List<Packet> NearBottomPacket = new List<Packet>();

        public Packet() { }
        public Packet(string delegateFunction) { DelegateFunction = delegateFunction; }

        public void Sort()
        {
            Classes.Sort();
            Open.Sort();
            Calls.Sort();
            Readers.Sort();
            Builders.Sort();
            Lines.Sort();
            Strings.Sort();
            Supers.Sort();
            FunctionsOrders.Sort();
        }
    }
}