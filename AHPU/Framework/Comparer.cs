using System;
using System.Collections.Generic;
using System.Linq;
using AHPU.Habbo;

namespace AHPU.Framework
{
    internal static class Comparer
    {
        public static Dictionary<int, List<int>> OutgoingIds = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> IncomingIds = new Dictionary<int, List<int>>();

        public static void Compare(HabboActionScript old, HabboActionScript news, int diff, bool useNear)
        {
            InternalComparer(old.OutgoingPackets, news.OutgoingPackets, OutgoingIds, diff, useNear);
            InternalComparer(old.IncomingPackets, news.IncomingPackets, IncomingIds, diff, useNear);
        }

        #region Main Comparer

        private static void InternalComparer(SerializableDictionary<int, Packet> olds,
                                             SerializableDictionary<int, Packet> news, IDictionary<int, List<int>> to,
                                             int diff, bool useNear)
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

                        if (diff != 0) cpoints += diff > 60 ? 6 : 10;
                    }
                    else if (oldPacket.DelegateFunction.StartsWith("_SafeStr_") &&
                             newPacket.DelegateFunction.StartsWith("_SafeStr_"))
                    {
                        if (diff != 0)
                        {
                            var oldSafeStrId = int.Parse(oldPacket.DelegateFunction.Replace("_SafeStr_", string.Empty));
                            var newSafeStrId = int.Parse(newPacket.DelegateFunction.Replace("_SafeStr_", string.Empty));

                            if (Math.Abs(oldSafeStrId - newSafeStrId) < 5) cpoints += 5;
                            else if (Math.Abs(oldSafeStrId - newSafeStrId) < 10) cpoints += 4;
                            else if (Math.Abs(oldSafeStrId - newSafeStrId) < diff) cpoints += 3;
                        }
                    }
                    else continue;

                    cpoints += GetPoints(oldPacket, newPacket, useNear);

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

