using System;
using System.Linq;
using AdvancedAI.Helpers;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Plugins;
using Styx.WoWInternals;
using Styx.WoWInternals.DBC;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI
{
    #region Nested type: LocationContextEventArg
    public class WoWContextEventArg : EventArgs
    {
        public readonly WoWContext CurrentWoWContext;
        public readonly WoWContext PreviousWoWContext;

        public WoWContextEventArg(WoWContext currentWoWContext, WoWContext prevWoWContext)
        {
            CurrentWoWContext = currentWoWContext;
            PreviousWoWContext = prevWoWContext;
        }
    }
    #endregion Nested type: LocationContextEventArg

    partial class AdvancedAI
    {
        public static event EventHandler<WoWContextEventArg> OnWoWContextChanged;
        private static WoWContext _lastContext = WoWContext.None;

        internal static bool IsQuestBotActive { get; set; }
        internal static bool IsBgBotActive { get; set; }
        internal static bool IsDungeonBuddyActive { get; set; }
        internal static bool IsPokeBuddyActive { get; set; }
        internal static bool IsManualMovementBotActive { get; set; }

        internal static WoWContext CurrentWoWContext
        {
            get
            {
                return DetermineCurrentWoWContext;
            }
        }

        internal static HealingContext CurrentHealContext
        {
            get
            {
                WoWContext ctx = CurrentWoWContext;
                if (ctx == WoWContext.Instances && Me.GroupInfo.IsInRaid)
                    return HealingContext.Raids;

                return (HealingContext)ctx;
            }
        }

        private static WoWContext DetermineCurrentWoWContext
        {
            get
            {
                if (!StyxWoW.IsInGame)
                    return WoWContext.None;

                if (PvPRot)
                    return WoWContext.Battlegrounds;

                if (PvERot)
                    return WoWContext.Instances;

                Map map = StyxWoW.Me.CurrentMap;

                if (map.IsBattleground || IsArena())
                {
                    return WoWContext.Battlegrounds;
                }

                if (Me.IsInGroup())
                {
                    if (Me.IsInInstance)
                    {
                        return WoWContext.Instances;
                    }

                    if (Group.Tanks.Any())
                    {
                        return WoWContext.Instances;
                    }
                }

                //if (SingularRoutine.ForceInstanceBehaviors)
                //    return WoWContext.Instances;

                return WoWContext.Normal;
            }
        }

        private bool _contextEventSubscribed;

        private void UpdateContext()
        {
            // Subscribe to the map change event, so we can automatically update the context.
            if (!_contextEventSubscribed)
            {
                // Subscribe to OnBattlegroundEntered. Just 'cause.
                BotEvents.Battleground.OnBattlegroundEntered += e => UpdateContext();
                _contextEventSubscribed = true;
            }

            var current = DetermineCurrentWoWContext;

            // Can't update the context when it doesn't exist.
            if (current == WoWContext.None)
                return;

            if (current != _lastContext && OnWoWContextChanged != null)
            {
                // store values that require scanning lists
                UpdateContextStateValues();
                DescribeContext();
                try
                {
                    OnWoWContextChanged(this, new WoWContextEventArg(current, _lastContext));
                }
                catch
                {
                    // Eat any exceptions thrown.
                }

                _lastContext = current;
            }
        }

        private static bool UpdateContextStateValues()
        {
            bool questBot = IsBotInUse("Quest");
            bool bgBot = IsBotInUse("BGBuddy") || IsBotInUse("BG Bot");
            bool dungeonBot = IsBotInUse("DungeonBuddy");
            bool petHack = IsPluginActive("Pokébuddy") || IsPluginActive("Pokehbuddy");
            bool manualBot = IsBotInUse("LazyRaider", "Raid Bot", "Tyrael");

            bool changed = false;

            if (questBot != IsQuestBotActive)
            {
                changed = true;
                IsQuestBotActive = questBot;
            }

            if (bgBot != IsBgBotActive)
            {
                changed = true;
                IsBgBotActive = bgBot;
            }

            if (dungeonBot != IsDungeonBuddyActive)
            {
                changed = true;
                IsDungeonBuddyActive = dungeonBot;
            }

            if (petHack != IsPokeBuddyActive)
            {
                changed = true;
                IsPokeBuddyActive = petHack;
            }

            if (manualBot != IsManualMovementBotActive)
            {
                changed = true;
                IsManualMovementBotActive = manualBot;
            }

            return changed;
        } 




        private static WoWContext LastWoWContext { get; set; }

        private static GroupType GroupType
        {
            get
            {
                if (Me.GroupInfo.IsInRaid)
                {
                    return GroupType.Raid;
                }

                return Me.GroupInfo.IsInParty ? GroupType.Party : GroupType.Solo;
            }
        }

        

        private static bool IsArena()
        {
            return Me.MapId == 3698 || Me.MapId == 3702 || Me.MapId == 3968 || Me.MapId == 4378 || Me.MapId == 4406 || Me.MapId == 6296 || Me.MapId == 6732;
        }

        public static bool IsBotInUse(params string[] nameSubstrings)
        {
            string botName = GetBotName().ToLowerInvariant();
            return nameSubstrings.Any(s => botName.Contains(s.ToLowerInvariant()));
        }

        public static bool IsPluginActive(params string[] nameSubstrings)
        {
            var lowerNames = nameSubstrings.Select(s => s.ToLowerInvariant()).ToList();
            return PluginManager.Plugins.Any(p => p.Enabled && lowerNames.Contains(p.Name.ToLowerInvariant()));
        }

        private static int GetInstanceDifficulty()
        {
            int diffidx = Lua.GetReturnVal<int>("return GetInstanceDifficulty()", 0);
            return diffidx;
        }

        private static string[] _InstDiff = new string[] 
        {
            /* 0*/  "Unknown Difficulty",
            /* 1*/  "None; not in an Instance",
            /* 2*/  "5-player Normal",
            /* 3*/  "5-player Heroic",
            /* 4*/  "10-player Raid",
            /* 5*/  "25-player Raid",
            /* 6*/  "10-player Heroic Raid",
            /* 7*/  "25-player Heroic Raid",
            /* 8*/  "LFR Raid Instance",
            /* 9*/  "Challenge Mode Raid",
            /* 10*/  "40-player Raid"
        };

        private static string GetInstanceDifficultyName()
        {
            int diff = GetInstanceDifficulty();
            if (diff < _InstDiff.GetLowerBound(0) || diff > _InstDiff.GetUpperBound(0))
                return string.Format("Difficulty {0} Undefined", diff);

            return _InstDiff[diff];
        }

        public static string GetBotName()
        {
            BotBase bot = null;

            if (TreeRoot.Current != null)
            {
                if (!(TreeRoot.Current is NewMixedMode.MixedModeEx))
                    bot = TreeRoot.Current;
                else
                {
                    NewMixedMode.MixedModeEx mmb = (NewMixedMode.MixedModeEx)TreeRoot.Current;
                    if (mmb != null)
                    {
                        if (mmb.SecondaryBot != null && mmb.SecondaryBot.RequirementsMet)
                            return "Mixed:" + mmb.SecondaryBot.Name;
                        return mmb.PrimaryBot != null ? "Mixed:" + mmb.PrimaryBot.Name : "Mixed:[primary null]";
                    }
                }
            }

            return bot.Name;
        }

        static void DescribeContext()
        {
            string sRace = Me.Race.ToString().CamelToSpaced();
            if (Me.Race == WoWRace.Pandaren)
                sRace = " " + Me.FactionGroup.ToString() + sRace;

            Logging.Write(" "); // spacer before prior log text

            Logging.Write("Your Level {0}{1} {2} {3} Build is", Me.Level, sRace, SpecializationName(), Me.Class.ToString());

            Logging.Write("... running the {0} bot in {1} {2}",
                 GetBotName(),
                 Me.RealZoneText,
                 !Me.IsInInstance || Battlegrounds.IsInsideBattleground ? "" : "[" + GetInstanceDifficultyName() + "]"
                );

            Logging.Write("   MapId            = {0}", Me.MapId);
            Logging.Write("   ZoneId           = {0}", Me.ZoneId);
            /*
                        if (Me.CurrentMap != null && Me.CurrentMap.IsValid)
                        {
                            Logger.WriteFile("   AreaTableId      = {0}", Me.CurrentMap.AreaTableId);
                            Logger.WriteFile("   InternalName     = {0}", Me.CurrentMap.InternalName);
                            Logger.WriteFile("   IsArena          = {0}", Me.CurrentMap.IsArena.ToYN());
                            Logger.WriteFile("   IsBattleground   = {0}", Me.CurrentMap.IsBattleground.ToYN());
                            Logger.WriteFile("   IsContinent      = {0}", Me.CurrentMap.IsContinent.ToYN());
                            Logger.WriteFile("   IsDungeon        = {0}", Me.CurrentMap.IsDungeon.ToYN());
                            Logger.WriteFile("   IsInstance       = {0}", Me.CurrentMap.IsInstance.ToYN());
                            Logger.WriteFile("   IsRaid           = {0}", Me.CurrentMap.IsRaid.ToYN());
                            Logger.WriteFile("   IsScenario       = {0}", Me.CurrentMap.IsScenario.ToYN());
                            Logger.WriteFile("   MapDescription   = {0}", Me.CurrentMap.MapDescription);
                            Logger.WriteFile("   MapDescription2  = {0}", Me.CurrentMap.MapDescription2);
                            Logger.WriteFile("   MapType          = {0}", Me.CurrentMap.MapType);
                            Logger.WriteFile("   MaxPlayers       = {0}", Me.CurrentMap.MaxPlayers);
                            Logger.WriteFile("   Name             = {0}", Me.CurrentMap.Name);
                        }
            */
            string sRunningAs = "";

            if (Me.CurrentMap == null)
                sRunningAs = "Unknown";
            else if (Me.CurrentMap.IsArena)
                sRunningAs = "Arena";
            else if (Me.CurrentMap.IsBattleground)
                sRunningAs = "Battleground";
            else if (Me.CurrentMap.IsScenario)
                sRunningAs = "Scenario";
            else if (Me.CurrentMap.IsRaid)
                sRunningAs = "Raid";
            else if (Me.CurrentMap.IsDungeon)
                sRunningAs = "Dungeon";
            else if (Me.CurrentMap.IsInstance)
                sRunningAs = "Instance";
            else
                sRunningAs = "Zone: " + Me.CurrentMap.Name;

            Logging.Write("... {0} using my {1} Behaviors",
                 sRunningAs,
                 CurrentWoWContext == WoWContext.Normal ? "SOLO" : CurrentWoWContext.ToString().ToUpper());

            if (CurrentWoWContext != WoWContext.Battlegrounds && Me.IsInGroup())
            {
                Logging.Write("... in a group as {0} role with {1} of {2} players",
                    (Me.Role & (WoWPartyMember.GroupRole.Tank | WoWPartyMember.GroupRole.Healer | WoWPartyMember.GroupRole.Damage)).ToString().ToUpper(),
                     Me.GroupInfo.NumRaidMembers,
                     (int)Math.Max(Me.CurrentMap.MaxPlayers, Me.GroupInfo.GroupSize)
                    );
            }

            //Item.WriteCharacterGearAndSetupInfo();

#if LOG_GROUP_COMPOSITION
            if (CurrentWoWContext == WoWContext.Instances)
            {
                int idx = 1;
                Logger.WriteFile(" ");
                Logger.WriteFile("Group Comprised of {0} members as follows:", Me.GroupInfo.NumRaidMembers);
                foreach (var pm in Me.GroupInfo.RaidMembers )
                {
                    string role = (pm.Role & ~WoWPartyMember.GroupRole.None).ToString().ToUpper() + "      ";
                    role = role.Substring( 0, 6);                   
                    
                    Logger.WriteFile( "{0} {1} {2} {3} {4} {5}",
                        idx++, 
                        role,
                        pm.IsOnline ? "online " : "offline",
                        pm.Level,
                        pm.HealthMax,
                        pm.Specialization
                        );
                }

                Logger.WriteFile(" ");
            }
#endif

            if (Styx.CommonBot.Targeting.PullDistance < 25)
                Logging.Write("your Pull Distance is {0:F0} yds which is low for any class!!!", Styx.CommonBot.Targeting.PullDistance);
        }

        private static string SpecializationName()
        {
            if (Me.Specialization == WoWSpec.None)
                return "Lowbie";

            string spec = Me.Specialization.ToString().CamelToSpaced();
            int idxLastSpace = spec.LastIndexOf(' ');
            if (idxLastSpace >= 0 && ++idxLastSpace < spec.Length)
                spec = spec.Substring(idxLastSpace);

            return spec;
        }
       
    }
}
