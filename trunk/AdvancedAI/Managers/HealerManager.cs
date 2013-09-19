
using System.Collections.Generic;
using System.Linq;
using AdvancedAI.Helpers;
using Styx;
using Styx.Common.Helpers;
using Styx.CommonBot;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using System;
using Styx.Common;

namespace AdvancedAI.Managers
{
    /*
     * Targeting works like so, in order of being called
     * 
     * GetInitialObjectList - Return a list of initial objects for the targeting to use.
     * RemoveTargetsFilter - Remove anything that doesn't belong in the list.
     * IncludeTargetsFilter - If you want to include units regardless of the remove filter
     * WeighTargetsFilter - Weigh each target in the list.     
     *
     */

    internal class HealerManager : Targeting
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        private static readonly WaitTimer _tankReset = WaitTimer.ThirtySeconds;

        // private static ulong _tankGuid;

        static HealerManager()
        {
            // Make sure we have a singleton instance!
            Instance = new HealerManager();
        }

        public new static HealerManager Instance { get; private set; }

        public static bool NeedHealTargeting { get; set; }

        private List<WoWUnit> HealList { get { return ObjectList.ConvertAll(o => o.ToUnit()); } }
        public static List<WoWObject> GetInitialList { get { return ObjectManager.ObjectList.Where(o => o is WoWPlayer).ToList(); } }
        private static IEnumerable<WoWPartyMember> GroupMembers { get { return !StyxWoW.Me.GroupInfo.IsInRaid ? StyxWoW.Me.GroupInfo.PartyMembers : StyxWoW.Me.GroupInfo.RaidMembers; } }

        protected override List<WoWObject> GetInitialObjectList()
        {
            // Targeting requires a list of WoWObjects - so it's not bound to any specific type of object. Just casting it down to WoWObject will work fine.
            return ObjectManager.ObjectList.Where(o => o is WoWPlayer).ToList();
        }

        protected override void DefaultIncludeTargetsFilter(List<WoWObject> incomingUnits, HashSet<WoWObject> outgoingUnits)
        {
            bool foundMe = false;
            bool isHorde = StyxWoW.Me.IsHorde;

            foreach (WoWObject incomingUnit in incomingUnits)
            {
                try
                {
                    if (incomingUnit.IsMe)
                        foundMe = true;

                    if (incomingUnit.ToPlayer().IsHorde != isHorde || !incomingUnit.ToPlayer().IsFriendly)
                        continue;

                    outgoingUnits.Add(incomingUnit);
                    if (/*SingularSettings.Instance.IncludePetsAsHealTargets &&*/ incomingUnit is WoWPlayer && incomingUnit.ToPlayer().GotAlivePet)
                        outgoingUnits.Add(incomingUnit.ToPlayer().Pet);
                }
                catch (System.AccessViolationException)
                {
                }
                catch (Styx.InvalidObjectPointerException)
                {
                }
            }

            if (!foundMe)
            {
                outgoingUnits.Add(StyxWoW.Me);
                if (/*SingularSettings.Instance.IncludePetsAsHealTargets &&*/ StyxWoW.Me.GotAlivePet)
                    outgoingUnits.Add(StyxWoW.Me.Pet);
            }

            if (StyxWoW.Me.FocusedUnit != null && StyxWoW.Me.FocusedUnit.IsFriendly && !StyxWoW.Me.FocusedUnit.IsPet && !StyxWoW.Me.FocusedUnit.IsPlayer)
                outgoingUnits.Add(StyxWoW.Me.FocusedUnit);

        }

