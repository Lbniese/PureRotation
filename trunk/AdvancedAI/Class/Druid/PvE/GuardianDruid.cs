using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommonBehaviors.Actions;
using JetBrains.Annotations;
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
    [UsedImplicitly]
    class GuardianDruid
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateGDCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        GuardianDruidPvP.CreateGDPvPCombat),
                    // Don't do anything if we have no target, nothing in melee range, or we're casting. (Includes vortex!)
                new Decorator(ret => !StyxWoW.Me.GotTarget || !StyxWoW.Me.CurrentTarget.IsWithinMeleeRange || StyxWoW.Me.IsCasting || StyxWoW.Me.CurrentPendingCursorSpell != null,
                    new ActionAlwaysSucceed()),

                new Action(ret =>
                {
                    if (IsHorridonFight())
                        DoBossMarking();
                    return RunStatus.Failure;
                }),

                Spell.Cast("Bear Form", ret => StyxWoW.Me.Shapeshift != ShapeshiftForm.Bear),

                CreateCooldowns(),
                    // So.. the guardian rotation boils down to... 4 abilities.
                    // 5 when counting Swipe in AOE rotations

                // So, theorycrafted this myself. The returns on this are worth using it. Unfortunately, you'll
                    // rarely have > 90 rage when tanking properly (aka; keeping up a high SD uptime)
                    // This could be tweaked to less rage [50] to take advantage of the crazy absorb you get from it
                    // But for now, we'll leave it at 90 and hit Maul manually when required. (A lot of bosses this tier auto-attack
                    // for jack shit, so we'll leave it up to the user to deal with it)
                Spell.Cast("Maul", ret => StyxWoW.Me.RagePercent > 90 && StyxWoW.Me.HasAura("Tooth and Claw")),
                    // Mangle on CD
                Spell.Cast("Mangle"),
                    // Thrash on CD to keep up the debuffs + mangle procs
                Spell.Cast("Thrash", reqs => Spell.GetSpellCooldown("Thrash").TotalSeconds <= 0),
                    // AOE comes in before lac/ff
                CreateAoe(),
                    // Lac for threat + mangle procs
                Spell.Cast("Lacerate"),
                    // Keep up the sunders + mangle procs (not great for threat, hence lowest priority)
                Spell.Cast("Faerie Fire"),

                Spell.Cast("Maul", ret => !IsCurrentTank())
                    // Symbiosis effect :)
                    //Cast("Consecration")
                );
            }
        }

        public static Composite CreateGDBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        GuardianDruidPvP.CreateGDPvPBuffs));
            }
        }

        private static Composite CreateCooldowns()
        {
            return new PrioritySelector(
                Spell.Cast("Cenarion Ward", on => Me),
                // Enrage if we need 20 more rage for a FR or SD
                Spell.Cast("Enrage", ret => StyxWoW.Me.RagePercent < 40),
                // Do interrupts when we're allowed to.
                Spell.Cast("Skull Bash", on => InterruptableUnit(), ret => AdvancedAI.InterruptsEnabled && InterruptableUnit() != null),

                //Cast("Incarnation: Son of Ursoc", ret => !StyxWoW.Me.HasAura("Berserk")),

                // Symbiosis effect.
                //Cast("Bone Shield", ret => StyxWoW.Me, ret => !StyxWoW.Me.HasAura("Bone Shield")),
                Spell.Cast("Incarnation", ret => KeyboardPolling.IsKeyDown(Keys.Q) && KeyboardPolling.IsKeyDown(Keys.LMenu)),

                // Could be tweaked to be lower. Probably 40.
                Spell.Cast("Barkskin", ret => StyxWoW.Me.HealthPercent <= 60),

                // Could be tweaked to be lower, again. 50 seems reasonable. May add a toggle for this to deal with big spike damage manually.
                Spell.Cast("Survival Instincts", ret => StyxWoW.Me.HealthPercent <= 30),

                // We need health FAST. Get some.
                Spell.Cast("Might of Ursoc", ret => StyxWoW.Me.HealthPercent <= 30),

                // Never higher than 70 here. It heals 30% of our health. Wasted heal if we use it above 70.
                Spell.Cast("Renewal", ret => StyxWoW.Me.HealthPercent <= 50 || StyxWoW.Me.HasAura("Might of Ursoc")),

                // Emergency heals.
                Item.CreateUsePotionAndHealthstone(40, 0),

                // Only use FR when we have full rage. Its probably not worth blowing it when we don't need to.
                // Also, don't overwrite it if we're glyphed for it!
                Spell.Cast("Frenzied Regeneration",
                    ret =>
                    StyxWoW.Me.HealthPercent <= 65 && StyxWoW.Me.CurrentRage >= 60 &&
                    !StyxWoW.Me.HasAura("Frenzied Regeneration")),

                // Don't overwrite SDs. But keep it up as much as possible.
                Spell.Cast("Savage Defense", ret => !StyxWoW.Me.HasAura("Savage Defense"))
                );
        }

        private static Composite CreateAoe()
        {
            return new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 2,
                new PrioritySelector(
                // Best whenn used on 3-5 mobs. Not 30+
                    Spell.Cast("Berserk", ret => !StyxWoW.Me.HasAura("Incarnation: Son of Ursoc")),
                    Spell.Cast("Swipe")
                    ));
        }

        #region Target Tank Tracking & Tank Helpers
        // So, this code is just to track who the current tank is on the mob we're looking at.
        // Sometimes using threat is fine, sometimes the boss switches targets to cast an ability.
        // We want to ensure that we're the ones with threat. If not, then we want to Maul spam for some extra DPS and T&C procs
        // to help healers out and keep our other tank's damage low.
        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.CurrentTargetGuid == StyxWoW.Me.Guid;
        }

        static readonly HashSet<uint> IgnoreInterruptMobs = new HashSet<uint>
        {

        };

        private static WoWUnit InterruptableUnit()
        {
            // Simple 'nuff. First one we're able to interrupt!
            // Skull Bash has a 13yd range (charge) so make sure we use it if we can.
            // TODO: Figure out if we want a toggle for which list to use. For now, just using melee range mobs to ensure we're not charging off
            // while tanking a boss, to do an interrupt on a mob somewhere.
            return
                Unit.UnfriendlyUnits(13).FirstOrDefault(
                    u =>
                    !IgnoreInterruptMobs.Contains(u.Entry) && /*u.Distance <= 13 &&*/ u.IsCasting &&
                    u.CanInterruptCurrentSpellCast);
        }

        #endregion

        #region Boss Helpers

        private static bool IsHorridonFight()
        {
            return ObjectManager.GetObjectsOfType<WoWUnit>().Any(u => u.Entry == 68476 || u.Entry == 62983);
        }

        public int UnitGetTotalAbsorbs(WoWUnit unit)
        {
            return StyxWoW.Memory.Read<int>(
                StyxWoW.Memory.CallInjected(StyxWoW.Memory.GetAbsolute((IntPtr)(0x8EE050 - 0x400000)),
                    CallingConvention.ThisCall,
                    (uint)unit.BaseAddress));
        }

        static readonly HashSet<uint> HorridonMarkTargets = new HashSet<uint>
            {
                69175, // Ferraki Wastewalker
                69164, // Gurubashi Venom Priest
                69314, // Venomous Effusion
                69178, // Drakkari Frozen Warlord
                69177, // Amani Warbear
                69176, // Amanishi Beast Shaman


                31146, // DEBUG: Raider's Training Dummy
                67127, // DEBUG: Training Dummy

                //62995, // Animated Protector
            };

        private static void DoBossMarking()
        {
            var units =
                ObjectManager.GetObjectsOfType<WoWUnit>()
                // NOTE: Need to add IsVisuallyDead to HB
                             .Where(u => !u.IsDead && u.Attackable && !u.IsFriendly && u.ZDiff <= 6)
                             .OrderBy(u => u.Distance);

            var dinomancer = units.FirstOrDefault(u => u.Entry == 69221);
            var markersUsed = new HashSet<RaidTargetMarker>();

            if (dinomancer != null)
            {
                if (dinomancer.GetMark() != RaidTargetMarker.Skull)
                {
                    dinomancer.Mark(RaidTargetMarker.Skull);
                }
                markersUsed.Add(RaidTargetMarker.Skull);
            }

            var markableUnits = units.Where(u => HorridonMarkTargets.Contains(u.Entry));

            var marks = markableUnits.Select(u => u.GetMark());
            markersUsed.UnionWith(marks);

            bool skullAllowed = dinomancer == null;

            foreach (var u in markableUnits)
            {
                if (u.GetMark() == RaidTargetMarker.None)
                {
                    for (int i = (int)RaidTargetMarker.Skull - (skullAllowed ? 0 : 1); i >= 0; i--)
                    {
                        if (markersUsed.Contains((RaidTargetMarker)i))
                            continue;

                        u.Mark((RaidTargetMarker)i);
                        markersUsed.Add((RaidTargetMarker)i);
                        break;
                    }
                }
            }
        }

        #endregion

        #region DruidTalents
        public enum DruidTalents
        {
            FelineSwiftness = 1,//Tier 1
            DisplacerBeast,
            WildCharge,
            NaturesSwiftness,//Tier 2
            Renewal,
            CenarionWard,
            FaerieSwarm,//Tier 3
            MassEntanglement,
            Typhoon,
            SouloftheForest,//Tier 4
            Incarnation,
            ForceofNature,
            DisorientingRoar,//Tier 5
            UrsolsVortex,
            MightyBash,
            HeartoftheWild,//Tier 6
            DreamofCenarius,
            NaturesVigil
        }
        #endregion
    }
}
