using System;
using System.Collections.Generic;
using System.Linq;

using Styx.Common.Helpers;
using Styx.WoWInternals;

namespace AdvancedAI.Helpers
{
    class LibCandyBar
    {
        /*
         Current fields:
         * running = true/false
         * start = time started
         * exp = expire time
         * remaining = time left
         * candyBarBar = wrapper stuff
         * candyBarLabel = text for the bar. (not the timer!)
         * fill = whether the bar should drain (default) or fill up true/false
         * candybarIconFrame = icon frame
         * candyBarBar = status bar (actual bar itself)
         * candyBarBackground = texture
         * candyBarDuration = texture
         * updater = handles the events to set updates/animations
         
         * width
         * height
         * showTime = whether the time is shown on the bar
         */
    }


    internal class DeadlyBossMods
    {
        private static readonly Dictionary<string, TimerBar> BarCache = new Dictionary<string, TimerBar>();

        public static int NumBars { get { return Lua.GetReturnVal<int>("return DBM.Bars.numBars", 0); } }

        private static IEnumerable<string> BarIds
        {
            get
            {
                var barIds =
                    Lua.GetReturnVal<string>(
                        "t={} for bar in pairs(DBM.Bars.bars) do table.insert(t, bar.id) end return (table.concat(t,'@!@'))",
                        0);
                return barIds.Split(new[] {"@!@"}, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static IEnumerable<TimerBar> Bars
        {
            get
            {
                var barIds =
                    Lua.GetReturnVal<string>(
                        "t={} for bar in pairs(DBM.Bars.bars) do table.insert(t, bar.id) end return (table.concat(t,'@!@'))",
                        0);

                foreach (string barId in barIds.Split(new[] {"@!@"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return new TimerBar(barId);
                }
            }
        }

        public static TimerBar FindBarByPartialId(string id)
        {
            TimerBar bar;
            if (BarCache.TryGetValue(id, out bar))
            {
                // If the bar is dead, then remove it. We no longer want to track it.
                if (bar.Timer.IsFinished)
                {
                    BarCache.Remove(id);
                }
                else
                {
                    return bar;
                }
            }

            string firstMatching = BarIds.FirstOrDefault(bid => bid.Contains(id));
            if (!string.IsNullOrWhiteSpace(firstMatching))
            {
                bar = new TimerBar(firstMatching);
                BarCache.Add(id, bar);
                return bar;
            }

            return null;
        }

        private static string FindBarAndExecute(string id, string doStuff)
        {
            return string.Format("for bar in pairs(DBM.Bars.bars) do if '{0}' == bar.id then {1} end end",
                id,
                doStuff);
        }

        internal class TimerBar
        {
            private WaitTimer _timer;

            public TimerBar(string barId)
            {
                Id = barId;
            }

            public string Id { get; private set; }

            public bool Dead { get { return Lua.GetReturnVal<bool>(FindBarAndExecute(Id, "return bar.dead"), 0); } }
            public bool Dummy { get { return Lua.GetReturnVal<bool>(FindBarAndExecute(Id, "return bar.dummy"), 0); } }
            public bool Flashing { get { return Lua.GetReturnVal<bool>(FindBarAndExecute(Id, "return bar.flashing"), 0); } }
            public bool Enlarged { get { return Lua.GetReturnVal<bool>(FindBarAndExecute(Id, "return bar.enlarged"), 0); } }
            public bool FadingIn { get { return Lua.GetReturnVal<bool>(FindBarAndExecute(Id, "return bar.fadingIn"), 0); } }
            public float LuaTimeLeft { get { return Lua.GetReturnVal<float>(FindBarAndExecute(Id, "return bar.timer"), 0); } }
            public float LuaTotalTime { get { return Lua.GetReturnVal<float>(FindBarAndExecute(Id, "return bar.totalTime"), 0); } }

            public TimeSpan TotalTime { get { return TimeSpan.FromSeconds(LuaTotalTime); } }
            public TimeSpan TimeLeft { get { return TimeSpan.FromSeconds(LuaTimeLeft); } }

            public WaitTimer Timer { get { return _timer ?? (_timer = new WaitTimer(TimeLeft)); } }

            public void Cancel()
            {
                Lua.DoString(FindBarAndExecute(Id, "bar:Cancel()"));
            }

            public override string ToString()
            {
                return string.Format("Id: {0}, TotalTime: {1}, TimeLeft: {2}", Id, TotalTime, TimeLeft);
            }
        }
    }
}