        protected override void DefaultRemoveTargetsFilter(List<WoWObject> units)
        {
            bool isHorde = StyxWoW.Me.IsHorde;
            int maxHealRangeSqr;
            maxHealRangeSqr = 40 * 40;

            string[] _doNotHeal;
            _doNotHeal = new[] { "Reshape Life", "Parasitic Growth", "Cyclone", "Dominate Mind", "Agressive Behavior", "Beast of Nightmares", "Corrupted Healing" };

            for (int i = units.Count - 1; i >= 0; i--)
            {
                WoWUnit unit = units[i].ToUnit();
                try
                {
                    if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsFriendly || unit.HealthPercent <= 0 || unit.HasAnyAura(_doNotHeal))
                    {
                        units.RemoveAt(i);
                        continue;
                    }

                    WoWPlayer p = null;
                    if (unit is WoWPlayer)
                        p = unit.ToPlayer();
                    else if (unit.IsPet && unit.OwnedByRoot != null && unit.OwnedByRoot.IsPlayer)
                        p = unit.OwnedByRoot.ToPlayer();

                    if (p != null)
                    {
                        // Make sure we ignore dead/ghost players. If we need res logic, they need to be in the class-specific area.
                        if (p.IsGhost)
                        {
                            units.RemoveAt(i);
                            continue;
                        }

                        // They're not in our party/raid. So ignore them. We can't heal them anyway.
                        if (!p.IsInMyPartyOrRaid)
                        {
                            units.RemoveAt(i);
                            continue;
                        }
                        /*
                                            if (!p.Combat && p.HealthPercent >= SingularSettings.Instance.IgnoreHealTargetsAboveHealth)
                                            {
                                                units.RemoveAt(i);
                                                continue;
                                            }
                         */
                    }

                    // If we have movement turned off, ignore people who aren't in range.
                    // Almost all healing is 40 yards, so we'll use that. If in Battlegrounds use a slightly larger value to expane our 
                    // healing range, but not too large that we are running all over the bg zone 
                    // note: reordered following tests so only one floating point distance comparison done due to evalution of DisableAllMovement
                    if (unit.DistanceSqr > maxHealRangeSqr)
                    {
                        units.RemoveAt(i);
                        continue;
                    }
                }
                catch (System.AccessViolationException)
                {
                    units.RemoveAt(i);
                    continue;
                }
                catch (Styx.InvalidObjectPointerException)
                {
                    units.RemoveAt(i);
                    continue;
                }
            }
        }

        protected override void DefaultTargetWeight(List<TargetPriority> units)
        {
            var tanks = GetMainTankGuids();
            var inBg = Battlegrounds.IsInsideBattleground;
            var amHolyPally = StyxWoW.Me.Specialization == WoWSpec.PaladinHoly;

            foreach (TargetPriority prio in units)
            {
                WoWUnit u = prio.Object.ToUnit();
                if (u == null || !u.IsValid)
                {
                    prio.Score = -9999f;
                    continue;
                }

                // The more health they have, the lower the score.
                // This should give -500 for units at 100%
                // And -50 for units at 10%
                try
                {
                    prio.Score = u.IsAlive ? 500f : -500f;
                    prio.Score -= u.HealthPercent * 5;

                    // If they're out of range, give them a bit lower score.
                    if (u.DistanceSqr > 40 * 40)
                    {
                        prio.Score -= 50f;
                    }

                    // If they're out of LOS, again, lower score!
                    if (!u.InLineOfSpellSight)
                    {
                        prio.Score -= 100f;
                    }

                    // Give tanks more weight. If the tank dies, we all die. KEEP HIM UP.
                    if (tanks.Contains(u.Guid) && u.HealthPercent != 100 &&
                        // Ignore giving more weight to the tank if we have Beacon of Light on it.
                        (!amHolyPally || !u.Auras.Any(a => a.Key == "Beacon of Light" && a.Value.CreatorGuid == StyxWoW.Me.Guid)))
                    {
                        prio.Score += 100f;
                    }

                    // Give flag carriers more weight in battlegrounds. We need to keep them alive!
                    if (inBg && u.IsPlayer && u.Auras.Keys.Any(a => a.ToLowerInvariant().Contains("flag")))
                    {
                        prio.Score += 100f;
                    }
                }
                catch (System.AccessViolationException)
                {
                    prio.Score = -9999f;
                    continue;
                }
                catch (Styx.InvalidObjectPointerException)
                {
                    prio.Score = -9999f;
                    continue;
                }
            }
        }

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

        private static HashSet<ulong> GetMainTankGuids()
        {
            var infos = StyxWoW.Me.GroupInfo.RaidMembers;

            return new HashSet<ulong>(
                from pi in infos
                where (pi.Role & WoWPartyMember.GroupRole.Tank) != 0
                select pi.Guid);
        }

        /// <summary>
        /// finds the lowest health target in HealerManager.  HealerManager updates the list over multiple pulses, resulting in 
        /// the .FirstUnit entry often being at higher health than later entries.  This method dynamically searches the current
        /// list and returns the lowest at this moment.
        /// </summary>
        /// <returns></returns>
        public static WoWUnit FindLowestHealthTarget()
        {
#if LOWEST_IS_FIRSTUNIT
            return HealerManager.Instance.FirstUnit;
#else
            double minHealth = 999;
            WoWUnit minUnit = null;

            // iterate the list so we make a single pass through it
            //Test Cached Units
            foreach (WoWUnit unit in HealerManager.Instance.TargetList)
            {
                try
                {
                    if (unit.HealthPercent < minHealth && unit.Distance < 40)
                    {
                        minHealth = unit.HealthPercent;
                        minUnit = unit;
                    }
                }
                catch
                {
                    // simply eat the exception here
                }
            }

            return minUnit;
#endif
        }


        public static WoWUnit GetBestCoverageTarget(string spell, int health, int range, int radius, int minCount, SimpleBooleanDelegate requirements = null, IEnumerable<WoWUnit> mainTarget = null)
        {
            if (!Me.IsInGroup() || !Me.Combat)
                return null;

            if (!Spell.CanCastHack(spell, Me, skipWowCheck: true))
            {
                return null;
            }

            if (requirements == null)
                requirements = req => true;

            // build temp list of targets that could use heal and are in range + radius
            List<WoWUnit> coveredTargets = HealerManager.Instance.TargetList
                .Where(u => u.IsAlive && u.SpellDistance() < (range + radius) && u.HealthPercent < health && requirements(u))
                .ToList();


            // create a iEnumerable of the possible heal targets wtihin range
            IEnumerable<WoWUnit> listOf;
            if (range == 0)
                listOf = new List<WoWUnit>() { Me };
            else if (mainTarget == null)
                listOf = HealerManager.Instance.TargetList.Where(p => p.IsAlive && p.SpellDistance() <= range);
            else
                listOf = mainTarget;

            // now search list finding target with greatest number of heal targets in radius
            var t = listOf
                .Select(p => new
                {
                    Player = p,
                    Count = coveredTargets
                        .Where(pp => pp.IsAlive && pp.SpellDistance(p) < radius)
                        .Count()
                })
                .OrderByDescending(v => v.Count)
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (t != null)
            {
                if (t.Count >= minCount)
                {
                    Logging.WriteDiagnostic("GetBestCoverageTarget('{0}'): found {1} with {2} nearby under {3}%", spell, t.Player.SafeName(), t.Count, health);
                    //Logger.WriteDebug("GetBestCoverageTarget('{0}'): found {1} with {2} nearby under {3}%", spell, t.Player.SafeName(), t.Count, health);
                    return t.Player;
                }

            }

            return null;
        }

        /// <summary>
        /// find best Tank target that is missing Heal Over Time passed
        /// </summary>
        /// <param name="hotName">spell name of HoT</param>
        /// <returns>reference to target that needs the HoT</returns>
        public static WoWUnit GetBestTankTargetForHOT( string hotName, float health = 100f)
        {
            WoWUnit hotTarget = null;
            hotTarget = Group.Tanks.Where(u => u.IsAlive && u.Combat && u.HealthPercent < health && u.DistanceSqr < 40 * 40 && u.InLineOfSpellSight).OrderBy(u => u.HealthPercent).FirstOrDefault();
            if (hotTarget != null)
                Logging.WriteDiagnostic("GetBestTankTargetForHOT('{0}'): found tank {1} @ {2:F1}%, hasmyaura={3} with {4} ms left", hotName, hotTarget.SafeName(), hotTarget.HealthPercent, hotTarget.HasMyAura(hotName), (int)hotTarget.GetAuraTimeLeft(hotName).TotalMilliseconds);
            return hotTarget;
        }

        public static WoWUnit GetBestTankTargetForPWS(float health = 100f)
        {
            WoWUnit hotTarget = null;
            string hotName = "Power Word: Shield";
            string hotDebuff = "Weakened Soul";

            hotTarget = Group.Tanks.Where(u => u.IsAlive && u.Combat && u.HealthPercent < health && u.DistanceSqr < 40 * 40 && !u.HasAura(hotName) && !u.HasAura(hotDebuff) && u.InLineOfSpellSight).OrderBy(u => u.HealthPercent).FirstOrDefault();
            if (hotTarget != null)
                Logging.WriteDiagnostic("GetBestTankTargetForPWS('{0}'): found tank {1} @ {2:F1}%, hasmyaura={3} with {4} ms left", hotName, hotTarget.SafeName(), hotTarget.HealthPercent, hotTarget.HasMyAura(hotName), (int)hotTarget.GetAuraTimeLeft(hotName).TotalMilliseconds);
            return hotTarget;
        }

        public static WoWUnit GetSwiftmendTarget
        {
            get
            {
                try
                {
                    return (from unit in Unit.NearbyFriendlyPlayers
                            where unit.HasAnyAura("Rejuvenation", "Regrowth")
                            orderby SwiftmendPriority(unit) ascending
                            select unit).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Logging.WriteDiagnostic("GetSwiftmentTarget error");
                    return null;
                }
            }
        }

        private static double SwiftmendPriority(WoWUnit p)
        {
            return Unit.NearbyFriendlyPlayers.Count(u => u.Location.DistanceSqr(p.Location) < 8 * 8) * 1.0; 
        }

        public static WoWUnit Tank
        {
            get
            {
                try
                {
                    return (from unit in Unit.NearbyFriendlyPlayers
                            where unit.IsMainTank() || unit.IsAssistTank()
                            orderby unit.HealthPercent descending
                            select unit).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Logging.WriteDiagnostic("GetSwiftmentTarget error");
                    return null;
                }
            }
            set { throw new NotImplementedException(); }
        }

        public static WoWPlayer GetUnbuffedTarget(string withoutBuff)
        {
            //return Instance.TargetList.FirstOrDefault(u => u != null && !u.ToPlayer().HasAura(withoutBuff)) as WoWPlayer;
            return Unit.NearbyFriendlyPlayers.FirstOrDefault(u => u != null && !u.HasAura(withoutBuff));
        }

        public static int GetCountWithBuff(string withbuff)
        {
            //return Instance.TargetList.Count(u => u != null && u.ToPlayer().HasAura(withbuff));
            return Unit.NearbyFriendlyPlayers.Count(u => u != null && u.HasAura(withbuff));
        }

        public static int GetCountWithHealth(int health)
        {
            //return Instance.TargetList.Count(u => u != null && u.ToPlayer().HealthPercent < health);
            return Unit.NearbyFriendlyPlayers.Count(u => u != null && u.HealthPercent < health);
        }

        public static int GetCountWithBuffAndHealth(string withbuff, int health)
        {
            //return Instance.TargetList.Count(u => u != null && u.ToPlayer().HealthPercent < health && u.ToPlayer().HasAura(withbuff));
            return Unit.NearbyFriendlyPlayers.Count(u => u != null && u.HealthPercent < health && u.HasAura(withbuff));
        }
        
        //public static WoWUnit TankToMoveTowards
        //{
        //    get
        //    {
        //        if (!SingularSettings.Instance.StayNearTank)
        //            return null;

        //        if (RaFHelper.Leader != null && RaFHelper.Leader.IsValid && RaFHelper.Leader.IsAlive && RaFHelper.Leader.Distance < SingularSettings.Instance.MaxHealTargetRange)
        //            return RaFHelper.Leader;

        //        return Group.Tanks.Where(t => t.IsAlive && t.Distance < SingularSettings.Instance.MaxHealTargetRange).OrderBy(t => t.Distance).FirstOrDefault();
        //    }
        //}

        //public static Composite CreateStayNearTankBehavior()
        //{
        //    if (SingularRoutine.CurrentWoWContext != WoWContext.Instances)
        //        return new ActionAlwaysFail();

        //    return
        //        // no healing needed, then move within heal range of tank
        //        new Decorator(
        //            ret => HealerManager.TankToMoveTowards != null,
        //            new PrioritySelector(
        //                new Decorator(
        //                    ret => !HealerManager.TankToMoveTowards.InLineOfSpellSight,      
        //                    new Sequence(
        //                        Movement.CreateMoveToLosBehavior(on => HealerManager.TankToMoveTowards),
        //                        new ActionAlwaysFail()
        //                        )
        //                    ),
        //                new Decorator(
        //                    ret => HealerManager.TankToMoveTowards.Distance > 38f,
        //                    new Sequence(
        //                        Movement.CreateMoveToUnitBehavior( 30f, ret => HealerManager.TankToMoveTowards),
        //                        new ActionAlwaysFail()
        //                        )
        //                    )
        //                )
        //            );

        //}
    }

    class PrioritizedBehaviorList
    {
        class PrioritizedBehavior
        {
            public int Priority { get; set; }
            public string Name { get; set; }
            public Composite behavior { get; set; }

            public PrioritizedBehavior(int p, string s, Composite bt)
            {
                Priority = p;
                Name = s;
                behavior = bt;
            }
        }

        List<PrioritizedBehavior> blist = new List<PrioritizedBehavior>();

        public void AddBehavior(int pri, string behavName, string spellName, Composite bt)
        {
            if (pri == 0)
                Logging.WriteDiagnostic("Skipping Behavior [{0}] configured for Priority {1}", behavName, pri);
            else if (!String.IsNullOrEmpty(spellName) && !SpellManager.HasSpell(spellName))
                Logging.WriteDiagnostic("Skipping Behavior [{0}] since spell '{1}' is not known by this character", behavName, spellName);
            else
                blist.Add(new PrioritizedBehavior(pri, behavName, bt));
        }

        public void OrderBehaviors()
        {
            blist = blist.OrderByDescending(b => b.Priority).ToList();
        }

        public Composite GenerateBehaviorTree()
        {
            return new PrioritySelector(blist.Select(b => b.behavior).ToArray());
        }

        public void ListBehaviors()
        {
            foreach (PrioritizedBehavior hs in blist)
            {
                Logging.WriteDiagnostic("   Priority {0} for Behavior [{1}]", hs.Priority.ToString().AlignRight(4), hs.Name);
                //Logger.WriteDebug(Color.GreenYellow, "   Priority {0} for Behavior [{1}]", hs.Priority.ToString().AlignRight(4), hs.Name);
            }
        }
    }

}