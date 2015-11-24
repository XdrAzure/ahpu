using System.Collections.Generic;
using System.Linq;
using AHPU.Habbo;

namespace AHPU.Framework
{
    internal static class Comparer
    {
        public static Dictionary<int, List<int>> OutgoingIds = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> IncomingIds = new Dictionary<int, List<int>>();

        public static void Compare(HabboActionScript old, HabboActionScript news)
        {
            InternalComparer(old.OutgoingPackets, news.OutgoingPackets, OutgoingIds);
            InternalComparer(old.IncomingPackets, news.IncomingPackets, IncomingIds);
        }

        #region Main Comparer

        private static void InternalComparer(SerializableDictionary<int, Packet> olds,
                                             SerializableDictionary<int, Packet> news, IDictionary<int, List<int>> to)
        {
            foreach (var oldPacketPair in olds)
            {
                var oldPacket = oldPacketPair.Value;
                var points = 0;
                var packetIds = new List<int>();

                foreach (var newPacketPair in news)
                {
                    var cpoints = 0;
                    var newPacket = newPacketPair.Value;

                    if (oldPacket.DelegateFunction == newPacket.DelegateFunction)
                    {
                        if (!oldPacket.DelegateFunction.StartsWith("_Safe"))
                        {
                            packetIds.Clear();
                            packetIds.Add(newPacketPair.Key);
                            break;
                        }
                    }
                    else if (oldPacket.DelegateFunction.StartsWith("_SafeStr_") && !newPacket.DelegateFunction.StartsWith("_SafeStr_"))
                        continue;

                    cpoints += GetPoints(oldPacket, newPacket);

                    if (points > cpoints) continue;

                    if (points == cpoints)
                    {
                        packetIds.Add(newPacketPair.Key);
                        continue;
                    }

                    points = cpoints;
                    packetIds.Clear();
                    packetIds.Add(newPacketPair.Key);
                }

                if (packetIds.Count > 1)
                {
                    points = 0;
                    int packetNearId = -1;

                    foreach (var packetId in packetIds)
                    {
                        var newPacket = news[packetId];

                        var cpoints =
                            oldPacket.NearBottomPacket.Sum(
                                nearOldPacket =>
                                    newPacket.NearBottomPacket.Sum(
                                        nearNewPacket => GetPoints(nearOldPacket, nearNewPacket))) +
                            oldPacket.NearTopPacket.Sum(
                                nearOldPacket =>
                                    newPacket.NearTopPacket.Sum(nearNewPacket => GetPoints(nearOldPacket, nearNewPacket)));

                        if (cpoints > points)
                        {
                            points = cpoints;
                            packetNearId = packetId;
                        }
                    }

                    if (packetNearId != -1)
                    {
                        packetIds.Clear();
                        packetIds.Add(packetNearId);
                    }
                }

                to.Add(oldPacketPair.Key, packetIds);
            }
        }

        #endregion


