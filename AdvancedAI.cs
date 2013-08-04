using System.Windows.Forms;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using CommonBehaviors.Actions;
using AdvancedAI.Managers;
using AdvancedAI.Helpers;
using Styx.WoWInternals.WoWObjects;


namespace AdvancedAI
{
    public partial class AdvancedAI : CombatRoutine
    {
        public override sealed string Name { get { return "AdvancedAI [" + StyxWoW.Me.Specialization + "]"; } }
        public override WoWClass Class { get { return StyxWoW.Me.Class; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        
        #region init
        public static AdvancedAI Instance { get; private set; }
        public AdvancedAI()
        {
            Instance = this;

            // Do this now, so we ensure we update our context when needed.
            BotEvents.Player.OnMapChanged += e =>
                {
                    // Don't run this handler if we're not the current routine!
                    if (RoutineManager.Current.Name != Name)
                        return;

                    // Only ever update the context. All our internal handlers will use the context changed event
                    // so we're not reliant on anything outside of ourselves for updates.
                    UpdateContext();
                };
        }

        public override void Initialize()
        {
            RegisterHotkeys();
            TalentManager.Update();
            UpdateContext();
            // NOTE: Hook these events AFTER the context update.
            OnWoWContextChanged += (orig, ne) =>
            {
                Logging.Write(" Context changed, re-creating behaviors");
                RebuildBehaviors();
            };
            RoutineManager.Reloaded += (s, e) =>
            {
                Logging.Write(" Routines were reloaded, re-creating behaviors");
                RebuildBehaviors();
            };
            CombatandBuffSelection();
            MovementManager.Init();
            Dispelling.Init();
            //base.Initialize();
        }

        public override void Pulse()
        {
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                return;

            if (TalentManager.Pulse())
                return;

            UpdateContext();

            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Hunter:
                case WoWClass.DeathKnight:
                case WoWClass.Warlock:
                case WoWClass.Mage:
                    PetManager.Pulse();
                    break;
            }

            //if (!StyxWoW.Me.IsInGroup()) return;
            if (HealerManager.NeedHealTargeting)
                HealerManager.Instance.Pulse();
            //if (Group.MeIsTank && CurrentWoWContext == WoWContext.Instances)
            //    TankManager.Instance.Pulse();

            //base.Pulse();
        }
        #endregion

        #region Requirements
        protected virtual Composite CreateCombat()
        {
            return new HookExecutor("AdvancedAI_Combat_Root",
                "Root composite for AdvancedAI combat. Rotations will be plugged into this hook.",
                new ActionAlwaysFail());
        }
        protected virtual Composite CreateBuffs()
        {
            return new HookExecutor("AdvancedAI_Buffs_Root",
                "Root composite for AdvancedAI buffs. Rotations will be plugged into this hook.",
                new ActionAlwaysFail());
        }
        #endregion
    }    
}
