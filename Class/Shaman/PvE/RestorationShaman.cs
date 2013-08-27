using System.Globalization;
using System.Windows.Forms;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Action = Styx.TreeSharp.Action;


namespace AdvancedAI.Spec
{
    class RestorationShaman
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        private static string[] _doNotHeal;
        public static Composite CreateRSCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                var cancelHeal = Math.Max(95, Math.Max(93, Math.Max(55, 25)));
                return new PrioritySelector(
                    Spell.WaitForCastOrChannel(),
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationShamanPvP.CreateRSPvPCombat),
                    new Decorator(ret => (Me.Combat || healtarget.Combat || healtarget.GetPredictedHealthPercent() <= 99) && !Me.Mounted,
                        new PrioritySelector(
                            //Totems.CreateTotemsBehavior(),
                            RollRiptide(),
                            TidalWaves(),
                            new Decorator(ret => AdvancedAI.Dispell,
                                Dispelling.CreateDispelBehavior()),
                            Item.UsePotionAndHealthstone(40),
                            new Throttle(1, 1,
                                new PrioritySelector(
                                    Spell.Cast("Earth Shield", 
                                        on => GetBestEarthShieldTargetInstance(),
                                        ret => !GetBestEarthShieldTargetInstance().HasAura("Earth Shield")))),
                            Spell.Cast("Spirit Link Totem", 
                                on => healtarget,
                                ret => HealerManager.Instance.TargetList.Count(p => p.GetPredictedHealthPercent() < 40 && p.Distance <= Totems.GetTotemRange(WoWTotem.SpiritLink)) >= 3 && AdvancedAI.Burst),
                            new Decorator(ret => healtarget.HealthPercent < 25,
                                new Sequence(
                                    Spell.Cast("Ancestral Swiftness"),
                                    Spell.Cast("Greater Healing Wave", 
                                        on => healtarget))),
                            Spell.Cast("Healing Tide Totem",
                                ret => Me.Combat && HealerManager.Instance.TargetList.Count(p => p.GetPredictedHealthPercent() < 60 && p.Distance <= Totems.GetTotemRange(WoWTotem.HealingTide)) >= (Me.GroupInfo.IsInRaid ? 3 : 2) && AdvancedAI.Burst),
                            Spell.Cast("Healing Stream Totem",
                                ret => Me.Combat && !Totems.Exist(WoWTotemType.Water) && HealerManager.Instance.TargetList.Any(p => p.GetPredictedHealthPercent() < 95 && p.Distance <= Totems.GetTotemRange(WoWTotem.HealingTide))),
                            Spell.Cast("Mana Tide Totem", 
                                ret => !Totems.Exist(WoWTotemType.Water) && Me.ManaPercent < 80),
                            HealingRain(),
                            ChainHeal(),
                            Spell.Cast("Greater Healing Wave", 
                                on => healtarget,
                                ret => AvegreaterhealingWave() < Deficit(),//55
                                cancel => healtarget.HealthPercent > cancelHeal),
                            Spell.Cast("Healing Wave", 
                                on => healtarget,
                                ret => AvehealingWave() < Deficit(),//93
                                cancel => healtarget.HealthPercent > cancelHeal),
                            Spell.Cast("Healing Surge",
                                on => healtarget,
                                ret => AvehealingSurge() < Deficit(),//25
                                cancel => healtarget.HealthPercent > cancelHeal),
                            Spell.Cast("Ascendance",
                                ret => HealerManager.Instance.TargetList.Count(p => p.GetPredictedHealthPercent() < 50) >= 4 && !Me.HasAura("Ascendance") && AdvancedAI.Burst),
                            Riptide(),
                            new Decorator(ret => AdvancedAI.InterruptsEnabled,
                                Common.CreateInterruptBehavior()),
                            //Totems.CreateTotemsBehavior(),
                            Spell.Cast("Lightning Bolt",
                                on => BoltTar(), 
                                ret => TalentManager.HasGlyph("Telluric Currents"), 
                                cancel => healtarget.HealthPercent < 70))));
            }
        }

        public static Composite CreateRSBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationShamanPvP.CreateRSPvPBuffs),
                    Spell.Cast("Water Shield", on => Me, ret => !Me.HasMyAura("Water Shield")),
                    CreateShamanImbueMainHandBehavior(Imbue.Earthliving, Imbue.Flametongue),
                    CreateRSCombat);
            }
        }

        private static ulong guidLastEarthShield = 0;
        private static WoWUnit GetBestEarthShieldTargetInstance()
        {
            WoWUnit target = null;

            if (Unit.NearbyFriendlyPlayers.Any(m => m.HasMyAura("Earth Shield")))
                return null;

            if (Me.GroupInfo.IsInParty)
            {
                target = Group.Tanks.FirstOrDefault(IsValidEarthShieldTarget);
                if (Me.Combat && target == null)
                {
                    target = HealerManager.Instance.TargetList.Where(u => u.Combat && IsValidEarthShieldTarget(u))
                        .OrderByDescending(u => u.MaxHealth)
                        .FirstOrDefault();
                }
            }

            guidLastEarthShield = target != null ? target.Guid : 0;
            return target;
        }

        private enum Imbue
        {
            None = 0,
            Flametongue = 5,
            Earthliving = 3345,
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
            var imb = GetImbue(item);
            return imb == Imbue.Flametongue;
        }

        public static bool IsImbuedForHealing(WoWItem item)
        {
            return GetImbue(item) == Imbue.Earthliving;
        }

        private static bool IsValidEarthShieldTarget(WoWUnit unit)
        {
            if (unit == null || !unit.IsValid || !unit.IsAlive || Unit.GroupMembers.All(g => g.Guid != unit.Guid) || unit.Distance > 99)
                return false;

            return unit.HasMyAura("Earth Shield") || !unit.HasAnyAura("Earth Shield", "Water Shield", "Lightning Shield");
        }

        private static Composite HealingRain()
        {
            return new PrioritySelector(
                context => GetBestHealingRainTarget(),
                new Decorator(
                    ret => ret != null,
                    new PrioritySelector(
                        new Sequence(
                            BuffUnleashLife(on => HealerManager.Instance.TargetList.FirstOrDefault()),
                            Common.CreateWaitForLagDuration(ret => Spell.IsGlobalCooldown()),
                            new WaitContinue(TimeSpan.FromMilliseconds(1500),
                                             until => !Spell.IsGlobalCooldown(LagTolerance.No),
                                             new ActionAlwaysSucceed()),
                            Spell.CastOnGround("Healing Rain", on => (WoWUnit) on, req => true, false)))));
        }

        private static Composite ChainHeal()
        {
            return new PrioritySelector(
                ctx => GetBestChainHealTarget(),
                new Decorator(
                    ret => ret != null,
                    new PrioritySelector(
                        new Sequence(
                            Spell.Cast("Riptide", on => (WoWUnit) on, ret => !((WoWUnit)ret).HasAura("Riptide")),
                            new Wait(TimeSpan.FromMilliseconds(1500), until => !Spell.IsGlobalCooldown(),
                                     new ActionAlwaysFail())),
                        Spell.Cast("Chain Heal", on => (WoWUnit) on))));
        }

        private static Composite RollRiptide()
        {
            return new PrioritySelector(
                Spell.Cast("Riptide", on =>
                {
                    var unit = GetBestRiptideTankTarget();
                    _doNotHeal = new[] { "Reshape Life", "Parasitic Growth", "Cyclone", "Dominate Mind", "Agressive Behavior", "Beast of Nightmares", "Corrupted Healing" };
                    if (unit != null && Spell.CanCastHack("Riptide", unit, skipWowCheck: true) && !unit.HasAnyAura(_doNotHeal))
                    {
                        return unit;
                    }
                    return null;
                }));
        }

        private static Composite TidalWaves()
        {
            return new Decorator(
                ret => IsTidalWavesNeeded,
                new PrioritySelector(
                    Spell.Cast("Riptide", on =>
                    {
                        var unit = GetBestRiptideTarget();
                        return unit;
                    }, ret => !GetBestRiptideTarget().HasMyAura("Riptide"))));
        }

        private static bool IsTidalWavesNeeded
        {
            get
            {
                const int HW = 331;
                const int GHW = 77472;
                const int HS = 8004;

                // WoWAura tw = Me.GetAuraByName("Tidal Waves");
                var stacks = Me.GetAuraStacks("Tidal Waves");

                // 2 stacks means we don't have an issue
                if (stacks >= 2)
                {
                    return false;
                }

                // 1 stack? special case and a spell that will consume it is in progress or our audit count shows its gone
                var castId = Me.CastingSpellId;
                var castname = Me.CastingSpell == null ? "(none)" : Me.CastingSpell.Name;
                return stacks != 1 || castId == HW || castId == GHW || castId == HS;
            }
        }

        private static Composite Riptide()
        {
            return new Decorator(ret =>
                    {
                        var rollCount = HealerManager.Instance.TargetList.Count(u => u.IsAlive && u.HasMyAura("Riptide"));
                        return rollCount < 2;
                    },
                new PrioritySelector(
                    Spell.Cast("Riptide", on =>
                        {
                            // if tank needs Riptide, bail out on Rolling as they have priority
                            if (GetBestRiptideTankTarget() != null)
                                return null;
                            // get the best target from all wowunits in our group
                            var unit = GetBestRiptideTarget();
                            return unit;
                        }, ret => !GetBestRiptideTarget().HasMyAura("Riptide"))));
        }

        private static WoWUnit GetBestRiptideTarget()
        {
            var ripTarget = Clusters.GetBestUnitForCluster(ChainHealPlayers, ClusterType.Chained, ChainHealHopRange);
            return ripTarget;
        }

        private static WoWUnit GetBestChainHealTarget()
        {
            if (!Me.IsInGroup())
                return null;

            if (!Spell.CanCastHack("Chain Heal", Me, skipWowCheck: true))
            {
                return null;
            }

            // search players with Riptide first
            var targetInfo = ChainHealRiptidePlayers
                .Select(p => new { Unit = p, Count = Clusters.GetClusterCount(p, ChainHealPlayers, ClusterType.Chained, ChainHealHopRange) })
                .OrderByDescending(v => v.Count)
                .ThenByDescending(v => Group.Tanks.Any(t => t.Guid == v.Unit.Guid))
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            var target = targetInfo == null ? null : targetInfo.Unit;
            var count = targetInfo == null ? 0 : targetInfo.Count;

            // too few hops? then search any group member
            if (count < 3)
            {
                target = Clusters.GetBestUnitForCluster(ChainHealPlayers, ClusterType.Chained, ChainHealHopRange);
                if (target != null)
                {
                    count = Clusters.GetClusterCount(target, ChainHealPlayers, ClusterType.Chained, ChainHealHopRange);
                    if (count < 3)
                        target = null;
                }
            }

            return target;
        }

        private static WoWUnit GetBestHealingRainTarget()
        {
            if (!Me.IsInGroup() || !Me.Combat)
                return null;

            if (!Spell.CanCastHack("Healing Rain", Me, skipWowCheck: true))
            {
                // Logger.WriteDebug("GetBestHealingRainTarget: CanCastHack says NO to Healing Rain");
                return null;
            }

            // note: expensive, but worth it to optimize placement of Healing Rain by
            // finding location with most heals, but if tied one with most living targets also
            // build temp list of targets that could use heal and are in range + radius
            var coveredTargets = HealerManager.Instance.TargetList
                .Where(u => u.IsAlive && u.DistanceSqr < 50 * 50)
                .ToList();
            var coveredRainTargets = coveredTargets
                .Where(u => u.HealthPercent < 95)
                .ToList();

            // search all targets to find best one in best location to use as anchor for cast on ground
            var t = coveredTargets
                .Where(u => u.DistanceSqr < 40 * 40)
                .Select(p => new
                {
                    Player = p,
                    Count = coveredRainTargets.Count(pp => pp.Location.DistanceSqr(p.Location) < 10 * 10),
                    Covered = coveredTargets.Count(pp => pp.Location.DistanceSqr(p.Location) < 10 * 10)
                })
                .OrderByDescending(v => v.Count)
                .ThenByDescending(v => v.Covered)
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (t != null && t.Count >= 3)
            {
                return t.Player;
            }

            return null;

        }

        private static Composite BuffUnleashLife(UnitSelectionDelegate onUnit)
        {
            return new PrioritySelector(
                Spell.Cast("Unleash Elements",
                    onUnit,
                    ret => IsImbuedForHealing(Me.Inventory.Equipped.MainHand) && (Me.Combat || onUnit(ret).Combat)),
                new ActionAlwaysSucceed()
                );
        }

        private static float ChainHealHopRange
        {
            get
            {
                return TalentManager.Glyphs.Contains("Chaining") ? 25f : 12.5f;
            }
        }

        private static IEnumerable<WoWUnit> ChainHealPlayers
        {
            get
            {
                return HealerManager.Instance.TargetList
                    .Where(u => u.IsAlive && u.DistanceSqr < 40 * 40 && u.GetPredictedHealthPercent() < 90)
                    .Select(u => u);
            }
        }

        private static IEnumerable<WoWUnit> ChainHealRiptidePlayers
        {
            get
            {
                return HealerManager.Instance.TargetList
                    .Where(u => u.IsAlive && u.DistanceSqr < 40 * 40 && u.GetPredictedHealthPercent() < 90 && u.HasMyAura("Riptide"))
                    .Select(u => u);
            }
        }

        private static WoWUnit GetBestRiptideTankTarget()
        {
            WoWUnit ripTarget = Group.Tanks.Where(u => !u.HasAura("Reshape Life") && !u.HasAura("Parasitic Growth") && u.IsAlive && u.Combat && u.DistanceSqr < 40 * 40 && !u.HasMyAura("Riptide") && u.InLineOfSpellSight).OrderBy(u => u.HealthPercent).FirstOrDefault();
            return ripTarget;
        }

        private static WoWUnit BoltTar()
        {
            var bolttarget = Unit.NearbyUnitsInCombatWithMe.FirstOrDefault(u => u.IsTargetingUs() && u.IsHostile && Me.IsSafelyFacing(u));
            return bolttarget;
        }

        #region Spell math

        //private static float _spellPower = Me.SpellPowerModifierPercent;
        //private static float _haste = Me.HasteModifier;
        // && SpellManager.Spells["Healing Wave"].SpellEffect1.Amplitude > 10


        //where Mastery Bonus = (-1 x % Mastery x Target HP) + % Mastery |||| 4.0
        //Mastery bonus on heal = (1 – (% HP of Target/100)) x Maximum Mastery contribution |||| 5.0
        private static readonly double MasteryBonus = (1 - (healtarget.HealthPercent / 100)) * TotalMastery();
        //At lvl90 it works as following 600 Mastery = 1 point of Mastery = 3% of the effect of mastery (for detailed information refer to #10 , with my thanks to Bink )
        //% Healing Increase = (-1 x Max Deep Healing % x HP of target) + (Max Deep Healing %)

        //Our base Mastery percentage with 0 Mastery rating in our gear for MoP is 39%. The reason behind this is 
        //that every lvl90 Resto Shaman has a base mastery of 24% and for MoP we get another 15% ( 3000mastery rating 
        //through our passive Grace of Air raid buff) added to that, bringing us to a total of 39% of Mastery.

        //8680 with gear
        //8680 - 3000(base) = 5680 mastery from gear
        //3000/.15 = 5680/x
        //852 = 3000(x)
        //x = .284(28.4%)
        //28.4 + 24 + 15 == 67.4 (total mastery)

        //mastery - 3000 = mastery from gear
        //mastery from gear * .15 = var1
        //var1/3000 = Mastery percent from gear
        //mastery from gear + base 90 mastery + grace of air mastery  = total mastery %
        private static double TotalMastery()
        {
            var masteryfromgear = LuaCore.Mastery - 3000;
            var var1 = masteryfromgear*.15;
            var masterypctfromgear = var1/3000;
            return (masterypctfromgear + 24 + 15)/100;
            
        }


        private double _averageHeal(string healname)
        {
            double heal = SpellManager.Spells[healname].SpellEffect1.Amplitude;
            var average = Me.CritChance()*(heal*LuaCore._secondaryStats.MasteryCR*1.5*1.286) +
                                      (LuaCore._secondaryStats.Crit - 100)*(heal*LuaCore._secondaryStats.MasteryCR);
            return average;
        }

        //private float _healingWave = 0; //drdamage holds the key
        //ave base = 8345 [+ 75.6% of Spell power].
        //8345 * (33314(sp) * .756(75.6%) = 33530
        //33530 * 1.25(125%) = 41912 (wow tool tip number - ave hit)
        //41912 * 2.00(200% crit) = 83825 (ave crit)
        //(83825 * .14(crit chance)) + (41912 * .86(crit chance - 100)) = 47780 Average total

        //In Code: (Mastery was not nesisary)
        private static double AvehealingWave()
        {
            const int healingwaveBase = 8345;
            var avehit = healingwaveBase * (LuaCore.SpellPower * .756) * 1.25;
            var avecrit = avehit*2;
            var avetotal = (avecrit * Me.CritPercent) + (avecrit * (Me.CritPercent - 100));
            //var avetotalwithmastery = (avetotal * MasteryBonus) + avetotal;
            return avetotal;
        }

        private static double AvegreaterhealingWave()
        {
            const int greaterhealingwaveBase = 15181;
            var avehit = greaterhealingwaveBase * (LuaCore.SpellPower * 1.377) * 1.25;
            var avecrit = avehit * 2;
            var avetotal = (avecrit * Me.CritPercent) + (avecrit * (Me.CritPercent - 100));
            //var avetotalwithmastery = (avetotal * MasteryBonus) + avetotal;
            return avetotal;
        }

        private static double AvehealingSurge()
        {
            const int healingsurgeBase = 12519;
            var avehit = healingsurgeBase * (LuaCore.SpellPower * 1.135) * 1.25;
            var avecrit = avehit * 2;
            var avetotal = (avecrit * Me.CritPercent) + (avecrit * (Me.CritPercent - 100));
            //var avetotalwithmastery = (avetotal*MasteryBonus) + avetotal;
            return avetotal;
        }

        private static double Deficit()
        {
            return healtarget.MaxHealth - healtarget.CurrentHealth;
        }

        //private double _average = _secondaryStats.Crit*(heal*_secondaryStats.MasteryCR*1.5*1.286) +
        //                              (_secondaryStats.Crit - 100)*(heal*_secondaryStats.MasteryCR);
        //Average Heal = Probability of a Crit Heal x (Base Heal x Mastery Bonus x 1.5 x 1.286) + Probability of a Non-Crit Heal x (Base Heal x Mastery Bonus)
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
