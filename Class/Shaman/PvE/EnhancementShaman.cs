using System;
using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Shaman.PvE
{
    class EnhancementShaman
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        //Need to get imbues working to make life easier

        #region Buffs

        
        public static Composite EnhancementPreCombatBuffs()        
            {
                return new PrioritySelector(
                    //new Decorator(ret => AdvancedAI.PvPRot,
                    //    EnhancementShamanPvP.CreateESPvPBuffs),
                    new Decorator(ret => !Spell.IsCasting() && !Spell.IsGlobalCooldown(),
                        new PrioritySelector(
                            Spell.Cast("Lightning Shield", ret => !StyxWoW.Me.HasAura("Lightning Shield")),
                            CreateShamanImbueMainHandBehavior(Imbue.Windfury, Imbue.Flametongue),
                            CreateShamanImbueOffHandBehavior(Imbue.Flametongue)
                            )));
            }
        
        #endregion

        #region Combat

        public static Composite EnhancementCombat()
            {
                return new PrioritySelector(
                    //new Decorator(ret => AdvancedAI.PvPRot,
                    //              EnhancementShamanPvP.CreateESPvPCombat),

                    Spell.Cast("Healing Stream Totem", ret => Me.HealthPercent < 80 && !Totems.Exist(WoWTotemType.Water)),
                    Spell.Cast("Healing Tide Totem", ret => HealerManager.GetCountWithHealth(55) > 6 && !Totems.Exist(WoWTotemType.Water)),

                    //burst
                    new Decorator(ret => AdvancedAI.Burst,
                                  new PrioritySelector(
                                      Spell.Cast("Stormlash Totem", ret => !Me.HasAura("Stormlash Totem")),
                                      Spell.Cast("Elemental Mastery"),
                                      Spell.Cast("Fire Elemental Totem"),
                                      Spell.Cast("Feral Spirit"),
                                      Spell.Cast("Ascendance", ret => !Me.HasAura("Ascendance")))),

                //new Decorator(ret => Unit.UnfriendlyUnits(10).Count() >= 3,
                //            CreateAoe()),

                    Spell.Cast("Searing Totem", ret => Me.GotTarget && Me.CurrentTarget.SpellDistance() <Totems.GetTotemRange(WoWTotem.Searing) - 2f && !Totems.Exist(WoWTotemType.Fire)),

                    Spell.Cast("Unleash Elements", ret => SpellManager.HasSpell("Unleashed Fury")),

                    Spell.Cast("Elemental Blast", ret => Me.HasAura("Maelstrom Weapon", 1)),

                    new Decorator(ret => Me.HasAura("Maelstrom Weapon", 5),
                                  new PrioritySelector(
                                      Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                                      Spell.Cast("Lightning Bolt"))),

                    //StormBlast
                    new Decorator(ret => (Me.HasAura("Ascendance") && !WoWSpell.FromId(115356).Cooldown),
                                  new Action(ret => Lua.DoString("RunMacroText('/cast Stormblast')"))),

                    Spell.Cast("Stormstrike"),
                    Spell.Cast("Flame Shock", ret => Me.CachedHasAura("Unleash Flame") && !Me.CurrentTarget.HasMyAura("Flame Shock")),
                    Spell.Cast("Lava Lash"),
                    Spell.Cast("Flame Shock", ret => (Me.CachedHasAura("Unleash Flame") && Me.CurrentTarget.CachedGetAuraTimeLeft("Flame Shock") < 10) || !Me.CurrentTarget.HasMyAura("Flame Shock")),
                    
                    Spell.Cast("Unleash Elements"),
                    new Decorator(ret => Me.HasAura("Maelstrom Weapon", 3) && !Me.HasAura("Ascendance"),
                                  new PrioritySelector(
                                      Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                                      Spell.Cast("Lightning Bolt"))),
                    // need to make it at <2
                    Spell.Cast("Ancestral Swiftness", ret => !Me.HasAura("Maelstrom Weapon")),
                    Spell.Cast("Lighting Bolt", ret => Me.HasAura("Ancestral Swiftness")),
                    Spell.Cast("Earth Shock"),

                    Spell.Cast("Earth Elemental Totem", ret => Me.CurrentTarget.IsBoss && SpellManager.Spells["Fire Elemental Totem"].CooldownTimeLeft.Seconds >= 50));

                //need more gear
                //new Decorator(ret => Me.HasAura("Maelstrom Weapon", 1) && !Me.HasAura("Ascendance"),
                //    new PrioritySelector(
                //        Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                //        Spell.Cast("Lightning Bolt")
                //        )
                //    )
                //    );
            }
        
        #endregion

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
//actions.aoe=fire_nova,if=active_flame_shock>=4
//actions.aoe+=/magma_totem,if=active_enemies>5&!totem.fire.active
//actions.aoe+=/searing_totem,if=active_enemies<=5&!totem.fire.active
//actions.aoe+=/lava_lash,if=dot.flame_shock.ticking
//actions.aoe+=/chain_lightning,if=active_enemies>=2&buff.maelstrom_weapon.react>=3
//actions.aoe+=/unleash_elements
//actions.aoe+=/flame_shock,cycle_targets=1,if=!ticking
//actions.aoe+=/stormblast
//actions.aoe+=/fire_nova,if=active_flame_shock>=3
//actions.aoe+=/chain_lightning,if=active_enemies>=2&buff.maelstrom_weapon.react>=1
//actions.aoe+=/stormstrike
//actions.aoe+=/earth_shock,if=active_enemies<4
//actions.aoe+=/feral_spirit
//actions.aoe+=/earth_elemental_totem,if=!active&cooldown.fire_elemental_totem.remains>=50
//actions.aoe+=/spiritwalkers_grace,moving=1
//actions.aoe+=/fire_nova,if=active_flame_shock>=1
                );
        }

        #region Imbue

        private enum Imbue
        {
            None = 0,
            Flametongue = 5,
            Windfury = 283,
        }

        private static Decorator CreateShamanImbueMainHandBehavior(params Imbue[] imbueList)
        {
            return new Decorator(ret => CanImbue(Me.Inventory.Equipped.MainHand),
                new PrioritySelector(
                    imb => imbueList.FirstOrDefault(i => SpellManager.HasSpell(i.ToString() + " Weapon")),

                    new Decorator(
                        ret => Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id != (int)ret
                            && SpellManager.HasSpell(((Imbue)ret).ToString() + " Weapon")
                            && SpellManager.CanCast(((Imbue)ret).ToString() + " Weapon", null, false, false),
                        new Sequence(
                            new Action(ret => Lua.DoString("CancelItemTempEnchantment(1)")),
                            new WaitContinue(1,
                                ret => Me.Inventory.Equipped.MainHand != null && (Imbue)Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id == Imbue.None,
                                new ActionAlwaysSucceed()),
                            new DecoratorContinue(ret => ((Imbue)ret) != Imbue.None,
                                new Sequence(
                                    new Action(ret => SpellManager.Cast(((Imbue)ret).ToString() + " Weapon", null)),
                                    new Action(ret => SetNextAllowedImbueTime())
                                    )
                                )
                            )
                        )
                    )
                );
        }

        private static Decorator CreateShamanImbueOffHandBehavior(params Imbue[] imbueList)
        {
            return new Decorator(ret => CanImbue(Me.Inventory.Equipped.OffHand),
                new PrioritySelector(
                    imb => imbueList.FirstOrDefault(i => SpellManager.HasSpell(i.ToString() + " Weapon")),

                    new Decorator(
                        ret => Me.Inventory.Equipped.OffHand.TemporaryEnchantment.Id != (int)ret
                            && SpellManager.HasSpell(((Imbue)ret).ToString() + " Weapon")
                            && SpellManager.CanCast(((Imbue)ret).ToString() + " Weapon", null, false, false),
                        new Sequence(
                           new Action(ret => Lua.DoString("CancelItemTempEnchantment(2)")),
                            new WaitContinue(1,
                                ret => Me.Inventory.Equipped.OffHand != null && (Imbue)Me.Inventory.Equipped.OffHand.TemporaryEnchantment.Id == Imbue.None,
                                new ActionAlwaysSucceed()),
                            new DecoratorContinue(ret => ((Imbue)ret) != Imbue.None,
                                new Sequence(
                                    new Action(ret => SpellManager.Cast(((Imbue)ret).ToString() + " Weapon", null)),
                                    new Action(ret => SetNextAllowedImbueTime())
                                    )
                                )
                            )
                        )
                    )
                );
        }

        private static DateTime nextImbueAllowed = DateTime.Now;

        public static bool CanImbue(WoWItem item)
        {
            if (item != null && item.ItemInfo.IsWeapon)
            {
                // during combat, only mess with imbues if they are missing
                if (Me.Combat && item.TemporaryEnchantment.Id != 0)
                    return false;

                // check if enough time has passed since last imbue
                // .. guards against detecting is missing immediately after a cast but before buff appears
                // .. (which results in imbue cast spam)
                if (nextImbueAllowed > DateTime.Now)
                    return false;

                switch (item.ItemInfo.WeaponClass)
                {
                    case WoWItemWeaponClass.Axe:
                        return true;
                    case WoWItemWeaponClass.AxeTwoHand:
                        return true;
                    case WoWItemWeaponClass.Dagger:
                        return true;
                    case WoWItemWeaponClass.Fist:
                        return true;
                    case WoWItemWeaponClass.Mace:
                        return true;
                    case WoWItemWeaponClass.MaceTwoHand:
                        return true;
                    case WoWItemWeaponClass.Polearm:
                        return true;
                    case WoWItemWeaponClass.Staff:
                        return true;
                    case WoWItemWeaponClass.Sword:
                        return true;
                    case WoWItemWeaponClass.SwordTwoHand:
                        return true;
                }
            }

            return false;
        }

        public static void SetNextAllowedImbueTime()
        {
            // 2 seconds to allow for 0.5 seconds plus latency for buff to appear
            nextImbueAllowed = DateTime.Now + new TimeSpan(0, 0, 0, 0, 500); // 1500 + (int) StyxWoW.WoWClient.Latency << 1);
        }

        //string ToSpellName(this Imbue i)
        //{
        //    return i.ToString() + " Weapon";
        //}

        private static Imbue GetImbue(WoWItem item)
        {
            if (item != null)
                return (Imbue)item.TemporaryEnchantment.Id;

            return Imbue.None;
        }

        public static bool IsImbuedForDPS(WoWItem item)
        {
            Imbue imb = GetImbue(item);
            return imb == Imbue.Flametongue;
        }

        #endregion

        #region ShamanTalents
        public enum ShamanTalents
        {
            NaturesGuardian = 1,
            StoneBulwarkTotem,
            AstralShift,
            FrozenPower,
            EarthgrabTotem,
            WindwalkTotem,
            CallOfTheElements,
            TotemicRestoration,
            TotemicProjection,
            ElementalMastery,
            AncestralSwiftness,
            EchoOfTheElements,
            HealingTideTotem,
            AncestralGuidance,
            Conductivity,
            UnleashedFury,
            PrimalElementalist,
            ElementalBlast
        }
        #endregion
    }
}
