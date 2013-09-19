using System;
using System.Diagnostics;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using AdvancedAI.Utilities;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Routines;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI
{
    public partial class AdvancedAI : CombatRoutine
    {
        public override sealed string Name { get { return "AdvancedAI [" + Me.Specialization + "]"; } }
        public override WoWClass Class { get { return StyxWoW.Me.Class; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        private static AdvancedAI Instance { get; set; }
        public AdvancedAI() { Instance = this; }

        public override void Initialize()
        {
            RegisterHotkeys();
            LuaCore.PopulateSecondryStats();
            TalentManager.Init();
            TalentManager.Update();
            UpdateContext();
            OnWoWContextChanged += (orig, ne) =>
            {
                Logging.Write("Context changed, re-creating behaviors");
                AssignBehaviors();
                Spell.GcdInitialize();
                Lists.BossList.Init();
            };
            Spell.GcdInitialize();
            Dispelling.Init();
            //testing cached units
            //CachedUnits.Initialize();
            EventHandlers.Init();
            Lists.BossList.Init();
            Instance.AssignBehaviors();
            Logging.Write("Initialization Completed");
        }
        
        public override void ShutDown()
        {
            UnregisterHotkeys();
        }

        public override void Pulse()
        {
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                return;
            if (TalentManager.Pulse())
                return;

            if (TalentManager.EventRebuildTimer.IsFinished && TalentManager.RebuildNeeded)
            {
                TalentManager.RebuildNeeded = false;
                Logging.Write("TalentManager: Rebuilding behaviors due to changes detected.");
                TalentManager.Update();   // reload talents just in case
                AssignBehaviors();
                return;
            }


            UpdateContext();
            Spell.DoubleCastPreventionDict.RemoveAll(t => DateTime.UtcNow > t);

            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Hunter:
                case WoWClass.DeathKnight:
                case WoWClass.Warlock:
                case WoWClass.Mage:
                    PetManager.Pulse();
                    break;
            }

            //Testing chached units
            //CachedUnits.Pulse();

            if (HealerManager.NeedHealTargeting)
                HealerManager.Instance.Pulse();

            if (Movement)
            {
                Helpers.Movement.PulseMovement();
                TargetingGeneral.TargetingPulse();
            }

            if (!BotManager.Current.Name.Equals("BGBuddy") && !BotManager.Current.Name.Equals("Bg Bot"))
                return;
            if (StyxWoW.IsInGame == false || StyxWoW.Me.IsValid == false) return;
            if (StyxWoW.Me.IsActuallyInCombat && Helpers.Movement.MoveTo(StyxWoW.Me.CurrentTarget)) { Blacklist.Flush(); }
            if (TargetingPvP.TargetExists()) { TargetingPvP.GetInCombat(); return; }
            if (StyxWoW.Me.IsDead || StyxWoW.Me.HasAura("Preparation")) { Helpers.Movement.StopMovement(true, true, true, true); StyxWoW.Me.ClearTarget(); return; }
            TargetingPvP.TargetPulse();
        }

        static int countRentrancyStopBot = 0;
        /// <summary>
        /// Stop the Bot writing a reason to the log file.  
        /// Revised to account for TreeRoot.Stop() now 
        /// throwing an exception if called too early 
        /// before tree is run
        /// </summary>
        /// <param name="reason">text to write to log as reason for Bot Stop request</param>
        private static void StopBot(string reason)
        {
            if (!TreeRoot.IsRunning)
                reason = "Bot Cannot Run: " + reason;
            else
            {
                reason = "Stopping Bot: " + reason;

                if (countRentrancyStopBot == 0)
                {
                    countRentrancyStopBot++;
                    if (TreeRoot.Current != null)
                        TreeRoot.Current.Stop();

                    TreeRoot.Stop();
                }
            }

            Logging.Write(reason);
        }
    }
}