                to.Add(oldPacketPair.Key, packetIds);
            }
        }

        #endregion

        #region Near Comparer
        /*
        private static int NearComparer(IEnumerable<Packet> oldList, ICollection<Packet> newList)
        {
            return 0;
            //return oldList.Sum(oldPacket => newList.Sum(newPacket => GetPoints(oldPacket, newPacket, false))) / 100;
        }*/

        private static int NearComparer(IEnumerable<Packet> oldList, ICollection<Packet> newList)
        {
            var points = 0;
            foreach (var oldPacket in oldList)
            {
                Packet equalPacket = null;

                foreach (var newPacket in newList)
                {
                    if (oldPacket.Classes.Any() != newPacket.Classes.Any() ||
                        oldPacket.Classes.Any(str => !newPacket.Classes.Contains(str)))
                        continue;
                    if (oldPacket.Open.Any() != newPacket.Open.Any() ||
                        oldPacket.Open.Any(str => !newPacket.Open.Contains(str)))
                        continue;
                    if (oldPacket.Strings.Any() != newPacket.Strings.Any() ||
                        oldPacket.Strings.Any(str => !newPacket.Strings.Contains(str)))
                        continue;
                    if (oldPacket.Calls.Any() != newPacket.Calls.Any() ||
                        oldPacket.Calls.Any(str => !newPacket.Calls.Contains(str)))
                        continue;
                    if (oldPacket.Supers.Any() != newPacket.Supers.Any() ||
                        oldPacket.Supers.Any(str => !newPacket.Supers.Contains(str)))
                        continue;
                    if (oldPacket.FunctionsNames.Any() != newPacket.FunctionsNames.Any() ||
                        oldPacket.FunctionsNames.Any(str => !newPacket.FunctionsNames.Contains(str)))
                        continue;

                    if (oldPacket.ConditionalCount != newPacket.ConditionalCount) continue;
                    if (oldPacket.ConditionalElseCount != newPacket.ConditionalElseCount) continue;
                    if (oldPacket.EventsCount != newPacket.EventsCount) continue;
                    if (oldPacket.ForCount != newPacket.ForCount) continue;
                    if (oldPacket.ForeachCount != newPacket.ForeachCount) continue;
                    if (oldPacket.WhileCount != newPacket.WhileCount) continue;
                    if (oldPacket.SwitchCount != newPacket.SwitchCount) continue;
                    if (oldPacket.CaseCount != newPacket.CaseCount) continue;
                    if (oldPacket.DefaultCount != newPacket.DefaultCount) continue;
                    if (oldPacket.LocalCount != newPacket.LocalCount) continue;
                    if (oldPacket.ArgCount != newPacket.ArgCount) continue;
                    if (oldPacket.PointCount != newPacket.PointCount) continue;
                    if (oldPacket.IndexOfCount != newPacket.IndexOfCount) continue;
                    if (oldPacket.GetValueCount != newPacket.GetValueCount) continue;
                    if (oldPacket.References != newPacket.References) continue;
                    if (oldPacket.IntegersCount != newPacket.IntegersCount) continue;
                    if (oldPacket.StringsCount != newPacket.StringsCount) continue;
                    if (oldPacket.BoolsCount != newPacket.BoolsCount) continue;
                    if (oldPacket.ArrayCount != newPacket.ArrayCount) continue;
                    if (oldPacket.NewCount != newPacket.NewCount) continue;
                    if (oldPacket.ReturnNull != newPacket.ReturnNull) continue;
                    if (oldPacket.ReturnFalse != newPacket.ReturnFalse) continue;
                    if (oldPacket.ReturnTrue != newPacket.ReturnTrue) continue;
                    if (oldPacket.ReturnTotal != newPacket.ReturnTotal) continue;
                    if (oldPacket.SendCount != newPacket.SendCount) continue;
                    if (oldPacket.OrCount != newPacket.OrCount) continue;
                    if (oldPacket.AndCount != newPacket.AndCount) continue;
                    if (oldPacket.NotCount != newPacket.NotCount) continue;
                    if (oldPacket.BitAndCount != newPacket.BitAndCount) continue;
                    if (oldPacket.NullCount != newPacket.NullCount) continue;

                    equalPacket = newPacket;
                    points += 1;
                }

                newList.Remove(equalPacket);
            }

            return points;
        }

        #endregion


        private static int GetPoints(Packet oldPacket, Packet newPacket, bool nears = true)
        {
            var points = 0;

            if (!oldPacket.Classes.Any() && !newPacket.Classes.Any()) points += 3;
            else if (string.Join(",", oldPacket.Classes) == string.Join(",", newPacket.Classes)) points += oldPacket.Classes.Count * 6;
            else
            {
                points +=
                    oldPacket.Classes.Where(classStr => newPacket.Classes.Contains(classStr))
                        .Sum(classStr => 5);
            }

            if (!oldPacket.Open.Any() && !newPacket.Open.Any()) points += 3;
            else if (string.Join(",", oldPacket.Open) == string.Join(",", newPacket.Open)) points += oldPacket.Open.Count * 4;
            else if (Math.Abs(oldPacket.Open.Count - newPacket.Open.Count) > 4) points -= 3;
            else
            {
                points +=
                    oldPacket.Open.Distinct()
                        .Where(str => oldPacket.Open.Count(s => s == str) == newPacket.Open.Count(s => s == str))
                        .Sum(str => 3);
            }

            if (!oldPacket.Strings.Any() && !newPacket.Strings.Any()) points += 5;
            else if (string.Join(",", oldPacket.Strings) == string.Join(",", newPacket.Strings)) points += oldPacket.Strings.Count * 6;
            else if (Math.Abs(oldPacket.Strings.Count - newPacket.Strings.Count) > 4) points -= 3;
            else
            {
                points +=
                    oldPacket.Strings.Distinct()
                        .Where(
                            str =>
                                oldPacket.Strings.Count(s => s == str) == newPacket.Strings.Count(s => s == str))
                        .Sum(str => 5);
            }

            if (!oldPacket.Calls.Any() && !newPacket.Calls.Any()) points += 3;
            else if (string.Join(",", oldPacket.Calls) == string.Join(",", newPacket.Calls)) points += oldPacket.Calls.Count * 4;
            else if (Math.Abs(oldPacket.Calls.Count - newPacket.Calls.Count) > 4) points -= 3;
            else
            {
                points +=
                    oldPacket.Calls.Distinct()
                        .Where(
                            str => oldPacket.Calls.Count(s => s == str) == newPacket.Calls.Count(s => s == str))
                        .Sum(str => 3);
            }

            if (!oldPacket.Lines.Any() && !newPacket.Lines.Any()) points += 3;
            else if (string.Join(",", oldPacket.Lines) == string.Join(",", newPacket.Lines)) points += oldPacket.Lines.Count * 5;
            else if (Math.Abs(oldPacket.Lines.Count - newPacket.Lines.Count) > 4) points -= 3;
            else
            {
                points +=
                    oldPacket.Lines.Distinct()
                        .Where(
                            str => oldPacket.Lines.Count(s => s == str) == newPacket.Lines.Count(s => s == str))
                        .Sum(str => 5);
            }

            if (!oldPacket.Supers.Any() && !newPacket.Supers.Any()) points += 3;
            else if (string.Join(",", oldPacket.Supers) == string.Join(",", newPacket.Supers)) points += oldPacket.Supers.Count * 4;
            else if (Math.Abs(oldPacket.Supers.Count - newPacket.Supers.Count) > 4) points -= 3;
            else
            {
                points +=
                    oldPacket.Supers.Distinct()
                        .Where(
                            str =>
                                oldPacket.Supers.Count(s => s == str) == newPacket.Supers.Count(s => s == str))
                        .Sum(str => 3);
            }

            if (!oldPacket.Builders.Any() && !newPacket.Builders.Any()) points += 5;
            else points += string.Join(",", oldPacket.Builders) == string.Join(",", newPacket.Builders) ? 5 : 0;

            if (!oldPacket.Readers.Any() && !newPacket.Readers.Any()) points += 5;
            else points += string.Join(",", oldPacket.Readers) == string.Join(",", newPacket.Readers) ? 5 : 0;

            if (!oldPacket.FunctionsNames.Any() && !newPacket.FunctionsNames.Any()) points += 7;
            else if (string.Join(",", oldPacket.FunctionsNames) == string.Join(",", newPacket.FunctionsNames)) points += oldPacket.FunctionsNames.Count * 8;
            else
            {
                points +=
                    oldPacket.FunctionsNames.Where(classStr => newPacket.FunctionsNames.Contains(classStr))
                        .Sum(classStr => 7);
            }

            if (oldPacket.ConditionalCount == newPacket.ConditionalCount) points += 4;
            else if (oldPacket.ConditionalCount == 0 || newPacket.ConditionalCount == 0) points += 0;
            else if (Math.Abs(oldPacket.ConditionalCount - newPacket.ConditionalCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ConditionalCount - newPacket.ConditionalCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.ConditionalCount - newPacket.ConditionalCount) > 4) points -= 1;

            if (oldPacket.ConditionalElseCount == newPacket.ConditionalElseCount) points += 4;
            else if (oldPacket.ConditionalElseCount == 0 || newPacket.ConditionalElseCount == 0) points += 0;
            else if (Math.Abs(oldPacket.ConditionalElseCount - newPacket.ConditionalElseCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ConditionalElseCount - newPacket.ConditionalElseCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.ConditionalElseCount - newPacket.ConditionalElseCount) > 4) points -= 1;

            if (oldPacket.EventsCount == newPacket.EventsCount) points += 4;
            else if (oldPacket.EventsCount == 0 || newPacket.EventsCount == 0) points += 0;
            else if (Math.Abs(oldPacket.EventsCount - newPacket.EventsCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.EventsCount - newPacket.EventsCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.EventsCount - newPacket.EventsCount) > 4) points -= 1;

            if (oldPacket.ForCount == newPacket.ForCount) points += 4;
            else if (oldPacket.ForCount == 0 || newPacket.ForCount == 0) points += 0;
            else if (Math.Abs(oldPacket.ForCount - newPacket.ForCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ForCount - newPacket.ForCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.ForCount - newPacket.ForCount) > 4) points -= 1;

            if (oldPacket.ForeachCount == newPacket.ForeachCount) points += 4;
            else if (oldPacket.ForeachCount == 0 || newPacket.ForeachCount == 0) points += 0;
            else if (Math.Abs(oldPacket.ForeachCount - newPacket.ForeachCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ForeachCount - newPacket.ForeachCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.ForeachCount - newPacket.ForeachCount) > 4) points -= 1;

            if (oldPacket.WhileCount == newPacket.WhileCount) points += 4;
            else if (oldPacket.WhileCount == 0 || newPacket.WhileCount == 0) points += 0;
            else if (Math.Abs(oldPacket.WhileCount - newPacket.WhileCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.WhileCount - newPacket.WhileCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.WhileCount - newPacket.WhileCount) > 4) points -= 1;

            if (oldPacket.SwitchCount == newPacket.SwitchCount) points += 4;
            else if (oldPacket.SwitchCount == 0 || newPacket.SwitchCount == 0) points += 0;
            else if (Math.Abs(oldPacket.SwitchCount - newPacket.SwitchCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.SwitchCount - newPacket.SwitchCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.SwitchCount - newPacket.SwitchCount) > 4) points -= 1;

            if (oldPacket.CaseCount == newPacket.CaseCount) points += 4;
            else if (oldPacket.CaseCount == 0 || newPacket.CaseCount == 0) points += 0;
            else if (Math.Abs(oldPacket.CaseCount - newPacket.CaseCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.CaseCount - newPacket.CaseCount) > 4) points -= 1;

            if (oldPacket.DefaultCount == newPacket.DefaultCount) points += 3;
            else points -= 3;

            if (oldPacket.LocalCount == newPacket.LocalCount) points += 4;
            else if (Math.Abs(oldPacket.LocalCount - newPacket.LocalCount) > 8) points -= 1;
            else if (Math.Abs(oldPacket.LocalCount - newPacket.LocalCount) > 12) points -= 3;
            else if (Math.Abs(oldPacket.LocalCount - newPacket.LocalCount) > 15) points -= 4;
            else if (oldPacket.LocalCount == 0 || newPacket.LocalCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.LocalCount - newPacket.LocalCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.LocalCount - newPacket.LocalCount) < 3) points += 1;

            if (oldPacket.ArgCount == newPacket.ArgCount) points += 4;
            else if (Math.Abs(oldPacket.ArgCount - newPacket.ArgCount) > 8) points -= 1;
            else if (Math.Abs(oldPacket.ArgCount - newPacket.ArgCount) > 13) points -= 3;
            else if (Math.Abs(oldPacket.ArgCount - newPacket.ArgCount) > 20) points -= 4;
            else if (oldPacket.ArgCount == 0 || newPacket.ArgCount == 0) points += 0;
            else if (Math.Abs(oldPacket.ArgCount - newPacket.ArgCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ArgCount - newPacket.ArgCount) < 3) points += 1;

            if (oldPacket.PointCount == newPacket.PointCount) points += 5;
            else if (Math.Abs(oldPacket.PointCount - newPacket.PointCount) < 4) points += 2;
            else points -= 1;

            if (oldPacket.IndexOfCount == newPacket.IndexOfCount) points += 4;
            else if (oldPacket.IndexOfCount == 0 || newPacket.IndexOfCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.IndexOfCount - newPacket.IndexOfCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.IndexOfCount - newPacket.IndexOfCount) > 4) points -= 1;

            if (oldPacket.GetValueCount == newPacket.GetValueCount) points += 4;
            else if (oldPacket.GetValueCount == 0 || newPacket.GetValueCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.GetValueCount - newPacket.GetValueCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.GetValueCount - newPacket.GetValueCount) > 4) points -= 1;

            if (oldPacket.References == newPacket.References) points += 4;
            else if (oldPacket.References == 0 || newPacket.References == 0) points -= 1;
            else if (Math.Abs(oldPacket.References - newPacket.References) < 2) points += 2;
            else if (Math.Abs(oldPacket.References - newPacket.References) < 3) points += 1;
            else if (Math.Abs(oldPacket.References - newPacket.References) > 4) points -= 1;

            if (oldPacket.IntegersCount == newPacket.IntegersCount) points += 4;
            else if (oldPacket.IntegersCount == 0 || newPacket.IntegersCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.IntegersCount - newPacket.IntegersCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.IntegersCount - newPacket.IntegersCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.IntegersCount - newPacket.IntegersCount) > 4) points -= 1;

            if (oldPacket.StringsCount == newPacket.StringsCount) points += 4;
            else if (oldPacket.StringsCount == 0 || newPacket.StringsCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.StringsCount - newPacket.StringsCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.StringsCount - newPacket.StringsCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.StringsCount - newPacket.StringsCount) > 4) points -= 2;

            if (oldPacket.BoolsCount == newPacket.BoolsCount) points += 4;
            else if (oldPacket.BoolsCount == 0 || newPacket.BoolsCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.BoolsCount - newPacket.BoolsCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.BoolsCount - newPacket.BoolsCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.BoolsCount - newPacket.BoolsCount) > 4) points -= 1;

            if (oldPacket.ArrayCount == newPacket.ArrayCount) points += 4;
            else if (oldPacket.ArrayCount == 0 || newPacket.ArrayCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.ArrayCount - newPacket.ArrayCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.ArrayCount - newPacket.ArrayCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.ArrayCount - newPacket.ArrayCount) > 4) points -= 1;

            if (oldPacket.NewCount == newPacket.NewCount) points += 4;
            else if (oldPacket.NewCount == 0 || newPacket.NewCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.NewCount - newPacket.NewCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.NewCount - newPacket.NewCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.NewCount - newPacket.NewCount) > 4) points -= 1;

            if (oldPacket.ReturnNull == newPacket.ReturnNull) points += 4;
            else if (oldPacket.ReturnNull == 0 || newPacket.ReturnNull == 0) points -= 1;
            else if (Math.Abs(oldPacket.ReturnNull - newPacket.ReturnNull) < 2) points += 2;
            else if (Math.Abs(oldPacket.ReturnNull - newPacket.ReturnNull) < 3) points += 1;
            else if (Math.Abs(oldPacket.ReturnNull - newPacket.ReturnNull) > 4) points -= 1;

            if (oldPacket.ReturnFalse == newPacket.ReturnFalse) points += 4;
            else if (oldPacket.ReturnFalse == 0 || newPacket.ReturnFalse == 0) points -= 1;
            else if (Math.Abs(oldPacket.ReturnFalse - newPacket.ReturnFalse) < 2) points += 2;
            else if (Math.Abs(oldPacket.ReturnFalse - newPacket.ReturnFalse) < 3) points += 1;
            else if (Math.Abs(oldPacket.ReturnFalse - newPacket.ReturnFalse) > 4) points -= 1;

            if (oldPacket.ReturnTrue == newPacket.ReturnTrue) points += 4;
            else if (oldPacket.ReturnTrue == 0 || newPacket.ReturnTrue == 0) points -= 1;
            else if (Math.Abs(oldPacket.ReturnTrue - newPacket.ReturnTrue) < 2) points += 2;
            else if (Math.Abs(oldPacket.ReturnTrue - newPacket.ReturnTrue) < 3) points += 1;
            else if (Math.Abs(oldPacket.ReturnTrue - newPacket.ReturnTrue) > 4) points -= 1;

            if (oldPacket.ReturnTotal == newPacket.ReturnTotal) points += 4;
            else if (oldPacket.ReturnTotal == 0 || newPacket.ReturnTrue == 0) points -= 1;
            else if (Math.Abs(oldPacket.ReturnTotal - newPacket.ReturnTotal) < 2) points += 2;
            else if (Math.Abs(oldPacket.ReturnTotal - newPacket.ReturnTotal) < 3) points += 1;
            else if (Math.Abs(oldPacket.ReturnTotal - newPacket.ReturnTotal) > 4) points -= 1;

            if (oldPacket.SendCount == newPacket.SendCount) points += 4;
            else if (oldPacket.SendCount == 0 || newPacket.SendCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.SendCount - newPacket.SendCount) < 2) points += 2;
            else if (Math.Abs(oldPacket.SendCount - newPacket.SendCount) < 3) points += 1;
            else if (Math.Abs(oldPacket.SendCount - newPacket.SendCount) > 4) points -= 1;

            if (oldPacket.DotsCount == newPacket.DotsCount) points += 4;
            else if (Math.Abs(oldPacket.DotsCount - newPacket.SendCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.DotsCount - newPacket.DotsCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.DotsCount - newPacket.DotsCount) < 5) points += 1;
            else if (Math.Abs(oldPacket.DotsCount - newPacket.DotsCount) > 30) points -= 4;
            else if (Math.Abs(oldPacket.DotsCount - newPacket.DotsCount) > 80) points -= 6;

            if (oldPacket.OrCount == newPacket.OrCount) points += 4;
            else if (oldPacket.OrCount == 0 || newPacket.OrCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.OrCount - newPacket.OrCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.OrCount - newPacket.OrCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.OrCount - newPacket.OrCount) > 4) points -= 1;

            if (oldPacket.AndCount == newPacket.AndCount) points += 4;
            else if (oldPacket.AndCount == 0 || newPacket.AndCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.AndCount - newPacket.AndCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.AndCount - newPacket.AndCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.AndCount - newPacket.AndCount) > 4) points -= 1;

            if (oldPacket.NotCount == newPacket.NotCount) points += 4;
            else if (oldPacket.NotCount == 0 || newPacket.NotCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.NotCount - newPacket.NotCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.NotCount - newPacket.NotCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.NotCount - newPacket.NotCount) > 4) points -= 1;

            if (oldPacket.BitAndCount == newPacket.BitAndCount) points += 4;
            else if (oldPacket.BitAndCount == 0 || newPacket.BitAndCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.BitAndCount - newPacket.BitAndCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.BitAndCount - newPacket.BitAndCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.BitAndCount - newPacket.BitAndCount) > 4) points -= 1;

            if (oldPacket.NullCount == newPacket.NullCount) points += 4;
            else if (oldPacket.NullCount == 0 || newPacket.NullCount == 0) points -= 1;
            else if (Math.Abs(oldPacket.NullCount - newPacket.NullCount) < 2) points += 3;
            else if (Math.Abs(oldPacket.NullCount - newPacket.NullCount) < 3) points += 2;
            else if (Math.Abs(oldPacket.NullCount - newPacket.NullCount) > 4) points -= 1;

            if (nears)
            {
                if (!oldPacket.NearTopPacket.Any() && !newPacket.NearTopPacket.Any()) points += 4;
                else if (!oldPacket.NearTopPacket.Any() || !newPacket.NearTopPacket.Any()) points += 0;
                else if (Math.Abs(oldPacket.NearTopPacket.Count - newPacket.NearTopPacket.Count) > 3) points += 0;
                else points += NearComparer(oldPacket.NearTopPacket, newPacket.NearTopPacket.ToList());

                if (!oldPacket.NearBottomPacket.Any() && !newPacket.NearBottomPacket.Any()) points += 4;
                else if (!oldPacket.NearBottomPacket.Any() || !newPacket.NearBottomPacket.Any()) points += 0;
                else if (Math.Abs(oldPacket.NearBottomPacket.Count - newPacket.NearBottomPacket.Count) > 3) points += 0;
                else points += NearComparer(oldPacket.NearBottomPacket, newPacket.NearBottomPacket.ToList());
            }

            return points;
        }
    }
}