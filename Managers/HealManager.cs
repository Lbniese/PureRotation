#region Revision info

/*
 * $Author: tumatauenga1980 $
 * $Date: 2013-07-10 06:24:32 -0700 (Wed, 10 Jul 2013) $
 * $ID$
 * $Revision: 1597 $
 * $URL: https://subversion.assembla.com/svn/purerotation/trunk/PureRotation/Managers/HealManager.cs $
 * $LastChangedBy: tumatauenga1980 $
 * $ChangesMade$
 */

#endregion Revision info

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using JetBrains.Annotations;
//using PureRotation.Core;
//using PureRotation.Helpers;
//using PureRotation.Settings.Settings;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
//using Lua = PureRotation.Helpers.Lua;

namespace AdvancedAI.Managers
{
    [UsedImplicitly]
    public static class HealManager
    {
        /*
           * Healing works like so, in order of being called
           *
           * GetInitialList - Return a list of initial objects for us to use.
           * RemoveFilter - Remove anything that doesn't belong in the list.
           * IncludeFilter - If you want to include units regardless of the remove filter
           * WeighFilter - Weigh each target in the list.
           *
           * There are some Singular methods in this class. credits to them for initial implemetation. -- wulf.
         */

        private static ulong myGuid; // cache this shit.
        public static Dictionary<WoWPlayer, double> PrioList;
        public static List<WoWObject> GetInitialList { get { return ObjectManager.ObjectList.Where(o => o is WoWPlayer).ToList(); } }
        private static IEnumerable<WoWPartyMember> GroupMembers { get { return !StyxWoW.Me.GroupInfo.IsInRaid ? StyxWoW.Me.GroupInfo.PartyMembers : StyxWoW.Me.GroupInfo.RaidMembers; } }

        #region Tunables

        // Targets
        public static WoWPlayer Tank;
        public static WoWPlayer HealTarget;
        public static WoWPlayer SpecialTarget;
        public static WoWPlayer AoeHealTarget;

        //Settings for making target queries
        public const int MaxHealth = 95;
        private const int MaxHealingDistance = 40;
        public const int MaxHealingDistanceSqr = MaxHealingDistance * MaxHealingDistance;
        private const int MaxAoEHealingDistance = 30;
        public static int DispelCount = 0;
        public static int buffCount = 0;
        public static bool ShouldAOE = false;
        public static bool amHolyPally;

        public static WoWPoint AoeHealPoint = WoWPoint.Empty;
        private static int AoEUnitCount { get { return 3; } }
        private static int AoEHealthPercent { get { return 90; } }
        #endregion Tunables

        #region HealEngine

        public static void Initialize()
        {
            myGuid = StyxWoW.Me.Guid; // cache it.
            amHolyPally = StyxWoW.Me.Specialization == WoWSpec.PaladinHoly;

            PrioList = new Dictionary<WoWPlayer, double>();

        }

        private static int _throttle;
        public static Composite PulseHealManager
        {
            get
            {
                return new Action(delegate
                    {
                        
                        //Populate Healtarget
                        var target = CachedUnits.HealList.OrderByDescending(u => WeighFilter(u.ToPlayer())).FirstOrDefault() as WoWPlayer;
                        HealTarget = target ?? StyxWoW.Me;


                        _throttle++;
                        if (_throttle == 1)

                        //Check if we should AOE Heal
                        ShouldAOE = AOECheck;

                        //If so, set the targets. Should save us an O(n²) search during single target healing.
                        if (ShouldAOE)
                        {
                            //Set AOE HealTarget
                            AoeHealTarget = AOEHealLocation(MaxAoEHealingDistance);

                            //Set AOE HealPoint
                            AoeHealPoint = AOEHealLocation(AoeHealTarget);
                        }
                        else
                            AoeHealPoint = WoWPoint.Empty;

                            // Cheep throttle here because both DispelTarget and SpecialTarget return null if there isnt a target, so we cant really cache them.
                        {
                            // Our Special Target
                            SpecialTarget = GetSpecialTarget;

                            if (StyxWoW.Me.Class == WoWClass.Priest)
                                DispelCount = CachedUnits.MassDispelUnits.Count();
                        }
                        if (_throttle == 75) _throttle = 0;

                        return RunStatus.Failure;
                    });
            }
        }

        private static double WeighFilter(WoWPlayer p)
        {
            if (p == null) return -500f;

            double prio = p.IsAlive ? 500f : -500f;
            double HP = p.HealthPercent;


            // The more health they have, the lower the score.
            // This should give -500 for units at 100%
            // And -50 for units at 10%
            prio -= HP * 5;

            // Give tanks more weight. If the tank dies, we all die. KEEP HIM UP.
            if (CachedUnits.TankList.Contains(p) && HP != 100 &&
                // Ignore giving more weight to the tank if we have Beacon of Light on it.
                (!amHolyPally ||
                 !p.Auras.Any(a => a.Key == "Beacon of Light" && a.Value.CreatorGuid == myGuid)))
            {
                prio += 100f;
            }

            if (HP >= MaxHealth)
            {
                prio -= 500f;
            }


            PrioList[p] = prio;


            return prio;
        }

