#region Copyright
//Copyright 2012 Phelon Aka. Jon H.
/*
    This file is part of BGBuddy - Ultimate PVP Suite.

    BGBuddy - Ultimate PVP Suite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BGBuddy - Ultimate PVP Suite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedAI.Helpers;
using Styx.Common;
using Styx;
using Styx.CommonBot.POI;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals;
using Styx.Pathing;
using Styx.Common.Helpers;

namespace AdvancedAI.Helpers
{
    public static class TargetingPvP
    {
        private static WoWPlayer Me = StyxWoW.Me;
        public static WoWUnit Target;
        public static WoWUnit CurrentTarget = StyxWoW.Me;
        public static string CurrentTargetType;
        private static Dictionary<string, int> TargetList = new Dictionary<string, int>();
        private static List<string> MasterList;

        private readonly static WaitTimer TargetTimer = new WaitTimer(TimeSpan.FromMilliseconds(10000));
        public static bool TargetPulse()
        {
            return Targeting();
        }

        private static bool Targeting()
        {
            //Reset Timer
            if (TargetTimer.IsFinished)
            {
                TargetTimer.Reset();
            }
            else
            {
                return false;
            }
            //Clear Target
            Target = Me.CurrentTarget;
            if (Me.CurrentTarget != null)
            {
                if (Target.Mounted) return false;
                if (!Target.IsHostile) return false;
                if (!Target.Attackable) return false;
            }
            if (Me.CurrentTarget == null || !Me.CurrentTarget.IsAlive || CurrentTarget == Me || CurrentTarget == null)
            {
                CurrentTarget = Me;
                CurrentTargetType = "";
            }
            //Add list as Needed
            if (TargetList.Count < 1)
                TargetDictionary();
            //Sort the List
            List<KeyValuePair<string, int>> sorted = (from target in TargetList orderby target.Value descending select target).ToList();
            //Targetting
            foreach (KeyValuePair<string, int> target in sorted)
            {
                if (target.Key == "Closest" && TargetClosest())
                {
                    CurrentTargetType = "Closest";
                    return true;
                }
                if (target.Key == "FlagCarriers" && TargetFlagCarrier())
                {
                    CurrentTargetType = "FlagCarriers";
                    return true;
                }
                if (target.Key == "Healer" && TargetHealers())
                {
                    CurrentTargetType = "Healer";
                    return true;
                }
                if (target.Key == "LowHealth" && TargetLowHealth())
                {
                    CurrentTargetType = "LowHealth";
                    return true;
                }
                if (target.Key == "Undergeared" && TargetUndergeared())
                {
                    CurrentTargetType = "Undergeared";
                    return true;
                }
                if (target.Key == "Totems" && TargetTotems())
                {
                    CurrentTargetType = "Totems";
                    return true;
                }
            }
            return false;
        }

        private static void TargetDictionary()
        {
            //Priorities
            TargetList.Add("Closest", Convert.ToInt32(80));
            TargetList.Add("Demolishers", Convert.ToInt32(80));
            TargetList.Add("FlagCarriers", Convert.ToInt32(80));
            TargetList.Add("Healer", Convert.ToInt32(100));
            TargetList.Add("LowHealth", Convert.ToInt32(60));
            TargetList.Add("Undergeared", Convert.ToInt32(40));
            TargetList.Add("Totems", Convert.ToInt32(100));

        }

        public static bool TargetExists()
        {
            WoWUnit playerNear = ObjectManager.GetObjectsOfType<WoWUnit>().Where(
                                u => u.IsHostile && u.DistanceSqr <= 55 * 55 && u.IsPlayer && u.InLineOfSight).OrderBy(u => u.Distance).
                                FirstOrDefault();
            if (playerNear != null && !StyxWoW.Me.IsActuallyInCombat)
            {
                CurrentTarget = playerNear;
                return true;
            }
            return false;
        }

        public static void GetInCombat()
        {
            Logging.Write("Trying to Force Combat. Switching to " + ((WoWUnit)Target).SafeName() + "!");
            Navigator.MoveTo(CurrentTarget.Location);
            BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
            CurrentTarget.Target();
        }

        public static bool TargetClosest()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "Closest" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 15
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }

                //Closest!
                Target = ClosestEnemy();
                if (Target != null)
                {
                    if (Target.Guid != CurrentTarget.Guid)
                    {
                        Logging.Write("Closest Enemy Spotted! Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TargetFlagCarrier()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "FlagCarriers" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 30
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }

                // If there is a flag carrier return the flag carrier
                if (StyxWoW.Me.IsHorde) { Target = EnemyHordeFlagCarrier(); }
                if (StyxWoW.Me.IsAlliance) { Target = EnemyAllianceFlagCarrier(); }
                if (Target != null)
                {
                    if (Target.Guid != CurrentTarget.Guid)
                    {
                        Logging.Write("Flag Carrier Spotted! Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TargetHealers()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "Healer" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 30
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }
                Target = ValidTarget("Healer", 30);
                //Target = EnemyHealer();
                if (Target != null)
                {
                    if (Target.Guid != CurrentTarget.Guid)
                    {
                        Logging.Write("Healer Spotted!. Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TargetLowHealth()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "LowHealth" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 15
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }
                // Lowest Health Enemy
                Target = EnemyLowestHealth();
                if (Target != null)
                {
                    if (Target.Guid != CurrentTarget.Guid)
                    {
                        Logging.Write("Low Health Spotted!. Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TargetUndergeared()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "Undergeared" && StyxWoW.Me.CurrentTarget.IsAlive
                        && StyxWoW.Me.CurrentTarget.Distance <= 20
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }
                // Target Newbie
                Target = EnemyUndergeared();
                if (Target != null)
                {
                    if (StyxWoW.Me.CurrentTarget == null)
                    {
                        Logging.Write("Lowest Overall Health Spotted!. Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TargetTotems()
        {
            if (true)
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    if (CurrentTargetType == "Totems"
                        && StyxWoW.Me.CurrentTarget.Distance <= 10
                        && CurrentTarget.Guid == StyxWoW.Me.CurrentTarget.Guid)
                    {
                        BotPoi.Current = new BotPoi(CurrentTarget, PoiType.Kill);
                        Target.Target();
                        return true;
                    }
                }
                // Target Totem
                Target = EnemyTotem();
                if (Target != null)
                {
                    if (Target.Guid != CurrentTarget.Guid)
                    {
                        Logging.Write(Target.Name.ToString() + " Spotted!. Switching to " + ((WoWUnit)Target).SafeName() + "!");
                        BotPoi.Current = new BotPoi(Target, PoiType.Kill);
                        CurrentTarget = Target;
                        Target.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        private static WoWUnit EnemyTotem()
        {
            string[] TotemNames = { "Mana Tide Totem", "Earthbind Totem", "Tremor Totem", "Grounding Totem", "Cleansing Totem", "Spirit Link Totem" };

            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                        where Unit.CreatedByUnitGuid != StyxWoW.Me.Guid
                        where Unit.CreatureType == WoWCreatureType.Totem
                        where Unit.Distance < 10
                        where !Unit.IsFriendly
                        where TotemNames.Contains(Unit.Name.ToString())
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        select Unit).FirstOrDefault();
            }
        }

        private static WoWPlayer ClosestEnemy()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < 15
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where !Unit.HasAura("Spirit of Redemption")
                        where !Unit.HasAura("Blessing of Protection")
                        where !Unit.HasAura("Divine Shield")
                        where !Unit.HasAura("Ice Block")
                        where !Unit.HasAura("Cyclone")
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        select Unit).OrderBy(u => u.Distance).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyLowestHealth()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        orderby Unit.HealthPercent
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < 15
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where !Unit.HasAura("Spirit of Redemption")
                        where !Unit.HasAura("Blessing of Protection")
                        where !Unit.HasAura("Divine Shield")
                        where !Unit.HasAura("Ice Block")
                        where !Unit.HasAura("Cyclone")
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        where Unit.HealthPercent < 35
                        select Unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyUndergeared()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        orderby Unit.MaxHealth
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < 20
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where !Unit.HasAura("Spirit of Redemption")
                        where !Unit.HasAura("Blessing of Protection")
                        where !Unit.HasAura("Divine Shield")
                        where !Unit.HasAura("Ice Block")
                        where !Unit.HasAura("Cyclone")
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        select Unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyAllianceFlagCarrier()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < 30
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where Unit.HasAura("Alliance Flag")
                        where Unit.InLineOfSight
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        select Unit).FirstOrDefault();
            }
        }

        private static WoWPlayer EnemyHordeFlagCarrier()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < 30
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where Unit.HasAura("Horde Flag")
                        select Unit).FirstOrDefault();
            }
        }

        public static WoWPlayer ValidTarget(string role, int range)
        {
            PlayerSpecCheck();
            using (StyxWoW.Memory.AcquireFrame())
            {
                return (from Unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                        orderby Unit.MaxHealth
                        where Unit.IsAlive
                        where Unit.IsPlayer
                        where Unit.Distance < range
                        where !Unit.IsFriendly
                        where !Unit.IsPet
                        where !Unit.HasAura("Spirit of Redemption")
                        where !Unit.HasAura("Blessing of Protection")
                        where !Unit.HasAura("Divine Shield")
                        where !Unit.HasAura("Iceblock")
                        where !Unit.HasAura("Cyclone")
                        where !Unit.HasAura("Cloak of Shadows")
                        where !Unit.HasAura("Anti-Magic Shell")
                        where Navigator.CanNavigateFully(StyxWoW.Me.Location, Unit.Location)
                        where CheckHealerList(Unit) && role == "Healer" ||
                        CheckCasterList(Unit) && role == "Caster" ||
                        CheckMeleeList(Unit) && role == "Melee" ||
                        CheckTankList(Unit) && role == "Tank"
                        select Unit).FirstOrDefault();
            }
        }

        private static List<string> HealerList = new List<string>();
        private static List<string> CasterList = new List<string>();
        private static List<string> MeleeList = new List<string>();
        private static List<string> TankList = new List<string>();
        private static List<string> PlayerInfo = new List<string>();
        private static void PlayerSpecCheck()
        {
            try
            {
                string Faction = "1";
                if (StyxWoW.Me.IsHorde)
                    Faction = "0";
                if (Lua.GetReturnVal<int>("return GetNumBattlefieldScores()", 0) > 0)
                {
                    PlayerInfo.Clear();
                    HealerList.Clear();
                    MeleeList.Clear();
                    TankList.Clear();
                    CasterList.Clear();
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        for (int i = 0; i <= Lua.GetReturnVal<int>("return GetNumBattlefieldScores()", 0); i++)
                        {
                            int p = 0;
                            string PlayerName = "";

                            PlayerInfo = Lua.GetReturnValues("return GetBattlefieldScore(" + i + ")");
                            foreach (string info in PlayerInfo)
                            {
                                p++;

                                if (p == 1)
                                {
                                    //Logger.Write("Name: " + PlayerName);
                                    PlayerName = info;
                                }
                                if (p == 6)
                                {
                                    if (info == Faction)
                                    {
                                        //Logger.Write("Faction: " + PlayerName);
                                        break;
                                    }
                                }
                                if (p == 9)
                                {
                                    if (info == "Rogue")
                                    {
                                        MeleeList.Add(PlayerName);
                                        break;
                                    }
                                    if (info == "Warlock" || info == "Mage")
                                    {
                                        CasterList.Add(PlayerName);
                                        break;
                                    }
                                }
                                if (p == 16)
                                {
                                    if (info.Contains("Resto") || info.Contains("Disc") || info.Contains("Holy"))
                                    {
                                        //Logger.Write("Adding " + PlayerName + " as a " + info + " healer!");
                                        HealerList.Add(PlayerName);
                                        break;
                                    }
                                    else if (info.Contains("Prot") || info.Contains("Blood"))
                                    {
                                        TankList.Add(PlayerName);
                                        break;
                                    }
                                    else if (info.Contains("Fury") || info.Contains("Arms") ||
                                        info.Contains("Frost") || info.Contains("Unholy") ||
                                        info.Contains("Retribution") || info.Contains("Feral") || info.Contains("Enhancement"))
                                    {
                                        MeleeList.Add(PlayerName);
                                        break;
                                    }
                                    else
                                    {
                                        CasterList.Add(PlayerName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exg) { Logging.Write("" + exg); }
        }

        /// <summary>
        /// Checks for Player in Healers List
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CheckHealerList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                foreach (string healer in HealerList)
                {
                    if (healer.Contains(unit.Name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks for Player in Casters List
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CheckCasterList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                foreach (string caster in CasterList)
                {
                    if (caster.Contains(unit.Name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks for Player in Melee List
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CheckMeleeList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                foreach (string melee in MeleeList)
                {
                    if (melee.Contains(unit.Name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks for Player in Tank List
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CheckTankList(WoWPlayer unit)
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                foreach (string tank in TankList)
                {
                    if (tank.Contains(unit.Name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
