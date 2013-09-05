using System;
using Styx;
using Styx.Common;
using Styx.TreeSharp;

namespace AdvancedAI
{
    sealed partial class AdvancedAI
    {
        #region Overrides
        private Composite _combat, _buffs, _pull, _heal;
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PreCombatBuffBehavior { get { return _buffs; } }
        public override Composite PullBehavior { get { return _pull; } }
        public override Composite HealBehavior { get { return _heal; } }
        #endregion

        #region Rotation Selection

        private void CombatandBuffSelection()
        {
            if (_combat == null)
            {
                Logging.Write("Initializing combat behaviors.");
                _combat = null;
            }
            if (_buffs == null)
            {
                Logging.Write("Initializing buffs behaviors.");
                _buffs = null;
            }
            if (_pull == null)
            {
                Logging.Write("Initializing pull behaviors.");
                _pull = null;
            }
            if (_heal == null)
            {
                Logging.Write("Initializing heal behaviors.");
                _heal = null;
            }
            switch (StyxWoW.Me.Specialization)
            {
                case WoWSpec.DeathKnightBlood:
                    if (_combat == null) { _combat = Spec.BloodDeathknight.CreateBDKCombat; }
                    if (_pull == null) { _pull = Spec.BloodDeathknight.CreateBDKCombat; }
                    if (_heal == null) { _heal = Spec.BloodDeathknight.CreateBDKCombat; }
                    if (_buffs == null) { _buffs = Spec.BloodDeathknight.CreateBDKBuffs; }
                    break;
                case WoWSpec.DeathKnightFrost:
                    if (_combat == null) { _combat = Spec.FrostDeathknight.CreateFDKCombat; }
                    if (_pull == null) { _pull = Spec.FrostDeathknight.CreateFDKCombat; }
                    if (_heal == null) { _heal = Spec.FrostDeathknight.CreateFDKCombat; }
                    if (_buffs == null) { _buffs = Spec.FrostDeathknight.CreateFDKBuffs; }
                    break;
                case WoWSpec.DeathKnightUnholy:
                    if (_combat == null) { _combat = Spec.UnholyDeathknight.CreateUDKCombat; }
                    if (_pull == null) { _pull = Spec.UnholyDeathknight.CreateUDKCombat; }
                    if (_heal == null) { _heal = Spec.UnholyDeathknight.CreateUDKCombat; }
                    if (_buffs == null) { _buffs = Spec.UnholyDeathknight.CreateUDKBuffs; }
                    break;
                case WoWSpec.DruidBalance:
                    if (_combat == null) { _combat = Spec.BalanceDruid.CreateBDCombat; }
                    if (_pull == null) { _pull = Spec.BalanceDruid.CreateBDCombat; }
                    if (_heal == null) { _heal = Spec.BalanceDruid.CreateBDCombat; }
                    if (_buffs == null) { _buffs = Spec.BalanceDruid.CreateBDBuffs; }
                    break;
                case WoWSpec.DruidFeral:
                    if (_combat == null) { _combat = Spec.FeralDruid.CreateFDCombat; }
                    if (_pull == null) { _pull = Spec.FeralDruid.CreateFDCombat; }
                    if (_heal == null) { _heal = Spec.FeralDruid.CreateFDCombat; }
                    if (_buffs == null) { _buffs = Spec.FeralDruid.CreateFDBuffs; }
                    break;
                case WoWSpec.DruidGuardian:
                    if (_combat == null) { _combat = Spec.GuardianDruid.CreateGDCombat; }
                    if (_pull == null) { _pull = Spec.GuardianDruid.CreateGDCombat; }
                    if (_heal == null) { _heal = Spec.GuardianDruid.CreateGDCombat; }
                    if (_buffs == null) { _buffs = Spec.GuardianDruid.CreateGDBuffs; }
                    break;
                case WoWSpec.DruidRestoration:
                    if (_combat == null) { _combat = Spec.RestorationDruid.CreateRDCombat; }
                    if (_pull == null) { _pull = Spec.RestorationDruid.CreateRDCombat; }
                    if (_heal == null) { _heal = Spec.RestorationDruid.CreateRDCombat; }
                    if (_buffs == null) { _buffs = Spec.RestorationDruid.CreateRDBuffs; }
                    break;
                case WoWSpec.HunterBeastMastery:
                    if (_combat == null) { _combat = Spec.BeastmasterHunter.CreateBMHCombat; }
                    if (_pull == null) { _pull = Spec.BeastmasterHunter.CreateBMHCombat; }
                    if (_heal == null) { _heal = Spec.BeastmasterHunter.CreateBMHCombat; }
                    if (_buffs == null) { _buffs = Spec.BeastmasterHunter.CreateBMHBuffs; }
                    break;
                case WoWSpec.HunterMarksmanship:
                    if (_combat == null) { _combat = Spec.MarksmanshipHunter.CreateMHCombat; }
                    if (_pull == null) { _pull = Spec.MarksmanshipHunter.CreateMHCombat; }
                    if (_heal == null) { _heal = Spec.MarksmanshipHunter.CreateMHCombat; }
                    if (_buffs == null) { _buffs = Spec.MarksmanshipHunter.CreateMHKBuffs; }
                    break;
                case WoWSpec.HunterSurvival:
                    if (_combat == null) { _combat = Spec.SurvivalHunter.CreateSHCombat; }
                    if (_pull == null) { _pull = Spec.SurvivalHunter.CreateSHCombat; }
                    if (_heal == null) { _heal = Spec.SurvivalHunter.CreateSHCombat; }
                    if (_buffs == null) { _buffs = Spec.SurvivalHunter.CreateSHBuffs; }
                    break;
                case WoWSpec.MageArcane:
                    if (_combat == null) { _combat = Spec.ArcaneMage.CreateAMCombat; }
                    if (_pull == null) { _pull = Spec.ArcaneMage.CreateAMCombat; }
                    if (_heal == null) { _heal = Spec.ArcaneMage.CreateAMCombat; }
                    if (_buffs == null) { _buffs = Spec.ArcaneMage.CreateAMBuffs; }
                    break;
                case WoWSpec.MageFire:
                    if (_combat == null) { _combat = Spec.FireMage.CreateFiMCombat; }
                    if (_pull == null) { _pull = Spec.FireMage.CreateFiMCombat; }
                    if (_heal == null) { _heal = Spec.FireMage.CreateFiMCombat; }
                    if (_buffs == null) { _buffs = Spec.FireMage.CreateFiMBuffs; }
                    break;
                case WoWSpec.MageFrost:
                    if (_combat == null) { _combat = Spec.FrostMage.CreateFMCombat; }
                    if (_pull == null) { _pull = Spec.FrostMage.CreateFMCombat; }
                    if (_heal == null) { _heal = Spec.FrostMage.CreateFMCombat; }
                    if (_buffs == null) { _buffs = Spec.FrostMage.CreateFMBuffs; }
                    break;
                case WoWSpec.MonkBrewmaster:
                    if (_combat == null) { _combat = Spec.BrewmasterMonk.CreateBMCombat; }
                    if (_pull == null) { _pull = Spec.BrewmasterMonk.CreateBMCombat; }
                    if (_heal == null) { _heal = Spec.BrewmasterMonk.CreateBMCombat; }
                    if (_buffs == null) { _buffs = Spec.BrewmasterMonk.CreateBMBuffs; }
                    break;
                case WoWSpec.MonkMistweaver:
                    if (_combat == null) { _combat = Spec.MistweaverMonk.CreateMMCombat; }
                    if (_pull == null) { _pull = Spec.MistweaverMonk.CreateMMCombat; }
                    if (_heal == null) { _heal = Spec.MistweaverMonk.CreateMMCombat; }
                    if (_buffs == null) { _buffs = Spec.MistweaverMonk.CreateMMBuffs; }
                    break;
                case WoWSpec.MonkWindwalker:
                    if (_combat == null) { _combat = Spec.WindwalkerMonk.CreateWMCombat; }
                    if (_pull == null) { _pull = Spec.WindwalkerMonk.CreateWMCombat; }
                    if (_heal == null) { _heal = Spec.WindwalkerMonk.CreateWMCombat; }
                    if (_buffs == null) { _buffs = Spec.WindwalkerMonk.CreateWMBuffs; }
                    break;
                case WoWSpec.PaladinHoly:
                    if (_combat == null) { _combat = Spec.HolyPaladin.CreateHPaCombat; }
                    if (_pull == null) { _pull = Spec.HolyPaladin.CreateHPaCombat; }
                    if (_heal == null) { _heal = Spec.HolyPaladin.CreateHPaCombat; }
                    if (_buffs == null) { _buffs = Spec.HolyPaladin.CreateHPaBuffs; }
                    break;
                case WoWSpec.PaladinProtection:
                    if (_combat == null) { _combat = Spec.ProtectionPaladin.CreatePPCombat; }
                    if (_pull == null) { _pull = Spec.ProtectionPaladin.CreatePPCombat; }
                    if (_heal == null) { _heal = Spec.ProtectionPaladin.CreatePPCombat; }
                    if (_buffs == null) { _buffs = Spec.ProtectionPaladin.CreatePPBuffs; }
                    break;
                case WoWSpec.PaladinRetribution:
                    if (_combat == null) { _combat = Spec.RetributionPaladin.CreateRPCombat; }
                    if (_pull == null) { _pull = Spec.RetributionPaladin.CreateRPCombat; }
                    if (_heal == null) { _heal = Spec.RetributionPaladin.CreateRPCombat; }
                    if (_buffs == null) { _buffs = Spec.RetributionPaladin.CreateRPBuffs; }
                    break;
                case WoWSpec.PriestDiscipline:
                    if (_combat == null) { _combat = Spec.DisciplinePriest.CreateDPCombat; }
                    if (_pull == null) { _pull = Spec.DisciplinePriest.CreateDPCombat; }
                    if (_heal == null) { _heal = Spec.DisciplinePriest.CreateDPCombat; }
                    if (_buffs == null) { _buffs = Spec.DisciplinePriest.CreateDPBuffs; }
                    break;
                case WoWSpec.PriestHoly:
                    if (_combat == null) { _combat = Spec.HolyPriest.CreateHPCombat; }
                    if (_pull == null) { _pull = Spec.HolyPriest.CreateHPCombat; }
                    if (_heal == null) { _heal = Spec.HolyPriest.CreateHPCombat; }
                    if (_buffs == null) { _buffs = Spec.HolyPriest.CreateHPBuffs; }
                    break;
                case WoWSpec.PriestShadow:
                    if (_combat == null) { _combat = Spec.ShadowPriest.CreateSPCombat; }
                    if (_pull == null) { _pull = Spec.ShadowPriest.CreateSPCombat; }
                    if (_heal == null) { _heal = Spec.ShadowPriest.CreateSPCombat; }
                    if (_buffs == null) { _buffs = Spec.ShadowPriest.CreateSPBuffs; }
                    break;
                case WoWSpec.RogueAssassination:
                    if (_combat == null) { _combat = Spec.AssassinationRogue.CreateARCombat; }
                    if (_pull == null) { _pull = Spec.AssassinationRogue.CreateARCombat; }
                    if (_heal == null) { _heal = Spec.AssassinationRogue.CreateARCombat; }
                    if (_buffs == null) { _buffs = Spec.AssassinationRogue.CreateARBuffs; }
                    break;
                case WoWSpec.RogueCombat:
                    if (_combat == null) { _combat = Spec.CombatRogue.CreateCRCombat; }
                    if (_pull == null) { _pull = Spec.CombatRogue.CreateCRCombat; }
                    if (_heal == null) { _heal = Spec.CombatRogue.CreateCRCombat; }
                    if (_buffs == null) { _buffs = Spec.CombatRogue.CreateCRBuffs; }
                    break;
                case WoWSpec.RogueSubtlety:
                    if (_combat == null) { _combat = Spec.SubtletyRogue.CreateSRCombat; }
                    if (_pull == null) { _pull = Spec.SubtletyRogue.CreateSRCombat; }
                    if (_heal == null) { _heal = Spec.SubtletyRogue.CreateSRCombat; }
                    if (_buffs == null) { _buffs = Spec.SubtletyRogue.CreateSRBuffs; }
                    break;
                case WoWSpec.ShamanElemental:
                    if (_combat == null) { _combat = Spec.ElementalShaman.CreateElSCombat; }
                    if (_pull == null) { _pull = Spec.ElementalShaman.CreateElSCombat; }
                    if (_heal == null) { _heal = Spec.ElementalShaman.CreateElSCombat; }
                    if (_buffs == null) { _buffs = Spec.ElementalShaman.CreateElSBuffs; }
                    break;
                case WoWSpec.ShamanEnhancement:
                    if (_combat == null) { _combat = Spec.EnhancementShaman.CreateESCombat; }
                    if (_pull == null) { _pull = Spec.EnhancementShaman.CreateESCombat; }
                    if (_heal == null) { _heal = Spec.EnhancementShaman.CreateESCombat; }
                    if (_buffs == null) { _buffs = Spec.EnhancementShaman.CreateESBuffs; }
                    break;
                case WoWSpec.ShamanRestoration:
                    if (_combat == null) { _combat = Spec.RestorationShaman.CreateRSCombat; }
                    if (_pull == null) { _pull = Spec.RestorationShaman.CreateRSCombat; }
                    if (_heal == null) { _heal = Spec.RestorationShaman.CreateRSCombat; }
                    if (_buffs == null) { _buffs = Spec.RestorationShaman.CreateRSBuffs; }
                    break;
                case WoWSpec.WarlockAffliction:
                    if (_combat == null) { _combat = Spec.AfflictionWarlock.CreateAWCombat; }
                    if (_pull == null) { _pull = Spec.AfflictionWarlock.CreateAWCombat; }
                    if (_heal == null) { _heal = Spec.AfflictionWarlock.CreateAWCombat; }
                    if (_buffs == null) { _buffs = Spec.AfflictionWarlock.CreateAWBuffs; }
                    break;
                case WoWSpec.WarlockDemonology:
                    if (_combat == null) { _combat = Spec.DemonologyWarlock.CreateDemWCombat; }
                    if (_pull == null) { _pull = Spec.DemonologyWarlock.CreateDemWCombat; }
                    if (_heal == null) { _heal = Spec.DemonologyWarlock.CreateDemWCombat; }
                    if (_buffs == null) { _buffs = Spec.DemonologyWarlock.CreateDemWBuffs; }
                    break;
                case WoWSpec.WarlockDestruction:
                    if (_combat == null) { _combat = Spec.DestructionWarlock.CreateDWCombat; }
                    if (_pull == null) { _pull = Spec.DestructionWarlock.CreateDWCombat; }
                    if (_heal == null) { _heal = Spec.DestructionWarlock.CreateDWCombat; }
                    if (_buffs == null) { _buffs = Spec.DestructionWarlock.CreateDWBuffs; }
                    break;
                case WoWSpec.WarriorArms:
                    if (_combat == null) { _combat = Spec.ArmsWarrior.CreateAWCombat; }
                    if (_pull == null) { _pull = Spec.ArmsWarrior.CreateAWCombat; }
                    if (_heal == null) { _heal = Spec.ArmsWarrior.CreateAWCombat; }
                    if (_buffs == null) { _buffs = Spec.ArmsWarrior.CreateAWBuffs; }
                    break;
                case WoWSpec.WarriorFury:
                    if (_combat == null) { _combat = Spec.FuryWarrior.CreateFWCombat; }
                    if (_pull == null) { _pull = Spec.FuryWarrior.CreateFWCombat; }
                    if (_heal == null) { _heal = Spec.FuryWarrior.CreateFWCombat; }
                    if (_buffs == null) { _buffs = Spec.FuryWarrior.CreateFWBuffs; }
                    break;
                case WoWSpec.WarriorProtection:
                    if (_combat == null) { _combat = Spec.ProtectionWarrior.CreatePWCombat; }
                    if (_pull == null) { _pull = Spec.ProtectionWarrior.CreatePWCombat; }
                    if (_heal == null) { _heal = Spec.ProtectionWarrior.CreatePWCombat; }
                    if (_buffs == null) { _buffs = Spec.ProtectionWarrior.CreatePWBuffs; }
                    break;
            }
        }
        #endregion

        private void RebuildBehaviors()
        {
            try
            {
                Logging.WriteDiagnostic("RebuildBehaviors called.");

                //_currentRotation = null; // clear current rotation
                _combat = null;
                _buffs = null;
                _pull = null;
                _heal = null;

                CombatandBuffSelection();
                //SetRotation(); // set the new rotation
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic("[RebuildBehaviors] Exception was thrown: {0}", ex);
            }
        }
    }
}
