using System;
using AdvancedAI.Class.Deathknight.PvE;
using AdvancedAI.Class.Druid.PvE;
using AdvancedAI.Class.Monk.PvE;
using AdvancedAI.Class.Paladin.PvE;
using AdvancedAI.Class.Priest.PvE;
using AdvancedAI.Class.Shaman.PvE;
using AdvancedAI.Class.Warlock.PvE;
using AdvancedAI.Class.Warrior.PvE;
using AdvancedAI.Class.Warrior.PvP;
using JetBrains.Annotations;
using Styx;
using Styx.Common;
using Styx.TreeSharp;
using AdvancedAI.Managers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI
{
    [UsedImplicitly]
    partial class AdvancedAI
    {
        private Composite _combat, _preCombatBuffs, _pull, _heal;
        public override Composite PreCombatBuffBehavior { get { return _preCombatBuffs; } }
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PullBehavior { get { return _pull; } }
        public override Composite HealBehavior { get { return _heal; } }
        static WoWSpec Spec { get { return StyxWoW.Me.Specialization; } }

        readonly WoWContext _context = CurrentWoWContext;
        
        public void AssignBehaviors()
        {
            //Set all to null
            _preCombatBuffs = null;
            _combat = null;
            _heal = null;
            _pull = null;

            CompositeSelector();
        }

        #region ManualContext

        private void CompositeSelector()
        {
            if (_context == WoWContext.Battlegrounds)
            {
                Logging.Write("Initializing PvP Behaviors");
                switch (Me.Specialization)
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
            }
            else
            {
                Logging.Write("Initializing PvE Behaviors");
                switch (StyxWoW.Me.Specialization)
                {
                    case WoWSpec.DeathKnightBlood:
                        if (_combat == null) { _combat = BloodDeathknight.BloodCombat(); }
                        if (_pull == null) { _pull = BloodDeathknight.BloodCombat(); }
                        if (_heal == null) { _heal = BloodDeathknight.BloodCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = BloodDeathknight.BloodPreCombatBuffs(); }
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
                        if (_pull == null) { _pull = null; }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = BrewmasterMonk.BrewmasterPreCombatBuffs(); }
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
                        if (_combat == null) { _combat = AfflictionWarlock.AfflictionCombat(); }
                        if (_pull == null) { _pull = AfflictionWarlock.AfflictionPull(); }
                        if (_heal == null) { _heal = null; }
                        if (_preCombatBuffs == null) { _preCombatBuffs = AfflictionWarlock.AfflictionPreCombatBuffs(); }
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
                        Logging.Write("WoWSpec Arms");
                        if (_combat == null) { _combat = ArmsWarrior.ArmsCombat(); }
                        if (_pull == null) { _pull = ArmsWarrior.ArmsPull(); }
                        if (_heal == null) { _heal = ArmsWarrior.ArmsCombat(); }
                        if (_preCombatBuffs == null) { _preCombatBuffs = ArmsWarrior.ArmsPreCombatBuffs(); }
                        break;
                    case WoWSpec.WarriorFury:
                        Logging.Write("WoWSpec Fury");
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
            }
        }
        #endregion
    }
}
