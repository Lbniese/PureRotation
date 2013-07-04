using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;
using AdvancedAI.Managers;

namespace AdvancedAI.Spec
{
    class MistweaverMonk
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateMMCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMWPvPCombat),
                    Spell.Cast("Mana Tea", ret => Me.ManaPercent < 90),
                    Spell.Cast("Invoke Xuen, the White Tiger", ret => Me.CurrentTarget.IsBoss),
                    Spell.Cast("Touch of Death", ret => Me.HasAura("Death Note")),
                    Spell.Cast("Renewing Mist", on => HealerManager.FindLowestHealthTarget(), ret => !HealerManager.FindLowestHealthTarget().HasAura("Renewing Mist")),
                    Spell.Cast("Surging Mist", on => HealerManager.FindLowestHealthTarget(), ret => HealerManager.FindLowestHealthTarget().HealthPercent < 60 && Me.HasAura("Vital Mists", 5)),
                    Spell.Cast("Chi Wave"),
                    Spell.Cast("Thunder Focus Tea"),
                    Spell.Cast("Blackout Kick", ret => !Me.HasAura("Serpent's Zeal") && Me.HasAura("Muscle Memory")),
                    Spell.Cast("Tiger Palm", ret => Me.HasAura("Muscle Memory")),
                    Spell.Cast("Uplift", ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist")) > 2),
                    Spell.Cast("Expel Harm"),
                    Spell.Cast("Jab")
                    );
            }
        }

        public static Composite CreateMMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMWPvPBuffs));
            }
        }
    }
}
