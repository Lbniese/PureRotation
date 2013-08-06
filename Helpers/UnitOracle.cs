#region Revision info

/*
 * $Author: tumatauenga1980@gmail.com $
 * $Date: 2012-11-01 08:21:46 -0700 (Thu, 01 Nov 2012) $
 * $ID$
 * $Revision: 764 $
 * $URL: http://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/CLU/Helpers/UnitOracle.cs $
 * $LastChangedBy: tumatauenga1980@gmail.com $
 * $ChangesMade$
 */

#endregion Revision info

using System;
using System.Collections.Generic;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Helpers
{
    public class UnitOracle
    {
        private static Dictionary<ulong, UnitOracle> instances = new Dictionary<ulong, UnitOracle>();

        private const int TICKS_PER_SECOND = 4;

        [Flags]
        public enum Watch
        {
            None = 0,
            HealthVariance = 1

            //...2, 4, 8, 16...
        }

        private WoWUnit Unit;
        private Watch Flags;

        private const int WINDOW_SIZE = TICKS_PER_SECOND * 10;

        private CircularBuffer<double> health;

        public static UnitOracle WatchUnit(WoWUnit unit, Watch watchFlags)
        {
            try
            {
                return new UnitOracle(unit, watchFlags);
            }
            catch (NotSupportedException)
            {
                return instances[unit.Guid];
            }
        }

        private UnitOracle(WoWUnit unit, Watch watchFlags)
        {
            if (unit != null)
            {
                this.Unit = unit;

                Flags = watchFlags;

                if (instances.ContainsKey(unit.Guid))
                    throw new NotSupportedException();

                instances[unit.Guid] = this;

                if ((Flags & Watch.HealthVariance) == Watch.HealthVariance)
                {
                    health = new CircularBuffer<double>(WINDOW_SIZE);
                    for (var i = 0; i < WINDOW_SIZE; i++)
                        health.Enqueue((double)Unit.CurrentHealth);
                }
            }
        }

        private static DateTime lastPulse = DateTime.MinValue;
        private static int realTicksPerSeconds = TICKS_PER_SECOND;

        public static void Pulse()
        {
            double dt = lastPulse == DateTime.MinValue ? TICKS_PER_SECOND : DateTime.Now.Subtract(lastPulse).TotalSeconds;
            if (dt == 0.0)
            {
                dt = 1.0 / (double)TICKS_PER_SECOND;
            }

            foreach (var instance in instances.Values)
            {
                var unit = instance.Unit;
                var flags = instance.Flags;

                // cleanup
                if (!unit.IsValid)
                {
                    instance.Dispose();
                    continue;
                }

                // health variance
                switch (flags & Watch.HealthVariance)
                {
                    case Watch.HealthVariance:
                        try
                        {
                            instance.health.SafeEnqueue((double)unit.CurrentHealth);
                        }
                        catch
                        {
                            instance.health.SafeEnqueue(0.0);
                        }
                        break;
                }
            }

            lastPulse = DateTime.Now;
            realTicksPerSeconds = (int)(realTicksPerSeconds * 0.9 + 1.0 / dt * 0.1);

            //  CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": realTicksPerSeconds_pulse = " + realTicksPerSeconds + " dt = " + dt + " lastpulse = " + lastPulse);
        }

        public void Dispose()
        {
            instances.Remove(Unit.Guid);
        }

        public int HealthDelta(double deltaSeconds)
        {
            var ticks = (int)(realTicksPerSeconds * deltaSeconds);
            if (ticks > WINDOW_SIZE)
                ticks = WINDOW_SIZE;
            if (ticks < 2)
                ticks = 2;

            var lastValues = health.SafeGetLastValues(ticks);
            try
            {
                var dh = lastValues[lastValues.Length - 1] - lastValues[0];

                // CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": health.SafeGetLastValues(ticks) = [" + ticks + "] realTicksPerSeconds * [" + realTicksPerSeconds + "] deltaSeconds;"+ deltaSeconds+ " HealthDelta [" + (int)dh + "]");
                return (int)dh;
            }
            catch
            {
                //  CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": Woops, error in oracle's health delta...");
                return 0;
            }
        }

        // in seconds
        public int TimeToDie(double deltaSeconds)
        {
            var dhealth = HealthDelta(deltaSeconds);
            if (dhealth >= 0)
            {
                return 999999999;
            }
            var ticks = -this.Unit.CurrentHealth / dhealth;
            var seconds = ticks * realTicksPerSeconds;

            // CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": ticks [" + ticks + "] = -unit.CurrentHealth [" + -this.Unit.CurrentHealth + "] / dhealth [" + dhealth + "]");
            // CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": seconds [" + seconds + "] = ticks [" + ticks + "] * realTicksPerSeconds [" + realTicksPerSeconds + "]");
            return (int)seconds;
        }

        public static UnitOracle FindUnit(WoWUnit unit)
        {
            try
            {
                // CLU.DebugLog(Color.GreenYellow, "[CLU-ORACLE] " + CLU.Version + ": FindUnit [" + unit + "] = [" + unit.Guid + "]");
                return instances[unit.Guid];
            }
            catch
            {
                return null;
            }
        }
    }
}