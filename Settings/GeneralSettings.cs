
using System.ComponentModel;
using System.IO;
using System.Linq;
using Styx;
using Styx.Common;
using Styx.Helpers;

using DefaultValue = Styx.Helpers.DefaultValueAttribute;
using AdvancedAI.Managers;
using System.Reflection;
using System;
using System.Collections.Generic;
using AdvancedAI.Helpers;
using Styx.CommonBot;
using Styx.WoWInternals;

namespace AdvancedAI.Settings
{
    enum AllowMovementType
    {
        None,
        ClassSpecificOnly,
        All,
        Auto       
    }

    enum CheckTargets
    {
        None = 0,
        Current,
        All
    }

    enum PurgeAuraFilter
    {
        None = 0,
        Whitelist,
        All
    }

    enum RelativePriority
    {
        None = 0,
        LowPriority,
        HighPriority
    }

    enum TargetingStyle
    {
        None = 0,
        Enable,
        Auto
    }

    enum SelfRessurectStyle
    {
        None = 0,
        Enable,
        Auto
    }

    enum CombatRezTarget
    {
        None = 0,
        All = Tank | Healer | DPS,
        Tank = 1,
        Healer = 2,
        TankOrHealer = Tank | Healer,
        DPS = 4
    }

    internal class GeneralSettings : Styx.Helpers.Settings
    {
        private static GeneralSettings _instance;

        public GeneralSettings()
            : base(Path.Combine(CharacterSettingsPath, "GeneralSettings.xml"))
        {
        }

        public static string GlobalSettingsPath
        {
            get
            {
                return Path.Combine(Styx.Common.Utilities.AssemblyDirectory, "Settings");
            }
        }


        public static string CharacterSettingsPath
        {
            get
            {
                string settingsDirectory = Path.Combine(Styx.Common.Utilities.AssemblyDirectory, "Settings");
                return Path.Combine(Path.Combine(settingsDirectory, StyxWoW.Me.RealmName), StyxWoW.Me.Name);
            }
        }


        public static string AdvancedAISettingsPath
        {
            get
            {
                string settingsDirectory = Path.Combine(Styx.Common.Utilities.AssemblyDirectory, "Settings");
                return Path.Combine(Path.Combine(Path.Combine(settingsDirectory, StyxWoW.Me.RealmName), StyxWoW.Me.Name), "AdvancedAI");
            }
        }

        public static GeneralSettings Instance 
        { 
            get { return _instance ?? (_instance = new GeneralSettings()); }
            set { _instance = value; }
        }
        

        /// <summary>
        /// Write all Singular Settings in effect to the Log file
        /// </summary>
        public void LogSettings()
        {
            Logging.Write("");

            // reference the internal references so we can display only for our class
            LogSettings("AdvancedAI", GeneralSettings.Instance);
            //if (StyxWoW.Me.Class == WoWClass.DeathKnight )  LogSettings("DeathKnight", DeathKnight());
            //if (StyxWoW.Me.Class == WoWClass.Druid )        LogSettings("Druid", Druid());
            //if (StyxWoW.Me.Class == WoWClass.Hunter )       LogSettings("Hunter", Hunter());
            //if (StyxWoW.Me.Class == WoWClass.Mage )         LogSettings("Mage", Mage());
            //if (StyxWoW.Me.Class == WoWClass.Monk )         LogSettings("Monk", Monk());
            //if (StyxWoW.Me.Class == WoWClass.Paladin )      LogSettings("Paladin", Paladin());
            //if (StyxWoW.Me.Class == WoWClass.Priest )       LogSettings("Priest", Priest());
            //if (StyxWoW.Me.Class == WoWClass.Rogue )        LogSettings("Rogue", Rogue());
            //if (StyxWoW.Me.Class == WoWClass.Shaman)        LogSettings("Shaman", Shaman());
            //if (StyxWoW.Me.Class == WoWClass.Warlock)       LogSettings("Warlock", Warlock());
            //if (StyxWoW.Me.Class == WoWClass.Warrior)       LogSettings("Warrior", Warrior());
            


            Logging.Write("====== Evaluated/Dynamic Settings ======");
            Logging.Write("  {0}: {1}", "TrivialHealth", Unit.TrivialHealth());
            Logging.Write("");
        }

        public void LogSettings(string desc, Styx.Helpers.Settings set)
        {
            if (set == null)
                return;

            Logging.Write("====== {0} Settings ======", desc);
            foreach (var kvp in set.GetSettings())
            {
                Logging.Write("  {0}: {1}", kvp.Key, kvp.Value.ToString());
            }

            Logging.Write("");
        }

        /// <summary>
        /// Obsolete:  Almost all code should reference MovementManager.IsMovementDisabled
        /// .. which will handle Hotkey processing and any context sensitive bot behavior.
        /// .. This setting only retrieves the user setting which typically is insufficient.
        /// </summary>
        
        



        #region Category: Movement
        

        //[Setting]
        //[DefaultValue(12)]
        //[Category("Movement")]
        //[DisplayName("Melee Dismount Range")]
        //[Description("Distance from target that melee should dismount")]
        //public int MeleeDismountRange { get; set; }
        

        #endregion 

        #region Category: Consumables

        [Setting]
        [DefaultValue(30)]
        [Category("Consumables")]
        [DisplayName("Potion at Health %")]
        [Description("Health % to use a health pot/trinket/stone at.")]
        public int PotionHealth { get; set; }

        [Setting]
        [DefaultValue(30)]
        [Category("Consumables")]
        [DisplayName("Potion at Mana %")]
        [Description("Mana % to use a mana pot/trinket at. (used for all energy forms)")]
        public int PotionMana { get; set; }


