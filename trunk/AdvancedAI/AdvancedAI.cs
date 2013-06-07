using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
using CommonBehaviors.Actions;

namespace AdvancedAI
{
    public abstract class AdvancedAI : CombatRoutine
    {       
        #region init
        public override sealed string Name { get { return "AdvancedAI [" + GetType().Name + "]"; } }
        private Composite _combat, _buffs;
        public sealed override Composite CombatBehavior
        {
            get
            {
                return _combat ?? (_combat =
                                   new PrioritySelector(
                                       new Action(ret => { CacheAuras(); return RunStatus.Failure; }),
                                       CreateCombat()));
            }
        }
        public sealed override Composite PreCombatBuffBehavior
        {
            get { return _buffs ?? (_buffs = CreateBuffs()); }
        }
        #endregion

        #region Overrides
        public sealed override void Combat() { base.Combat(); }
        public sealed override void CombatBuff() { base.CombatBuff(); }
        public sealed override void Death() { base.Death(); }
        public sealed override void Heal() { base.Heal(); }
        public sealed override Composite MoveToTargetBehavior { get { return base.MoveToTargetBehavior; } }
        public sealed override bool NeedCombatBuffs { get { return base.NeedCombatBuffs; } }
        public sealed override bool NeedDeath { get { return base.NeedDeath; } }
        public sealed override bool NeedHeal { get { return base.NeedHeal; } }
        public sealed override bool NeedPreCombatBuffs { get { return base.NeedPreCombatBuffs; } }
        public sealed override bool NeedPullBuffs { get { return base.NeedPullBuffs; } }
        public sealed override bool NeedRest { get { return base.NeedRest; } }
        public sealed override void PreCombatBuff() { base.PreCombatBuff(); }
        public sealed override void Pull() { base.Pull(); }
        public sealed override void Rest() { base.Rest(); }
        #endregion

        #region Aura Caching
        public static WoWAuraCollection LocalPlayerAuras, TargetAuras;
        void CacheAuras()
        {
            LocalPlayerAuras = StyxWoW.Me.GetAllAuras();

            if (StyxWoW.Me.GotTarget)
            {
                TargetAuras = StyxWoW.Me.CurrentTarget.GetAllAuras();
            }

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
        Composite CreateRootCombat()
        {
            return new HookExecutor("Ares_Combat_Root",
                "Root composite for Ares combat. Rotations will be plugged into this hook.",
                new ActionAlwaysFail());
        }
        #endregion
    }
}