        #endregion HealEngine

        #region Priority Checks

        private static bool HasHealingModifier(WoWUnit u)
        {
            // -- Checking if unit receives more healing
            return u.HasAuraWithEffect(WoWApplyAuraType.ModHealingReceived, -20, 10, 200);  // unit receives less healing , -20, -10, 200
        }

        #endregion Priority Checks

        #region AoE

        private static WoWPlayer AOEHealLocation(int dist)
        {
                WoWPlayer pt = StyxWoW.Me;

                if (Tank != null)
                    pt = Tank;

                var currentPtCount = PeopleAroundPoint(pt.Location, dist);
                var tempCount = 0;
                foreach (WoWPlayer p in CachedUnits.HealList)
                {
                    tempCount = PeopleAroundPoint(p.Location, dist);
                    if (!p.IsMe && tempCount > currentPtCount)
                    {
                        pt = p;
                        currentPtCount = tempCount;
                    }
                }

                return tempCount >= AoEUnitCount ? pt : null;
        }

        private static WoWPoint AOEHealLocation(WoWPlayer p)
        {
            return p != null ? p.Location : WoWPoint.Empty;
        }

        public static bool AOECheck
        {
            get { return getAOEHealCount(AoEHealthPercent, MaxAoEHealingDistance) >= AoEUnitCount; }
        }

        private static int PeopleAroundPoint(WoWPoint pt, int dist)
        {
                var maxDistance = dist * dist;
                return CachedUnits.HealList.Count(p => pt.DistanceSqr(p.Location) <= maxDistance && p.ToPlayer().HealthPercent <= AoEHealthPercent);
        }

        public static int getAOEHealCount(int hp, int dist)
        {
                var maxDistance = dist * dist;
                return CachedUnits.HealList.Count(p => p.Location.DistanceSqr(StyxWoW.Me.Location) <= maxDistance && p.ToPlayer().HealthPercent <= hp);
        }

        #endregion AoE

        #region SpecialTarget

        private static WoWPlayer GetSpecialTarget
        {
            get
            {
                    if (TalentManager.CurrentSpec == WoWSpec.PaladinHoly)
                        return GetUnbuffedTarget("Beacon of Light");

                    if (TalentManager.CurrentSpec == WoWSpec.DruidRestoration)
                    {
                        WoWPlayer tar = GetBuffedTarget("Regrowth") ?? GetBuffedTarget("Rejuvenation");
                        return tar;
                    }

                    if (TalentManager.CurrentSpec == WoWSpec.MonkMistweaver)
                        return GetBuffedTarget("Soothing Mist");

                    return null;
            }
        }

        private static WoWPlayer GetBuffedTarget(string withBuff)
        {
            return CachedUnits.HealList.FirstOrDefault(unit => unit != null && unit.ToPlayer().CachedHasAura(withBuff)) as WoWPlayer;
        }

        private static WoWPlayer GetUnbuffedTarget(string withoutBuff)
        {

            return CachedUnits.HealList.Where(u => u != null && !u.ToPlayer().CachedHasAura(withoutBuff)).OrderByDescending(u => WeighFilter(u.ToPlayer())).FirstOrDefault() as WoWPlayer;
        }

        internal static WoWPlayer GetUnbuffedTarget(int withoutBuff)
        {

            return CachedUnits.HealList.Where(u => u != null && !u.ToPlayer().CachedHasAura(withoutBuff)).OrderByDescending(u => WeighFilter(u.ToPlayer())).FirstOrDefault() as WoWPlayer; ;
        }

        internal static WoWPlayer GetUnbuffedTarget(HashSet<int> withoutBuff)
        {

            return CachedUnits.HealList.Where(u => u != null && !u.ToPlayer().CachedHasAnyAura(withoutBuff)).OrderByDescending(u => WeighFilter(u.ToPlayer())).FirstOrDefault() as WoWPlayer; ;
        }

        // Counts (cached) to blanket check the raid for auras -- like renewing mist
        internal static int CountUnitAura(HashSet<int> aura)
        {
            return CachedUnits.HealList.Count(unit => !unit.ToPlayer().CachedHasAnyAura(aura));
        }

        internal static int CountUnitAura(int aura)
        {
            return CachedUnits.HealList.Count(unit => !unit.ToPlayer().CachedHasAura(aura));
        }

        internal static int CountUnitAura(string aura)
        {
            return CachedUnits.HealList.Count(unit => !unit.ToPlayer().CachedHasAura(aura));
        }

