#region Revision info

/*
 * $Author: tumatauenga1980@gmail.com $
 * $Date: 2012-11-01 08:21:46 -0700 (Thu, 01 Nov 2012) $
 * $ID$
 * $Revision: 764 $
 * $URL: http://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/CLU/Helpers/HealableUnit.cs $
 * $LastChangedBy: tumatauenga1980@gmail.com $
 * $ChangesMade$
 */

#endregion Revision info

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using CLU.Base;
//using CLU.Settings;
using AdvancedAI.Helpers;
using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Managers
{
    using Styx.CommonBot;

    public class HealableUnit
    {
        private bool beaconUnit;

        private bool lifeBloomUnit;
        private bool earthShieldUnit;
        private bool mainTank;
        private bool offTank;

        public static LocalPlayer Me { get { return StyxWoW.Me; } }

        internal static bool IsInDungeonParty { get { return Me.GroupInfo.IsInParty && !Me.GroupInfo.IsInRaid; } }

        internal static bool IsInGroup { get { return Me.GroupInfo.IsInRaid || Me.GroupInfo.IsInParty; } }

        internal static readonly IEnumerable<WoWPartyMember> Groupofplayers = (Me.GroupInfo.IsInRaid ? Me.GroupInfo.RaidMembers : Me.GroupInfo.PartyMembers);

        internal static IEnumerable<WoWPartyMember> GroupMembers { get { return !Me.GroupInfo.IsInRaid ? Me.GroupInfo.PartyMembers : Me.GroupInfo.RaidMembers; } }

        // This will be the primary heal target set from TargetBase.
        private static HealableUnit healtarget;

        public static HealableUnit HealTarget
        {
            get
            {
                if (healtarget == null)
                {
                    return new HealableUnit(Me);
                }
                return healtarget;
            }

            set
            {
                healtarget = value;
            }
        }

        // Empty Constructor
        private HealableUnit()
        {
        }

        /// <summary>
        /// CLU's Unit to heal
        /// </summary>
        /// <param name="unit">The WoWUnit</param>
        public HealableUnit(WoWUnit unit)
        {
            UnitObject = unit;
        }

        private static HealableUnit instance;

        public static HealableUnit Instance
        {
            get
            {
                return instance ?? (instance = new HealableUnit());
            }
        }

        //a crude attempt at handling adding and removing from listofHealableUnits.
        private static bool Adding
        {
            get;
            set;
        }

        private static bool Removing
        {
            get;
            set;
        }

        // Declare a new List with the HealableUnit object.
        private static List<HealableUnit> listofHealableUnits;

        public static List<HealableUnit> ListofHealableUnits
        {
            get
            {
                return listofHealableUnits ?? (listofHealableUnits = new List<HealableUnit>());
            }
            set
            {
                listofHealableUnits = value;
            }
        }

        /// <summary>
        /// Pulse
        /// </summary>
        public static void Pulse()
        {
            try
            {
                if (StyxWoW.IsInGame)
                {
                    Add();
                    Remove();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("HealableUnit Pulse : {0}", ex);
            }
        }

        /// <summary>
        /// The Object containing the healableunit (WoWUnit)
        /// </summary>
        [Browsable(false)]
        private WoWUnit UnitObject
        {
            get;
            set;
        }

        public int TimeToLive
        {
            get
            {
                return timeToLive(1);
            }
        }

        private int timeToLive(double deltaSeconds)
        {
            var oracle = UnitOracle.FindUnit(this.UnitObject);
            if (oracle == null)
            {
                UnitOracle.WatchUnit(this.UnitObject, UnitOracle.Watch.HealthVariance);
                return 999999;
            }

            return oracle.TimeToDie(deltaSeconds);
        }

        /// <summary>
        /// Unit to be Blacklisted from Healing
        /// </summary>
        public bool Blacklisted
        {
            get;
            set;
        }

        /// <summary>
        /// Unit requiring Urgent Dispels.
        /// </summary>
        public bool Urgentdispel
        {
            get;
            set;
        }

        /// <summary>
        /// Unit to place Holy Paladins Beacon of Light
        /// </summary>
        public bool Beacon
        {
            get
            {
                if (true)
                {
                    return beaconUnit = MainTank;
                }
                return this.beaconUnit;
            }

            set
            {
                this.beaconUnit = value;
            }
        }

        /// <summary>
        /// Unit to place Restoration Druids Lifebloom
        /// </summary>
        public bool LifeBloom
        {
            get
            {
                if (true)
                {
                    return lifeBloomUnit = MainTank;
                }
                return this.lifeBloomUnit;
            }

            set
            {
                this.lifeBloomUnit = value;
            }
        }

        /// <summary>
        /// Unit to place Restoration Shamans Earth Shield
        /// </summary>
        public bool EarthShield
        {
            get
            {
                if (true)
                {
                    return earthShieldUnit = MainTank;
                }
                return this.earthShieldUnit;
            }

            set
            {
                this.earthShieldUnit = value;
            }
        }

        /// <summary>
        /// Unit to be mainTank
        /// </summary>
        public bool MainTank
        {
            get
            {
                //// Retrieve our main tank.
                //var result = GetMaintank;
                //if (result != null && (result.ToUnit().Guid == this.UnitObject.Guid))
                //{
                //    return mainTank = true;
                //}
                return this.mainTank;
            }

            set
            {
                this.mainTank = value;
            }
        }

        /// <summary>
        /// Unit to be offTank
        /// </summary>
        public bool OffTank
        {
            get
            {
                // Retrieve our off tank.
                var result = listofHealableUnits.Where(x => x != null && x.Tank);
                if (result.Any(x => x != null && (x.ToUnit().Guid == this.UnitObject.Guid && !this.MainTank)))
                {
                    return offTank = true;
                }
                return this.offTank;
            }

            set
            {
                this.offTank = value;
            }
        }

        /// <summary>
        /// The WoWUnits name
        /// </summary>
        public string Name
        {
            get
            {
                return this.UnitObject.Name;
            }
        }

        /// <summary>
        /// The Units CurrentHealth
        /// </summary>
        public int CurrentHealth
        {
            get
            {
                return this.UnitObject.IsDead ? 0 : (int)this.UnitObject.CurrentHealth;
            }
        }

        /// <summary>
        /// The Units Maximum Health
        /// </summary>
        public int MaxHealth
        {
            get
            {
                return (int)this.UnitObject.MaxHealth;
            }
        }

        /// <summary>
        /// The Units HealthPercent
        /// </summary>
        public int HealthPercent
        {
            get
            {
                return this.UnitObject.IsDead ? 0 : (int)this.UnitObject.HealthPercent;
            }
        }

        /// <summary>
        /// The Units Health Deficet (MaxHealth - CurrentHealth)
        /// </summary>
        public int MissingHealth
        {
            get
            {
                return MaxHealth - CurrentHealth;
            }
        }

        /// <summary>
        /// The current Distance from the player to the unit.
        /// </summary>
        public double Distance
        {
            get
            {
                return Math.Round(this.UnitObject.Distance, 3);
            }
        }

        /// <summary>
        /// The Units Location
        /// </summary>
        public WoWPoint Location
        {
            get
            {
                try
                {
                    return this.UnitObject.Location;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("Error with Healable Unit Location : {0}", ex);
                }
                return new WoWPoint();
            }
        }

        /// <summary>
        /// Return the group number of the player...why is this from 0 - 4 and not 1 - 5 ??
        /// </summary>
        public uint GroupNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// True if the unit is a Tank.
        /// </summary>
        public bool Tank
        {
            get;
            set;
        }

        /// <summary>
        /// True if the unit is a Healer
        /// </summary>
        public bool Healer
        {
            get;
            set;
        }

        /// <summary>
        /// True if the unit is a Damage Dealer.
        /// </summary>
        public bool Damage
        {
            get;
            set;
        }

        /// <summary>
        /// True if the unit is me :)
        /// </summary>
        public bool IsMe
        {
            get
            {
                return this.UnitObject.IsMe;
            }
        }

        /// <summary>
        /// Used to convert a HealableUnit to a WoWUnit
        /// </summary>
        /// <returns></returns>
        public WoWUnit ToUnit()
        {
            return this.UnitObject;
        }

        /// <summary>
        ///  List of healable units by Raid or Party Information
        /// </summary>
        private static IEnumerable<HealableUnit> HealableUnits
        {
            get
            {
                var result = new List<HealableUnit>();
                try
                {
                    var grps = GroupMembers.Where(p => !HealableUnit.Contains(p.ToPlayer()) && HealableUnit.Filter(p.ToPlayer())).Select(g => g);

                    var list = new List<HealableUnit>();

                    if (!HealableUnit.Contains(Me))
                    {
                        Logging.WriteDiagnostic("Adding: {0} to list of Healable Units", Me.ToPlayer().SafeName());
                        list.Add(new HealableUnit(Me));
                    }

                    // SysLog.TroubleshootLog("Units Count = {0}", grps.Count());

                    foreach (WoWPartyMember p in grps)
                    {
                        Logging.WriteDiagnostic("Adding Group Member: {0} to list of Healable Units", p.ToPlayer().SafeName());
                        var role = p.Role;
                        bool tank = false;
                        bool healer = ((role & WoWPartyMember.GroupRole.Healer) != 0);
                        bool damage = ((role & WoWPartyMember.GroupRole.Damage) != 0);
                        uint groupnumber = Me.GroupInfo.IsInRaid ? p.GroupNumber : 0;

                        // Set tank by role
                        if ((role & WoWPartyMember.GroupRole.Tank) != 0)
                        {
                            Logging.WriteDiagnostic(" Setting {0} as tank based on Raid Role.", p.ToPlayer().SafeName() + " " + p.ToPlayer().Level);
                            tank = true;
                        }

                        // If the user has a focus target set, use it instead.
                        if (StyxWoW.Me.FocusedUnitGuid != 0 && StyxWoW.Me.FocusedUnit.Guid == p.ToPlayer().Guid)
                        {
                            Logging.WriteDiagnostic("Setting {0} as tank based on FocusedUnit.", p.ToPlayer().SafeName() + " " + p.ToPlayer().Level);
                            tank = true;
                        }

                        // RaF
                        if (RaFHelper.Leader != null && RaFHelper.Leader.CurrentHealth > 1 && RaFHelper.Leader != Me && RaFHelper.Leader.Guid == p.ToPlayer().Guid)
                        {
                            Logging.WriteDiagnostic("Setting {0} as tank based on RaFHelper.Leader.", Me.SafeName() + " " + p.ToPlayer().Level);
                            tank = true;
                        }

                        bool maintank = IsInDungeonParty ? tank : p.IsMainTank;
                        bool offtank = p.IsMainAssist;

                        list.Add(new HealableUnit { UnitObject = p.ToPlayer(), Tank = tank, Healer = healer, Damage = damage, GroupNumber = groupnumber, MainTank = maintank, OffTank = offtank, });
                    }

                    result = list;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("HealableUnits : {0}", ex);
                }

                return result;
            }
        }

        /// <summary>
        /// list of healable units by proximity
        /// </summary>
        private static IEnumerable<HealableUnit> HealableUnitsProx
        {
            get
            {
                var result = new List<HealableUnit>();
                try
                {
                    var list = new List<HealableUnit>();
                    var proximitylist = Unit.HealList.Where(p => !HealableUnit.Contains(p.ToPlayer()) && HealableUnit.Filter(p.ToPlayer()));

                    if (!HealableUnit.Contains(Me))
                    {
                        Logging.WriteDiagnostic("Adding: {0} to list of Healable Units", Me.ToPlayer().SafeName());
                        list.Add(new HealableUnit(Me));
                    }

                    foreach (WoWUnit p in proximitylist)
                    {
                        Logging.WriteDiagnostic("Adding Unit by Proximity: {0} to list of Healable Units", p.ToPlayer().SafeName());
                        bool tank = false;

                        // If the user has a focus target set, use it instead.
                        if (StyxWoW.Me.FocusedUnitGuid != 0 && StyxWoW.Me.FocusedUnit.Guid == p.ToPlayer().Guid)
                        {
                            Logging.WriteDiagnostic("Setting {0} as tank based on FocusedUnit.", p.ToPlayer().SafeName() + " " + p.ToPlayer().Level);
                            tank = true;
                        }

                        // RaF
                        if (RaFHelper.Leader != null && RaFHelper.Leader.CurrentHealth > 1 && RaFHelper.Leader != Me && RaFHelper.Leader.Guid == p.ToPlayer().Guid)
                        {
                            Logging.WriteDiagnostic("Setting {0} as tank based on RaFHelper.Leader.", Me.SafeName() + " " + p.ToPlayer().Level);
                            tank = true;
                        }

                        list.Add(new HealableUnit { UnitObject = p.ToPlayer(), Tank = tank, });
                    }
                    result = list;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("HealableUnitsByProximity : {0}", ex);
                }
                return result;
            }
        }

        /// <summary>
        /// Add HealableUnits that meet a criteria
        /// </summary>
        private static void Add()
        {
            try
            {
                if (Removing) return;

                Adding = true;

                switch (HealingAquisitionMethod.RaidParty)
                {
                    case HealingAquisitionMethod.Proximity:
                        HealableUnitsByProximity();
                        break;

                    case HealingAquisitionMethod.RaidParty:
                        HealableUnitsByPartyorRaid();
                        break;
                }

                Adding = false;
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("AddHealableUnits : {0}", ex);
            }
        }

        /// <summary>
        /// Remove invalid healableunits.
        /// </summary>
        public static void Remove()
        {
            try
            {
                if (Adding) return;

                Removing = true;

                var grp = ListofHealableUnits.Where(n => !HealableUnit.Filter(n.ToUnit())).ToList();
                foreach (var unit in grp.Where(unit => ListofHealableUnits.IndexOf(unit) != -1))
                {
                    ListofHealableUnits.RemoveAt(ListofHealableUnits.IndexOf(unit)); // remove
                    Logging.WriteDiagnostic(" Success Removed: {0} no longer a valid healableunit.", unit.ToUnit().SafeName());
                }

                Removing = false;
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("Remove : {0}", ex);
            }
        }

        /// <summary>
        /// Add HealableUnits to our List of HealableUnits given their current location
        /// </summary>
        public static void HealableUnitsByProximity()
        {
            try
            {
                var result = HealableUnitsProx;
                listofHealableUnits.AddRange(result.Where(t => t != null).Select(t => t));
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("Initialize HealableUnitsByProximity : {0}", ex);
            }
        }

        /// <summary>
        /// Add HealableUnits to our List of HealableUnits for Party and Raid members
        /// </summary>
        public static void HealableUnitsByPartyorRaid()
        {
            try
            {
                var result = HealableUnits;
                listofHealableUnits.AddRange(result.Where(t => t != null).Select(t => t));
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("InitializeHealing HealableUnitsByPartyorRaid : {0}", ex);
            }
        }

        /// <summary>
        /// Checks to see the specified unit is not present in our current List of HealableUnits
        /// </summary>
        /// <param name="unit">the unit to check for</param>
        public static bool Contains(WoWUnit unit)
        {
            try
            {
                var grp = ListofHealableUnits.ToList();
                return grp.Any(n => n != null && (unit != null && n.ToUnit().Guid == unit.Guid));
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("Contains : {0}", ex);
            }
            return false;
        }

        /// <summary>
        /// Filter for Healable Unit
        /// </summary>
        /// <param name="unit">the unit to check</param>
        /// <returns>returns true if the unit meets the requirements to be a HealableUnit</returns>
        public static bool Filter(WoWUnit unit)
        {
            try
            {
                if (unit != null && unit.IsMe) return true; // im always valid..bazinga :P
                return unit != null && (unit.IsValid &&
                                        unit.ToPlayer().IsAlive &&
                                        !unit.ToPlayer().IsGhost &&
                                        unit.ToPlayer().IsPlayer &&
                                        unit.ToPlayer() != null &&
                                        !unit.ToPlayer().IsFlying &&
                                        !unit.ToPlayer().OnTaxi &&
                                        unit.Distance2DSqr < 40 * 40);
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("Filter : {0}", ex);
            }
            return false;
        }

        /// <summary>
        /// Prints the contents of teh List of HealableUnits
        /// </summary>
        private static void Print()
        {
            Logging.WriteDiagnostic("HealableUnit List has {0} elements.", ListofHealableUnits.Count);
            for (int i = 0; i < ListofHealableUnits.Count; i++)
            {
                Logging.WriteDiagnostic(" {0} : {1} Tank: {2}", ListofHealableUnits[i].Name, i, ListofHealableUnits[i].Tank);
            }
        }

        /// <summary>
        /// Selects the most appropriate tank based on blizzards method (MainTank/MainAssist) or Max Health
        /// </summary>
        [Browsable(false)]
        public static HealableUnit GetMaintank
        {
            get
            {
                try
                {
                    if (!StyxWoW.Me.GroupInfo.IsInParty)
                        return null;

                    if (Tanks.Count() != 0 && Tanks.All(t => !t.MainTank))
                    {
                        var result = Tanks.Aggregate((t1, t2) => t1.MaxHealth > t2.MaxHealth ? t1 : t2);
                        if (result != null)
                        {
                            Logging.WriteDiagnostic("Selecting {0} as Main Tank based on max health...dont like it? Change it yourself via the UI settings!",result.ToUnit().SafeName());
                            return result;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("GetMaintank : {0}", ex);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns a list of tanks
        /// </summary>
        public static IEnumerable<HealableUnit> Tanks
        {
            get
            {
                var result = new List<HealableUnit>();
                try
                {
                    if (!StyxWoW.Me.GroupInfo.IsInParty)
                        return result;

                    var members = listofHealableUnits.Where(p => p.Tank);

                    result.AddRange(members.Where(t => t != null).Select(t => t));

                    return result;
                }
                catch (Exception ex)
                {
                    Logging.WriteDiagnostic("Tanks : {0}", ex);
                }

                return result;
            }
        }
    }
}