        private static int GetPoints(Packet oldPacket, Packet newPacket)
        {
            var points = 0;

            if (!oldPacket.Classes.Any() && !newPacket.Classes.Any()) points += 3;
            else if (string.Join(",", oldPacket.Classes) == string.Join(",", newPacket.Classes)) points += 3;

            if (!oldPacket.Open.Any() && !newPacket.Open.Any()) points += 3;
            else if (string.Join(",", oldPacket.Open) == string.Join(",", newPacket.Open)) points += 3;

            if (!oldPacket.Strings.Any() && !newPacket.Strings.Any()) points += 5;
            else if (string.Join(",", oldPacket.Strings) == string.Join(",", newPacket.Strings)) points += 5;

            if (!oldPacket.Calls.Any() && !newPacket.Calls.Any()) points += 3;
            else if (string.Join(",", oldPacket.Calls) == string.Join(",", newPacket.Calls)) points += 3;

            if (!oldPacket.Lines.Any() && !newPacket.Lines.Any()) points += 4;
            else if (string.Join(",", oldPacket.Lines) == string.Join(",", newPacket.Lines)) points += 4;

            if (!oldPacket.Supers.Any() && !newPacket.Supers.Any()) points += 3;
            else if (string.Join(",", oldPacket.Supers) == string.Join(",", newPacket.Supers)) points += 3;

            if (!oldPacket.Builders.Any() && !newPacket.Builders.Any()) points += 5;
            else points += string.Join(",", oldPacket.Builders) == string.Join(",", newPacket.Builders) ? 5 : 0;

            if (!oldPacket.Readers.Any() && !newPacket.Readers.Any()) points += 5;
            else points += string.Join(",", oldPacket.Readers) == string.Join(",", newPacket.Readers) ? 5 : 0;

            if (!oldPacket.FunctionsNames.Any() && !newPacket.FunctionsNames.Any()) points += 7;
            else if (string.Join(",", oldPacket.FunctionsNames) == string.Join(",", newPacket.FunctionsNames)) points += 7;

            if (string.Join(",", oldPacket.FunctionsOrders) == string.Join(",", newPacket.FunctionsOrders)) points += 6;

            if (oldPacket.ConditionalCount == newPacket.ConditionalCount) points += 4;

            if (oldPacket.ConditionalNegativeCount == newPacket.ConditionalNegativeCount) points += 4;

            if (oldPacket.ConditionalElseCount == newPacket.ConditionalElseCount) points += 4;

            if (oldPacket.KCount == newPacket.KCount) points += 4;

            if (oldPacket.EventsCount == newPacket.EventsCount) points += 4;

            if (oldPacket.ForCount == newPacket.ForCount) points += 4;

            if (oldPacket.ForeachCount == newPacket.ForeachCount) points += 4;

            if (oldPacket.WhileCount == newPacket.WhileCount) points += 4;

            if (oldPacket.SwitchCount == newPacket.SwitchCount) points += 4;

            if (oldPacket.CaseCount == newPacket.CaseCount) points += 4;

            if (oldPacket.DefaultCount == newPacket.DefaultCount) points += 3;

            if (oldPacket.LocalCount == newPacket.LocalCount) points += 4;

            if (oldPacket.ArgCount == newPacket.ArgCount) points += 4;

            if (oldPacket.ThisCount == newPacket.ThisCount) points += 4;

            if (oldPacket.PointCount == newPacket.PointCount) points += 5;

            if (oldPacket.IndexOfCount == newPacket.IndexOfCount) points += 4;

            if (oldPacket.GetValueCount == newPacket.GetValueCount) points += 4;

            if (oldPacket.References == newPacket.References) points += 4;

            if (oldPacket.IntegersCount == newPacket.IntegersCount) points += 4;

            if (oldPacket.StringsCount == newPacket.StringsCount) points += 4;

            if (oldPacket.BoolsCount == newPacket.BoolsCount) points += 4;

            if (oldPacket.ArrayCount == newPacket.ArrayCount) points += 4;

            if (oldPacket.NewCount == newPacket.NewCount) points += 4;

            if (oldPacket.ReturnNull == newPacket.ReturnNull) points += 4;

            if (oldPacket.ReturnFalse == newPacket.ReturnFalse) points += 4;

            if (oldPacket.ReturnTrue == newPacket.ReturnTrue) points += 4;

            if (oldPacket.ReturnTotal == newPacket.ReturnTotal) points += 4;

            if (oldPacket.SendCount == newPacket.SendCount) points += 4;

            if (oldPacket.DotsCount == newPacket.DotsCount) points += 4;

            if (oldPacket.OrCount == newPacket.OrCount) points += 4;

            if (oldPacket.AndCount == newPacket.AndCount) points += 4;

            if (oldPacket.NotCount == newPacket.NotCount) points += 4;

            if (oldPacket.BitAndCount == newPacket.BitAndCount) points += 4;

            if (oldPacket.NullCount == newPacket.NullCount) points += 4;

            if (oldPacket.Equal == newPacket.Equal) points += 4;

            if (oldPacket.ComparatorEqual == newPacket.ComparatorEqual) points += 4;

            if (oldPacket.ComparatorNotEqual == newPacket.ComparatorNotEqual) points += 4;

            if (oldPacket.ComparatorLower == newPacket.ComparatorLower) points += 4;

            if (oldPacket.ComparatorHigher == newPacket.ComparatorHigher) points += 4;

            if (oldPacket.ComparatorEqualOrLower == newPacket.ComparatorEqualOrLower) points += 4;

            if (oldPacket.ComparatorEqualOrHigher == newPacket.ComparatorEqualOrHigher) points += 4;

            if (oldPacket.FalseCount == newPacket.FalseCount) points += 4;

            if (oldPacket.TrueCount == newPacket.TrueCount) points += 4;

            if (oldPacket.RestCount == newPacket.RestCount) points += 4;

            if (oldPacket.SumCount == newPacket.SumCount) points += 4;

            if (oldPacket.LengthCount == newPacket.LengthCount) points += 4;

            if (oldPacket.AsCount == newPacket.RestCount) points += 4;

            if (oldPacket.IsCount == newPacket.SumCount) points += 4;

            if (oldPacket.InCount == newPacket.LengthCount) points += 4;

            return points;
        }
    }
}