        #endregion

        #region Category: Avoidance

        [Setting]
        [DefaultValue(true)]
        [Category("Avoidance")]
        [DisplayName("Disengage Allowed")]
        [Description("Allow use of Disengage, Blink, Rocket Jump, Balance-Wild Charge, or equivalent spell to quickly jump away")]
        public bool DisengageAllowed { get; set; }


        [Setting]
        [DefaultValue(false)]
        [Category("Avoidance")]
        [DisplayName("Kiting Allowed")]
        [Description("Allow kiting of mobs.")]
        public bool KiteAllow { get; set; }

        [Setting]
        [DefaultValue(50)]
        [Category("Avoidance")]
        [DisplayName("Kite below Health %")]
        [Description("Kite if health below this % and mob in melee range")]
        public int KiteHealth { get; set; }

        [Setting]
        [DefaultValue(2)]
        [Category("Avoidance")]
        [DisplayName("Kite at mob count")]
        [Description("Kite if this many mobs in melee range")]
        public int KiteMobCount { get; set; }

        [Setting]
        [DefaultValue(8)]
        [Category("Avoidance")]
        [DisplayName("Avoid Distance")]
        [Description("Only mobs within this distance that are attacking you count towards Disengage/Kite mob counts")]
        public int AvoidDistance { get; set; }

        [Browsable(false)]
        [Setting]
        [DefaultValue(false)]
        [Category("Avoidance")]
        [DisplayName("Jump Turn while Kiting")]
        [Description("Perform jump turn attack while kiting (only supported by Hunter presently)")]
        public bool JumpTurnAllow { get; set; }

        #endregion

        #region Category: General

        //[Setting]
        //[DefaultValue(true)]
        //[Category("General")]
        //[DisplayName("Wait For Res Sickness")]
        //[Description("Wait for resurrection sickness to wear off.")]
        //public bool WaitForResSickness { get; set; }

        [Browsable(false)]
        [Setting]
        [DefaultValue(0)]
        public int FormTabIndex { get; set; }

        [Browsable(false)]
        [Setting]
        [DefaultValue(337)]
        public int FormHeight { get; set; }

        [Browsable(false)]
        [Setting]
        [DefaultValue(378)]
        public int FormWidth { get; set; }

        #endregion


        #region Category: Group Healing / Support
        
        //[Setting]
        //[DefaultValue(RelativePriority.LowPriority)]
        //[Category("Group Healing/Support")]
        //[DisplayName("Dispel Debufs")]
        //[Description("Dispel harmful debuffs")]
        //public RelativePriority DispelDebuffs { get; set; }
        
        #endregion

        #region Category: Healing

        #endregion

        #region Category: Items



        #endregion

        #region Category: Racials


        
        #endregion

        #region Category: Tanking



        #endregion

        #region Category: Enemy Control


        //[Setting]
        //[DefaultValue(true)]
        //[Category("Enemy Control")]
        //[DisplayName("Use AOE Attacks")]
        //[Description("True: use multi-target damage spells when necessary; False: single target spells on current target only")]
        //public bool AllowAOE { get; set; }

        #endregion

        #region Class Late-Loading Wrappers

        // Do not change anything within this region.
        // It's written so we ONLY load the settings we're going to use.
        // There's no reason to load the settings for every class, if we're only executing code for a Druid.

        private DeathKnightSettings _dkSettings;

        private DruidSettings _druidSettings;

        private HunterSettings _hunterSettings;

        private MageSettings _mageSettings;
		
		private MonkSettings _monkSettings;
		
        private PaladinSettings _pallySettings;

        private PriestSettings _priestSettings;

        private RogueSettings _rogueSettings;

        private ShamanSettings _shamanSettings;

        private WarlockSettings _warlockSettings;

        private WarriorSettings _warriorSettings;

        private HotkeySettings _hotkeySettings;

        // late-binding interfaces 
        // -- changed from readonly properties to methods as GetProperties() in SaveToXML() was causing all classes configs to load
        // -- this was causing Save to write a DeathKnight.xml file for all non-DKs for example
        internal DeathKnightSettings DeathKnight() { return _dkSettings ?? (_dkSettings = new DeathKnightSettings()); } 
        internal DruidSettings Druid() { return _druidSettings ?? (_druidSettings = new DruidSettings()); }
        internal HunterSettings Hunter() { return _hunterSettings ?? (_hunterSettings = new HunterSettings()); }
        internal MageSettings Mage() { return _mageSettings ?? (_mageSettings = new MageSettings()); }
        internal MonkSettings Monk() { return _monkSettings ?? (_monkSettings = new MonkSettings()); }
        internal PaladinSettings Paladin() { return _pallySettings ?? (_pallySettings = new PaladinSettings()); }
        internal PriestSettings Priest() { return _priestSettings ?? (_priestSettings = new PriestSettings()); }
        internal RogueSettings Rogue() { return _rogueSettings ?? (_rogueSettings = new RogueSettings()); }
        internal ShamanSettings Shaman() { return _shamanSettings ?? (_shamanSettings = new ShamanSettings()); }
        internal WarlockSettings Warlock() { return _warlockSettings ?? (_warlockSettings = new WarlockSettings()); }
        internal WarriorSettings Warrior() { return _warriorSettings ?? (_warriorSettings = new WarriorSettings()); }
        internal HotkeySettings Hotkeys() { return _hotkeySettings ?? (_hotkeySettings = new HotkeySettings()); }

        #endregion
    }

}