        internal static bool TimeleftUnitAura(HashSet<int> aura, int msLeft)
        {
            return CachedUnits.HealList.Any(unit => unit.ToPlayer().CachedGetAuraTimeLeft(aura, true) <= msLeft);
        }

        #endregion SpecialTarget

        #region Tanks

        public static WoWPlayer GetTank
        {
            get
            {
                // Got a Focus Tank?
                if (StyxWoW.Me.FocusedUnit != null)
                {
                    var focustank = CachedUnits.HealList.FirstOrDefault(p => p.Guid == StyxWoW.Me.FocusedUnit.Guid);
                    if (focustank != null && focustank.IsValid)
                    {
                        return focustank.ToPlayer();
                    }
                }

                // Using Lazy Raider ?
                if (RaFHelper.Leader != null && RaFHelper.Leader.CurrentHealth > 1 && RaFHelper.Leader != StyxWoW.Me)
                {
                    var raFHelpertank = CachedUnits.HealList.FirstOrDefault(p => p.Guid == RaFHelper.Leader.Guid);
                    if (raFHelpertank != null && raFHelpertank.IsValid)
                    {
                        return raFHelpertank.ToPlayer();
                    }
                }

                // We in a raid?, lets see if we can get the mainTank as specified by Blizzard (this is decided by iLvL)
                var maintTank = CachedUnits.TankList.FirstOrDefault(p => p.IsAlive && p.IsValid && p.Distance < 40 && p.IsMainTank());

                if (maintTank != null) return maintTank;

                // We in a raid?, Main Tank may be dead, lets get the OffTank.
                var offTank = CachedUnits.TankList.FirstOrDefault(p => p.IsAlive && p.IsValid && p.Distance < 40 && p.IsAssistTank());

                if (offTank != null) return offTank;
                
                // Hmm ok..we must be in a Party (5 man) lets query the tank by role.
                var partyTank = CachedUnits.TankList.FirstOrDefault(p => p.IsAlive && p.IsValid && p.Distance < 40);
                if (partyTank != null) return partyTank;

                // Damn couldnt find a tank ima be the boss!
                return StyxWoW.Me;
            }
        }

        // Warning not cached.
        internal static List<WoWPlayer> Tanks
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return new List<WoWPlayer>();

                return GroupMembers.Where(p => p.HasRole(WoWPartyMember.GroupRole.Tank))
                    .Select(p => p.ToPlayer())
                    .Where(p => p != null).ToList();
            }
        }

        public static bool IsTank(WoWPlayer p)
        {
            return CachedUnits.TankList.Contains(p);
        }

        // Warning not cached.
        public static HashSet<ulong> GetMainTankGuids()
        {
            var infos = GroupMembers;

            return new HashSet<ulong>(
                from pi in infos
                where (pi.Role & WoWPartyMember.GroupRole.Tank) != 0
                select pi.Guid);
        }

        #endregion Tanks

        #region Healers

        // Warning not cached.
        internal static List<WoWPlayer> Healers
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return new List<WoWPlayer>();

                return GroupMembers.Where(p => p.HasRole(WoWPartyMember.GroupRole.Healer))
                    .Select(p => p.ToPlayer())
                    .Where(p => p != null).ToList();
            }
        }

        public static bool IsHealer(WoWPlayer p)
        {
            return CachedUnits.HealerList.Contains(p);
        }

        #endregion

        #region Target Checks

        public static bool IsHealTargetOk { get { try { return HealTarget != null && HealTarget.HealthPercent > MaxHealth; } catch {} return false; } }
        public static bool IsSpecialTargetOk { get { try { return SpecialTarget != null && SpecialTarget.HealthPercent > MaxHealth; } catch { } return false; } }
        public static bool IsTankOk { get { try { return Tank != null && Tank.HealthPercent > MaxHealth; } catch { } return false; } }

        internal static bool HealthCheck(this WoWUnit unit, int hp)
        {
            return unit != null && unit.HealthPercent <= hp;
        }

        #endregion

        #region IncomingHealz

        public static uint GetPredictedHealth(WoWUnit unit, bool includeHoT = false, bool includeMyHeals = false)
        {
            return unit.GetPredictedHealth(includeMyHeals);
        }

        public static int UnitGetTotalAbsorbs(WoWUnit unit)
        {
            return unit.TotalAbsorbs;
        }

        public static float GetPredictedHealthPercent(WoWUnit unit, bool includeHoT = false, bool includeMyHeals = false)
        {
            return (float)unit.GetPredictedHealthPercent(includeMyHeals);
        }

        public static float GetIncomingPercent(WoWUnit unit, bool includeHoT = false, bool includeMyHeals = false)
        {
            return (float)unit.HealthPercent - GetPredictedHealthPercent(unit, includeHoT, includeMyHeals);
        }

        #endregion IncomingHealz
    }
}