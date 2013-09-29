using System;
using AdvancedAI.Class.Deathknight.PvE;
using AdvancedAI.Class.Druid.PvE;
using AdvancedAI.Class.Monk.PvE;
using AdvancedAI.Class.Paladin.PvE;
using AdvancedAI.Class.Priest.PvE;
using AdvancedAI.Class.Shaman.PvE;
using AdvancedAI.Class.Warrior.PvE;
using AdvancedAI.Class.Warrior.PvP;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using Styx;
using Styx.Common;
using Styx.TreeSharp;

namespace AdvancedAI
{
    partial class AdvancedAI
    {
        private Composite _combat, _preCombatBuffs, _pull, _heal, _deathBehavior;
        public override Composite PreCombatBuffBehavior { get { return _preCombatBuffs; } }
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PullBehavior { get { return _pull; } }
        public override Composite HealBehavior { get { return _heal; } }
        public override Composite DeathBehavior { get { return _deathBehavior; } }
        WoWContext _context = CurrentWoWContext;
        
        public void AssignBehaviors()
        {
            //Set all to null
            _preCombatBuffs = null;
            _combat = null;
            _heal = null;
            _pull = null;

            if (ManualContext)
                CompositeSelector();
            else
            {
                SetRotation();

                EnsureComposite(true, _context, BehaviorType.Combat);
                EnsureComposite(false, _context, BehaviorType.Heal);
                EnsureComposite(false, _context, BehaviorType.PreCombatBuffs);
                EnsureComposite(false, _context, BehaviorType.Pull);
                EnsureComposite(false, _context, BehaviorType.Death);
            }
        }


