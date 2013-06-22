using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class EnhancementShaman// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Shaman; } }
        //public override WoWSpec Spec { get { return WoWSpec.ShamanEnhancement; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        //Need to get imbues working to make life easier
        public enum Imbue
        {
            None = 0,

            Flametongue = 5,
            Windfury = 283,
            Earthliving = 3345,
            Frostbrand = 2,
            Rockbiter = 3021
        }

        #region Buffs
        public static Composite CreateESBuffs
        {
            get
            {
                return new Decorator(
                        ret => !Spell.IsCasting() && !Spell.IsGlobalCooldown(),
                        new PrioritySelector(

                    Spell.Cast("Lightning Shield", ret => !StyxWoW.Me.HasAura("Lightning Shield"))
                    //CreateShamanImbueMainHandBehavior(Imbue.Windfury, Imbue.Flametongue),
                    //CreateShamanImbueOffHandBehavior(Imbue.Flametongue)
                                            ));
            }
        }
        #endregion

        #region Combat
        public static Composite CreateESCombat
        {
            get
            {
                return new PrioritySelector(


                    //CreateShamanImbueMainHandBehavior(Imbue.Windfury, Imbue.Flametongue),
                    //CreateShamanImbueOffHandBehavior(Imbue.Flametongue),

                    Spell.Cast("Healing Stream Totem", ret => Me.HealthPercent < 80),
                    Spell.Cast("Stormlash Totem", ret => PartyBuff.WeHaveBloodlust && !Me.HasAura("Stormlash Totem")),

                    Spell.Cast("Lightning Shield", ret => !Me.HasAura("Lightning Shield")),

                    Spell.Cast("Fire Elemental Totem", ret => Me.CurrentTarget.IsBoss),

                    Spell.Cast("Ascendance", ret => Me.CurrentTarget.IsBoss),
                    //this will have to be fixed Major Part of dps
                    //Spell.Cast("Searing Totem", ret => Me.GotTarget
                    //            && Me.CurrentTarget.SpellDistance() < GetTotemRange(WoWTotem.Searing) - 2f
                    //            && !Exist(WoWTotemType.Fire)),

                    Spell.Cast("Unleash Elements"),

                    new Decorator(ret => Me.HasAura("Maelstrom Weapon", 5),
                        new PrioritySelector(
                            Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                            Spell.Cast("Lightning Bolt")
                            )
                        ),


                    new Decorator(ret => (Me.HasAura("Ascendance") && !WoWSpell.FromId(115356).Cooldown),
                        new Action(ret => Lua.DoString("RunMacroText('/cast Stormblast')"))),


                    Spell.Cast("Stormstrike"),

                    Spell.Cast("Flame Shock", ret => Me.HasAura("Unleash Flame") && !Me.CurrentTarget.HasMyAura("Flame Shock")),

                    Spell.Cast("Lava Lash"),

                    Spell.Cast("Flame Shock", ret => Me.HasAura("Unleash Flame") ||
                               !Me.HasAura("Unleash Flame") && !Me.CurrentTarget.HasMyAura("Flame Shock") && SpellManager.Spells["Unleashed Elements"].CooldownTimeLeft.TotalSeconds >= 5),

                    Spell.Cast("Unleash Elements"),

                    new Decorator(ret => Me.HasAura("Maelstrom Weapon", 3) && !Me.HasAura("Ascendance"),
                        new PrioritySelector(
                            Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                            Spell.Cast("Lightning Bolt")
                            )
                        ),
                    // need to make it at <2
                    Spell.Cast("Ancestral Swiftness", ret => !Me.HasAura("Maelstrom Weapon")),

                    Spell.Cast("Lighting Bolt", ret => Me.HasAura("Ancestral Swiftness")),

                    Spell.Cast("Earth Shock"),

                    Spell.Cast("Feral Spirit"),

                    Spell.Cast("Earth Elemental Totem", ret => Me.CurrentTarget.IsBoss && SpellManager.Spells["Fire Elemental Totem"].CooldownTimeLeft.Seconds >= 50)

                    //need more gear
                    //new Decorator(ret => Me.HasAura("Maelstrom Weapon", 1) && !Me.HasAura("Ascendance"),
                    //    new PrioritySelector(
                    //        Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                    //        Spell.Cast("Lightning Bolt")
                    //        )
                    //    )
                        );
            }
        }
        #endregion
    }
}
