#region Revision info

/*
 * $Author: tumatauenga1980 $
 * $Date: 2013-07-14 03:44:19 -0700 (Sun, 14 Jul 2013) $
 * $ID$
 * $Revision: 1601 $
 * $URL: https://subversion.assembla.com/svn/purerotation/trunk/PureRotation/Managers/CachedUnits.cs $
 * $LastChangedBy: tumatauenga1980 $
 * $ChangesMade$
 */

#endregion Revision info

using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using JetBrains.Annotations;
//using PureRotation.Core;
//using PureRotation.Helpers;
//using PureRotation.Settings.Settings;
using Styx;
using Styx.Common;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.WoWInternals.World;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Helpers
{
    [UsedImplicitly]
    class CachedUnits : CacheManager
    {
        // Cache tunning -- milliseconds
        private const int HEALLIST_EXPIRY = 500;
        private const int TANKLIST_EXPIRY = 500;
        private const int HEALERLIST_EXPIRY = 500;
        private const int TANK_EXPIRY = 500;
        private const int MASSDISPEL_EXPIRY = 500;
        private const int ATTACKABLEUNITS_EXPIRY = 250;
        
        // Cached Lists        
        public static List<WoWObject> HealList;
        public static List<WoWPlayer> TankList;
        public static List<WoWPlayer> HealerList;
        public static List<WoWUnit> AttackableUnits;
        public static List<WoWObject> MassDispelUnits;

        public static void Initialize()
        {
            if (!UpdateCache())
            { return; }
            
            // Instantiate our lists..
            HealList = new List<WoWObject>();
            TankList = new List<WoWPlayer>();
            HealerList = new List<WoWPlayer>();
            AttackableUnits = new List<WoWUnit>();
            MassDispelUnits = new List<WoWObject>();

            AttachCombatLogEvent();

            AdvancedAI.OnWoWContextChanged += HandleContextChanged;

            Logging.Write("CachedUnits created!");
        }

        public static void Pulse()
        {
            new Action(delegate
                {
                    UpdateCache();

                    return RunStatus.Failure;
                });
        }

        public static bool UpdateCache()
        {
            UpdateCachedPlayers();
            UpdateCachedUnits();
            return true;
        }

        private static void UpdateCachedPlayers()
        {
            HealList = CachedHealList;                  // cache healable units.
            RemoveFilter(HealList);                     // Remove Unnecassary healable units.
            IncludeFilter(HealList);                    // Inlcude healable units.
            TankList = CachedTankList;                  // cache the tanks.
            HealerList = CachedHealerList;              // cache the healers.
            HealerManager.Tank = CachedTank;              // cache the Tank - Focus, RaFHelper, MainTank, MainAssist, PartyTank, Me, in that order..
            //MassDispelUnits = CachedMassDispel;         // cache Mass Dispel Units.
        }

        private static void UpdateCachedUnits()
        {
            AttackableUnits = CacheAttackableUnits;     // cache Attackable Units.
        }

        #region Filtering

        // Search predicate returns true if a player has any of these conditions.
        private static bool BadPlayers(WoWObject o)
        {
            if (o.Distance > 40)
            {
                return true;
            }

            WoWPlayer p = o.ToPlayer();

            if (p == null)
            {
                return true;
            }

            if (!p.IsValid)
            {
                return true;
            }

            // TODO: Taxing check at the moment.

            //// Amber-Shaper Un'sok
            //if (p.CachedHasAura("Reshape Life"))
            //{
            //    return true;
            //}

            if (p.IsDead)
            {
                return true;
            }

            if (p.IsHostile)
            {
                return true;
            }

            if (p.DistanceSqr > 40)
            {
                return true;
            }

            if (p.IsGhost)
            {
                return true;
            }

            if (!p.IsMe && !p.IsInMyPartyOrRaid)
            {
                return true;
            }

            // TODO: This check needs to be made less intensive..its insanley expensive and sometimes not what we expect. 
            if (!p.IsMe && !p.InLineOfSight) // !GameWorld.IsInLineOfSpellSight(StyxWoW.Me.GetTraceLinePos(), p.Location
            {
                return true;
            }

            return false;
        }

        private static bool GoodPlayers(WoWObject o)
        {
            return o != null && o.IsMe;
        }

        private static void RemoveFilter(List<WoWObject> units)
        {
            Logging.WriteDiagnostic(" {0} players removed by RemoveAll(badPlayers).", units.RemoveAll(BadPlayers));
        }

        private static void IncludeFilter(List<WoWObject> units)
        {
            if (!units.Exists(GoodPlayers))
            {
                HealList.Add(StyxWoW.Me);
                Logging.WriteDiagnostic(" Exists(goodPlayers): {0}.", units.Exists(GoodPlayers));
            }
        }

        #endregion

        #region Cache Checks

        private static List<WoWObject> CachedHealList
        {
            get
            {
                const string CACHEKEY = "HealList";

                // Check the cache
                var healList = Get<List<WoWObject>>(CACHEKEY);

                if (healList == null)
                {
                    // Go and retrieve the data from the objectManager
                    healList = HealerManager.GetInitialList;

                    // Then add it to the cache so we
                    // can retrieve it from there next time
                    // set the object to expire
                    Add(healList, CACHEKEY, HEALLIST_EXPIRY); 
                }
                return healList;
            }
        }

        private static List<WoWPlayer> CachedTankList
        {
            get
            {
                const string CACHEKEY = "Tanks";

                // Check the cache
                var tankList = Get<List<WoWPlayer>>(CACHEKEY);

                if (tankList == null)
                {
                    // Go and retrieve the data from the objectManager
                    tankList = HealerManager.Tanks;

                    // Then add it to the cache so we
                    // can retrieve it from there next time
                    // set the object to expire
                    Add(tankList, CACHEKEY, TANKLIST_EXPIRY);
                }
                return tankList;
            }
        }

        private static List<WoWPlayer> CachedHealerList
        {
            get
            {
                const string CACHEKEY = "Healers";

                // Check the cache
                var healerList = Get<List<WoWPlayer>>(CACHEKEY);

                if (healerList == null)
                {
                    // Go and retrieve the data from the objectManager
                    healerList = HealerManager.Healers;

                    // Then add it to the cache so we
                    // can retrieve it from there next time
                    // set the object to expire
                    Add(healerList, CACHEKEY, HEALERLIST_EXPIRY);
                }
                return healerList;
            }
        }

        //private static List<WoWObject> CachedMassDispel
        //{
        //    get
        //    {
        //        const string CACHEKEY = "MassDispel";

        //         Check the cache
        //        var massDispelList = Get<List<WoWObject>>(CACHEKEY);

        //        if (massDispelList == null)
        //        {
        //             Go and retrieve the data from the objectManager
        //            massDispelList = DispelManager.MassDispel.ToList();

        //             Then add it to the cache so we
        //             can retrieve it from there next time
        //             set the object to expire
        //            Add(massDispelList, CACHEKEY, MASSDISPEL_EXPIRY);
        //        }
        //        return massDispelList;
        //    }
        //}

        private static List<WoWUnit> CacheAttackableUnits
        {
            get
            {
                const string CACHEKEY = "AttackableUnits";

                // Check the cache
                var attackableUnits = Get<List<WoWUnit>>(CACHEKEY);

                if (attackableUnits == null)
                {
                    // Go and retrieve the data from the objectManager
                    attackableUnits = Unit.AttackableUnits.ToList();
                    //attackableUnits = AttackableUnits.ToList();

                    // Then add it to the cache so we
                    // can retrieve it from there next time
                    // set the object to expire at 1000ms
                    Add(attackableUnits, CACHEKEY, ATTACKABLEUNITS_EXPIRY);
                }
                return attackableUnits;
            }
        }

        private static WoWPlayer CachedTank
        {
            get
            {
                const string CACHEKEY = "Tank";

                // Check the cache
                var Tank = Get<WoWPlayer>(CACHEKEY);

                if (Tank == null)
                {
                    // Go and retrieve the data from the objectManager
                    Tank = HealerManager.GetTank;

                    // Then add it to the cache so we
                    // can retrieve it from there next time
                    // set the object to expire
                    Add(Tank, CACHEKEY, TANK_EXPIRY);
                }
                return Tank;
            }
        }

        #endregion

        #region Combat Log Events

        // this is here to update only at specific times.

        private static bool _eventsAttached;

        private static void AttachCombatLogEvent()
        {
            if (_eventsAttached)
                return;
            Styx.WoWInternals.Lua.Events.AttachEvent("PARTY_MEMBERS_CHANGED", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("UNIT_NAME_UPDATE", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("ZONE_CHANGED_NEW_AREA", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("ZONE_CHANGED", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("ROLE_CHANGED_INFORM", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("UNIT_CONNECTION", HandleEvents);
            Styx.WoWInternals.Lua.Events.AttachEvent("GROUP_ROSTER_UPDATE", HandleEvents);
            Logging.Write("CachedUnits eventsAttached!");
            _eventsAttached = true;
        }

        private static void HandleEvents(object sender, LuaEventArgs args)
        {
            var e = new CombatLogEventArgs(args.EventName, args.FireTimeStamp, args.Args);

            switch (args.EventName)
            {
                case "PARTY_MEMBERS_CHANGED":
                case "UNIT_NAME_UPDATE":
                case "ZONE_CHANGED_NEW_AREA":
                case "ZONE_CHANGED":
                case "ROLE_CHANGED_INFORM":
                case "UNIT_CONNECTION":
                case "GROUP_ROSTER_UPDATE":
                    UpdateCache();
                    break;
            }
        }

        private static void HandleContextChanged(object sender, WoWContextEventArg e)
        {
            UpdateCache();
            Logging.WriteDiagnostic("HandleContextChanged: Update CachedUnits Fired.");
        }

        #endregion Combat Log Events
    }
}