        /// <summary>Set the Rotations</summary>
        private void SetRotation()
        {
            if (_preCombatBuffs == null)
            {
                Logging.Write("Initializing Pre-Combat Buffs");
                _preCombatBuffs = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.PreCombatBuffs)));
            }

            if (_combat == null)
            {
                Logging.Write("Initializing Combat");
                _combat = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.Combat)));
            }

            if (_heal == null)
            {
                Logging.Write("Initializing Healing");
                _heal = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.Heal)));
            }

            if (_pull == null && Movement)
            {
                Logging.Write("Initializing Pulling");
                _pull = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.Pull)));
            }

            if (_deathBehavior == null)
            {
                Logging.Write("Initializing Death Behavior");
                _deathBehavior = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.Death)));
            }
        }

        private static string HookName(BehaviorType typ)
        {
            return "AdvancedAI." + typ.ToString();
        }

        private void EnsureComposite(bool error, WoWContext context, BehaviorType type)
        {
            var count = 0;

            // Logger.WriteDebug("Creating " + type + " behavior.");

            var composite = CompositeBuilder.GetComposite(Class, TalentManager.CurrentSpec, type, context, out count);

            TreeHooks.Instance.ReplaceHook(HookName(type), composite);

            if ((composite == null || count <= 0) && error)
            {
                StopBot(string.Format("AdvancedAI does not support {0} for this {1} {2} in {3} context!", type, StyxWoW.Me.Class, TalentManager.CurrentSpec, context));
            }
        }

        #region ManualContext

        private Composite CompositeSelector()
        {
            if (_context == WoWContext.Battlegrounds)
            {
                switch (StyxWoW.Me.Specialization)
                {
                    case WoWSpec.DeathKnightBlood:
                        if (_combat == null) { _combat = null; } //Needs Changing when PvP cc is written
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DeathKnightFrost:
                        if (_combat == null) { _combat = null; } //Needs Changing when PvP cc is written
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DeathKnightUnholy:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DruidBalance:
                        if (_combat == null) { _combat = null; } // Will need Adding
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DruidFeral:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DruidGuardian:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DruidRestoration:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterBeastMastery:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterMarksmanship:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterSurvival:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageArcane:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageFire:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageFrost:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MonkBrewmaster:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MonkMistweaver:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MonkWindwalker:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PaladinHoly:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PaladinProtection:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PaladinRetribution:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PriestDiscipline:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PriestHoly:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PriestShadow:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.RogueAssassination:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.RogueCombat:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.RogueSubtlety:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.ShamanElemental:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.ShamanEnhancement:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.ShamanRestoration:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarlockAffliction:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarlockDemonology:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarlockDestruction:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarriorArms:
                        if (_combat == null) { _combat = ArmsWarriorPvP.ArmsPvPCombat(); }
                        if (_pull == null) { _pull = ArmsWarriorPvP.ArmsPvPCombat(); }
                        if (_heal == null) { _heal = ArmsWarriorPvP.ArmsPvPCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ArmsWarriorPvP.ArmsPvPPreCombatBuffs(); }
                        break;
                    case WoWSpec.WarriorFury:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarriorProtection:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                }
                return null;   
            }
            else
            {
                switch (StyxWoW.Me.Specialization)
                {
                    case WoWSpec.DeathKnightBlood:
                        if (_combat == null) { _combat = BloodDeathknight.BloodDKCombat(); }
                        if (_pull == null) { _pull = BloodDeathknight.BloodDKCombat(); }
                        if (_heal == null) { _heal = BloodDeathknight.BloodDKCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = BloodDeathknight.BloodDKPreCombatBuffs(); }
                        break;
                    case WoWSpec.DeathKnightFrost:
                        if (_combat == null) { _combat = FrostDeathknight.FrostDKCombat(); }
                        if (_pull == null) { _pull = FrostDeathknight.FrostDKCombat(); }
                        if (_heal == null) { _heal = FrostDeathknight.FrostDKCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = FrostDeathknight.FrostDKPreCombatBuffs(); }
                        break;
                    case WoWSpec.DeathKnightUnholy:
                        if (_combat == null) { _combat = UnholyDeathknight.UnholyDKCombat(); }
                        if (_pull == null) { _pull = UnholyDeathknight.UnholyDKCombat(); }
                        if (_heal == null) { _heal = UnholyDeathknight.UnholyDKCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = UnholyDeathknight.UnholyDKCombat(); }
                        break;
                    case WoWSpec.DruidBalance:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.DruidFeral:
                        if (_combat == null) { _combat = FeralDruid.FeralCombat(); }
                        if (_pull == null) { _pull = FeralDruid.FeralCombat(); }
                        if (_heal == null) { _heal = FeralDruid.FeralCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = FeralDruid.FeralCombat(); }
                        break;
                    case WoWSpec.DruidGuardian:
                        if (_combat == null) { _combat = GuardianDruid.GuardianCombat(); }
                        if (_pull == null) { _pull = GuardianDruid.GuardianCombat(); }
                        if (_heal == null) { _heal = GuardianDruid.GuardianCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = GuardianDruid.GuardianCombat(); }
                        break;
                    case WoWSpec.DruidRestoration:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterBeastMastery:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterMarksmanship:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.HunterSurvival:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageArcane:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageFire:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MageFrost:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MonkBrewmaster:
                        if (_combat == null) { _combat = BrewmasterMonk.BrewmasterCombat(); }
                        if (_pull == null) { _pull = BrewmasterMonk.BrewmasterCombat(); }
                        if (_heal == null) { _heal = BrewmasterMonk.BrewmasterCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = BrewmasterMonk.BrewmasterCombat(); }
                        break;
                    case WoWSpec.MonkMistweaver:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.MonkWindwalker:
                        if (_combat == null) { _combat = WindwalkerMonk.WindwalkerCombat(); }
                        if (_pull == null) { _pull = WindwalkerMonk.WindwalkerCombat(); }
                        if (_heal == null) { _heal = WindwalkerMonk.WindwalkerCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = WindwalkerMonk.WindwalkerCombat(); }
                        break;
                    case WoWSpec.PaladinHoly:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PaladinProtection:
                        if (_combat == null) { _combat = ProtectionPaladin.ProtectionCombat(); }
                        if (_pull == null) { _pull = ProtectionPaladin.ProtectionCombat(); }
                        if (_heal == null) { _heal = ProtectionPaladin.ProtectionCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ProtectionPaladin.ProtectionPreCombatBuffs(); }
                        break;
                    case WoWSpec.PaladinRetribution:
                        if (_combat == null) { _combat = RetributionPaladin.RetributionCombat(); }
                        if (_pull == null) { _pull = RetributionPaladin.RetributionCombat(); }
                        if (_heal == null) { _heal = RetributionPaladin.RetributionCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = RetributionPaladin.RetributionPreCombatBuffs(); }
                        break;
                    case WoWSpec.PriestDiscipline:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PriestHoly:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.PriestShadow:
                        if (_combat == null) { _combat = ShadowPriest.ShadowCombat(); }
                        if (_pull == null) { _pull = ShadowPriest.ShadowCombat(); }
                        if (_heal == null) { _heal = ShadowPriest.ShadowCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ShadowPriest.ShadowPreCombatBuffs(); }
                        break;
                    case WoWSpec.RogueAssassination:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.RogueCombat:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.RogueSubtlety:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.ShamanElemental:
                        if (_combat == null) { _combat = ElementalShaman.ElementalCombat(); }
                        if (_pull == null) { _pull = ElementalShaman.ElementalCombat(); }
                        if (_heal == null) { _heal = ElementalShaman.ElementalCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ElementalShaman.ElementalCombat(); }
                        break;
                    case WoWSpec.ShamanEnhancement:
                        if (_combat == null) { _combat = EnhancementShaman.EnhancementCombat(); }
                        if (_pull == null) { _pull = EnhancementShaman.EnhancementCombat(); }
                        if (_heal == null) { _heal = EnhancementShaman.EnhancementCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = EnhancementShaman.EnhancementPreCombatBuffs(); }
                        break;
                    case WoWSpec.ShamanRestoration:
                        if (_combat == null) { _combat = RestorationShaman.RestorationCombat(); }
                        if (_pull == null) { _pull = RestorationShaman.RestorationCombat(); }
                        if (_heal == null) { _heal = RestorationShaman.RestorationCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = RestorationShaman.RestorationPreCombatBuffs(); }
                        break;
                    case WoWSpec.WarlockAffliction:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarlockDemonology:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarlockDestruction:
                        if (_combat == null) { _combat = null; }
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = null; }
                        break;
                    case WoWSpec.WarriorArms:
                        if (_combat == null) { _combat = ArmsWarrior.ArmsCombat(); }
                        if (_pull == null) { _pull = ArmsWarrior.ArmsPull(); }
                        if (_heal == null) { _heal = ArmsWarrior.ArmsCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ArmsWarrior.ArmsPreCombatBuffs(); }
                        break;
                    case WoWSpec.WarriorFury:
                        if (_combat == null) { _combat = FuryWarrior.FuryCombat(); }
                        if (_pull == null) { _pull = FuryWarrior.FuryPull(); }
                        if (_heal == null) { _heal = FuryWarrior.FuryCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = FuryWarrior.FuryPreCombatBuffs(); }
                        break;
                    case WoWSpec.WarriorProtection:
                        if (_combat == null) { _combat = ProtectionWarrior.ProtCombat(); }
                        if (_pull == null) { _pull = ProtectionWarrior.ProtCombat(); }
                        if (_heal == null) { _heal = ProtectionWarrior.ProtCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ProtectionWarrior.ProtPreCombatBuffs(); }
                        break;
                }
                return null;   
            }
        }
        #endregion

        /// <summary>
        /// This behavior wraps the child behaviors in a 'FrameLock' which can provide a big performance improvement 
        /// if the child behaviors makes multiple api calls that internally run off a frame in WoW in one CC pulse.
        /// </summary>
        private class LockSelector : PrioritySelector
        {
            delegate RunStatus TickDelegate(object context);

            TickDelegate _TickSelectedByUser;

            public LockSelector(params Composite[] children)
                : base(children)
            {
                if (true)// Option to use framelock goes here
                    _TickSelectedByUser = TickWithFrameLock;
                else
                    _TickSelectedByUser = TickNoFrameLock;
            }

            public override RunStatus Tick(object context)
            {
                return _TickSelectedByUser(context);
            }

            private RunStatus TickWithFrameLock(object context)
            {
                using (StyxWoW.Memory.AcquireFrame())
                {
                    return base.Tick(context);
                }
            }

            private RunStatus TickNoFrameLock(object context)
            {
                return base.Tick(context);
            }

        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class BehaviorAttribute : Attribute
    {
        public BehaviorAttribute(BehaviorType type, WoWClass @class = WoWClass.None, WoWSpec spec = (WoWSpec) int.MaxValue, WoWContext context = WoWContext.All, int priority = 0)
        {
            Type = type;
            SpecificClass = @class;
            SpecificSpec = spec;
            SpecificContext = context;
            PriorityLevel = priority;
        }

        public BehaviorType Type { get; private set; }
        public WoWSpec SpecificSpec { get; private set; }
        public WoWContext SpecificContext { get; private set; }
        public WoWClass SpecificClass { get; private set; }
        public int PriorityLevel { get; private set; }
    }

    public class CallTrace : PrioritySelector
    {
        public static DateTime LastCall { get; set; }
        public static ulong CountCall { get; set; }
        public static bool TraceActive { get { return AdvancedAI.Trace; } }

        public string Name { get; set; }

        private static bool _init = false;

        private static void Initialize()
        {
            if (_init)
                return;

            _init = true;
        }

        public CallTrace(string name, params Composite[] children)
            : base(children)
        {
            Initialize();

            Name = name;
            LastCall = DateTime.MinValue;
        }

        public override RunStatus Tick(object context)
        {
            RunStatus ret;
            CountCall++;

            if (!TraceActive)
            {
                ret = base.Tick(context);
            }
            else
            {
                DateTime started = DateTime.Now;
                Logging.WriteDiagnostic("... enter: {0}", Name);
                ret = base.Tick(context);
                Logging.WriteDiagnostic("... leave: {0}, took {1} ms", Name, (ulong)(DateTime.Now - started).TotalMilliseconds);
            }

            return ret;
        }

    }
}
