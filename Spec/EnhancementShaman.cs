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

namespace AdvancedAI.Spec
{
    class EnhancementShaman : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Shaman; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

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
        Composite CreateBuffs()
        {
            return new Decorator(
                    ret => !Spell.IsCasting() && !Spell.IsGlobalCooldown(),
                    new PrioritySelector(

                Spell.Cast("Lightning Shield", ret => !StyxWoW.Me.HasAura("Lightning Shield"))
                //CreateShamanImbueMainHandBehavior(Imbue.Windfury, Imbue.Flametongue),
                //CreateShamanImbueOffHandBehavior(Imbue.Flametongue)

                                        ));
        }
        #endregion

        #region Combat
        Composite CreateCombat()
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
        #endregion


        //#region totem junk
        //public static float GetTotemRange(WoWTotem totem)
        //{
        //    switch (totem)
        //    {
        //        case WoWTotem.HealingStream:
        //        case WoWTotem.Tremor:
        //            return 30f;

        //        case WoWTotem.Searing:
        //            return 25f;

        //        case WoWTotem.Earthbind:
        //            return 10f;

        //        case WoWTotem.Grounding:
        //        case WoWTotem.Magma:
        //            return 8f;

        //        case WoWTotem.StoneBulwark:
        //            // No idea, unlike former glyphed stoneclaw it has a 5 sec pluse shield component so range is more important
        //            return 40f;

        //        case WoWTotem.HealingTide:
        //            return 40f;

        //        case WoWTotem.Stormlash:
        //            return 30f;

        //    }

        //    return 0f;
        //}

        //public static bool Exist(WoWTotemInfo ti)
        //{
        //    return IsRealTotem(ti.WoWTotem);
        //}

        //public static bool Exist(WoWTotemType type)
        //{
        //    WoWTotem wt = GetTotem(type).WoWTotem;
        //    return IsRealTotem(wt);
        //}

        //public static WoWTotemInfo GetTotem(WoWTotem wt)
        //{
        //    return GetTotem(wt.ToType());
        //}

        //public static WoWTotemInfo GetTotem(WoWTotemType type)
        //{
        //    return StyxWoW.Me.Totems[(int)type - 1];
        //}

        //public static bool IsRealTotem(WoWTotem ti)
        //{
        //    return ti != WoWTotem.None
        //        && ti != WoWTotem.DummyAir
        //        && ti != WoWTotem.DummyEarth
        //        && ti != WoWTotem.DummyFire
        //        && ti != WoWTotem.DummyWater;
        //}

        //public static WoWTotemType ToType(this WoWTotem totem)
        //{
        //    return (WoWTotemType)((long)totem >> 32);
        //}
        //#endregion

    }
}
