using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Helpers
{
    internal static class LuaCore
    {
        internal static SecondaryStats _secondaryStats;          //create within frame (does series of LUA calls)

        //-- Put all Lua Calls in here..For other Lua Calls that need to be used elsewhere put Styx.WoWInternals in front of it -- wulf

        internal static Composite StartAutoAttack
        {
            get
            {
                return new Action(ret =>
                {
                    if (!StyxWoW.Me.IsAutoAttacking)
                        Lua.DoString("StartAttack()");
                    return RunStatus.Failure;
                });
            }
        }

        public static double GetSpellCooldown(string spell)
        {
            try
            {
                SpellFindResults results;
                if (SpellManager.FindSpell(spell, out results))
                {
                    var conv = results.Override != null ? results.Override.Name : results.Original.Name;
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return GetSpellCooldown(\"" + conv + "\");", 1);
                    }
                }

                return 0;
            }
            catch
            {
                Logging.WriteDiagnostic(" Lua Failed in GetSpellCooldown"); 
                return 0;
            }
        } // may not even need to check for the existance of the spell ? --wulf

        public static double GetSpellCooldown(int spell)
        {
            try
            {
                SpellFindResults results;
                if (SpellManager.FindSpell(spell, out results))
                {
                    var conv = results.Override != null ? results.Override.Name : results.Original.Name;
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return GetSpellCooldown(\"" + conv + "\");", 1);
                    }
                }

                return 0;
            }
            catch
            {
                Logging.WriteDiagnostic(" Lua Failed in GetSpellCooldown"); 
                return 0;
            }
        } // may not even need to check for the existance of the spell ? --wulf

        public static double GetRuneCooldown(int runeslot)
        {
            try
            {
                using (StyxWoW.Memory.AcquireFrame())
                {
                    var lua = String.Format("local x=select(1, GetRuneCooldown({0})); if x==nil then return 0 else return x-GetTime() end", runeslot);
                    var t = Double.Parse(Lua.GetReturnValues(lua)[0]);
                    return Math.Abs(t);
                }
            }
            catch
            {
                Logging.WriteDiagnostic(" Lua Failed in GetRuneCooldown"); 
                return 0;
            }
        }

        #region Player and Target Debuffs and Buffs (Damn you alex for making me put these in :P -- wulf)

        public static double PlayerBuffTimeLeft(string name)
        {
            name = LocalizeSpellName(name);
            try
            {
                var lua = String.Format("local x=select(7, UnitBuff('player', \"{0}\", nil, 'PLAYER')); if x==nil then return 0 else return x-GetTime() end", RealLuaEscape(name));
                var t = Double.Parse(Lua.GetReturnValues(lua)[0]);
                return t;
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in PlayerBuffTimeLeft");
                return 999999;
            }
        }

        public static int PlayerCountBuff(string name)
        {
            name = LocalizeSpellName(name);
            try
            {
                var lua = string.Format("local x=select(4, UnitBuff('player', \"{0}\")); if x==nil then return 0 else return x end", RealLuaEscape(name));
                var t = int.Parse(Lua.GetReturnValues(lua)[0]);
                return t;
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in PlayerCountBuff");
                return 0;
            }
        }

        public static double TargetDebuffTimeLeft(string name)
        {
            name = LocalizeSpellName(name);
            try
            {
                var lua = string.Format("local x=select(7, UnitDebuff(\"target\", \"{0}\", nil, 'PLAYER')); if x==nil then return 0 else return x-GetTime() end", RealLuaEscape(name));
                var t = double.Parse(Lua.GetReturnValues(lua)[0]);
                return t;
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in TargetDebuffTimeLeft");
                return 999999;
            }
        }

        public static int TargetCountDebuff(string name)
        {
            name = LocalizeSpellName(name);
            try
            {
                var lua = string.Format("local x=select(4, UnitDebuff('target', \"{0}\", nil, 'PLAYER')); if x==nil then return 0 else return x end", RealLuaEscape(name));
                var t = int.Parse(Lua.GetReturnValues(lua)[0]);
                return t;
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in TargetCountDebuff");
                return 0;
            }
        }

        public static int TargetCountBuff(string name)
        {
            name = LocalizeSpellName(name);
            try
            {
                var lua = string.Format("local x=select(4, UnitBuff('target', \"{0}\", nil, 'PLAYER')); if x==nil then return 0 else return x end", RealLuaEscape(name));
                var t = int.Parse(Lua.GetReturnValues(lua)[0]);
                return t;
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in TargetCountBuff");
                return 0;
            }
        }

        #endregion Player and Target Debuffs and Buffs (Damn you alex for making me put these in :P -- wulf)

        #region Misc Lua Helpers

        public static string RealLuaEscape(string luastring)
        {
            var bytes = Encoding.UTF8.GetBytes(luastring);
            return bytes.Aggregate(String.Empty, (current, b) => current + ("\\" + b));
        }

        public static Composite RunMacroText(string macro, CanRunDecoratorDelegate cond)
        {
            return new Decorator(
                       cond,

                //new PrioritySelector(
                       new Sequence(
                           new Action(a => Lua.DoString("RunMacroText(\"" + RealLuaEscape(macro) + "\")")),
                           new Action(a => Logging.WriteDiagnostic("Running Macro Text: {0}", macro))
                               )
                           );
        }

        public static Composite CancelMyAura(string name, CanRunDecoratorDelegate cond)
        {
            name = LocalizeSpellName(name);
            var macro = String.Format("/cancelaura {0}", name);
            return new Decorator(
                delegate(object a)
                {
                    if (name.Length == 0)
                        return false;

                    if (!cond(a))
                        return false;

                    return true;
                },
                new Sequence(
                    new Action(a => Lua.DoString("RunMacroText(\"" + RealLuaEscape(macro) + "\")"))));
        }

        /// <summary>
        /// this will localise the spell name to the local client.
        /// </summary>
        private static readonly Dictionary<string, string> LocalizedSpellNames = new Dictionary<string, string>();

        public static string LocalizeSpellName(string name)
        {
            if (LocalizedSpellNames.ContainsKey(name))
                return LocalizedSpellNames[name];

            string loc;

            int id = 0;
            try
            {
                id = SpellManager.Spells[name].Id;
            }
            catch
            {
                return name;
            }

            try
            {
                loc = Lua.GetReturnValues("return select(1, GetSpellInfo(" + id + "))")[0];
            }
            catch
            {
                Logging.WriteDiagnostic("Lua failed in LocalizeSpellName");
                return name;
            }

            LocalizedSpellNames[name] = loc;
            Logging.WriteDiagnostic("Localized spell: '" + name + "' is '" + loc + "'.");
            return loc;
        }

        /// <summary>
        /// Returns the icon name for an ability, i.e. "interface\icons\spell_fel_elementaldevastation"
        /// </summary>
        /// <param name="spellId">ID of spell</param>
        /// <returns>Ability's Icon Label IN LOWER CASE</returns>
        public static string GetSpellIconText(int spellId)
        {
            var vals = Lua.GetReturnValues("return select(3, GetSpellInfo(" + spellId + "))")[0];
            return vals.ToLower();
        }

        #endregion Misc Lua Helpers

        #region Energy Calls

        public static double PlayerPower
        {
            get
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return UnitPower(\"player\");", 0);
                    }
                }
                catch { Logging.WriteDiagnostic(" Lua Failed in PlayerPower"); return StyxWoW.Me.CurrentPower; }
            }
        }

        public static double PlayerPowerMax
        {
            get
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return UnitPowerMax(\"player\",1);", 0);
                    }
                }
                catch { Logging.WriteDiagnostic(" Lua Failed in PlayerPowerMax"); return StyxWoW.Me.MaxPower; }
            }
        }


        public static double PlayerChi
        {
            get
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return UnitPower(\"player\");", 12);
                    }
                }
                catch { Logging.WriteDiagnostic(" Lua Failed in PlayerChi"); return StyxWoW.Me.CurrentChi; }
            }
        }

        //Return the Chi for Monk's

        /// <summary>
        /// Not sure if this is the one you need for DK's but it works for druids cat form
        /// </summary>
        private static double PlayerEnergy
        {
            get
            {
                try
                {
                    return Lua.GetReturnVal<int>("return UnitMana(\"player\");", 0);
                }
                catch
                {
                    Logging.WriteDiagnostic(" Lua Failed in PlayerEnergy");
                    return StyxWoW.Me.CurrentMana;
                }
            }
        }

        public static double PlayerComboPts
        {
            get
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return GetComboPoints(\"player\");", 0);
                    }
                }
                catch
                {
                    Logging.WriteDiagnostic(" Lua Failed in PlayerComboPts");
                    return 0;
                }
            }
        }

        public static double SpellPower
        {
            get
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<float>("return math.max(GetSpellBonusDamage(1),GetSpellBonusDamage(2),GetSpellBonusDamage(3),GetSpellBonusDamage(4),GetSpellBonusDamage(5),GetSpellBonusDamage(6),GetSpellBonusDamage(7))", 0);
                    }
                }
                catch
                {
                    Logging.Write("Lua Failed in SpellPower");
                    return 0;
                }
            }
        }

        public static int RuneType(uint IdNumber)
        {
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return Lua.GetReturnVal<int>("return GetRuneType(\"player\");", IdNumber);
                    }
                }
                catch
                {
                    Logging.WriteDiagnostic(" Lua Failed in Runetypes");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Returns a unit's current level of mana, rage, energy or other power type. Returns zero for non-existent units.
        /// </summary>

        public static int PlayerUnitPower(string powerType)
        {
            try
            {
                var myval = Lua.GetReturnVal<int>(String.Format("return UnitPower(\"player\", {0})", powerType), 0);

                //Logger.InfoLog("Demonic Power = {0}", myval);
                return myval;
            }
            catch
            {
                Logging.WriteDiagnostic(" Lua Failed in EVERYTHING");
                return 0;
            }
        }

        /// <summary>
        /// Returns information about the player's mana/energy/etc regeneration rate
        /// </summary>

        /// <summary>
        /// Calculate time to energy cap.
        /// </summary>
        public static double TimeToEnergyCap()
        {
            double timetoEnergyCap;

            double playerEnergy;

            double ER_Rate;

            playerEnergy = Lua.GetReturnVal<int>("return UnitMana(\"player\");", 0); // current Energy

            ER_Rate = EnergyRegen();
            timetoEnergyCap = (100 - playerEnergy) * (1.0 / ER_Rate); // math

            return timetoEnergyCap;
        }

        public static double EnergyRegen()
        {
            double energyRegen;

            energyRegen = Lua.GetReturnVal<float>("return GetPowerRegen()", 1); // rate of energy regen

            return energyRegen;
        }


        #endregion Energy Calls


        #region SecondryStats - Credit: Singular

        internal static void PopulateSecondryStats()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                _secondaryStats = new SecondaryStats();
            }

            // Haste Rating Required Per 1%
            // Level 60	 Level 70	 Level 80	 Level 85	 Level 90
            //   10	      15.77	      32.79	      128.125	 425.19

            Logging.WriteDiagnostic("");
            Logging.WriteDiagnostic("Health: {0}", StyxWoW.Me.MaxHealth);
            Logging.WriteDiagnostic("Agility: {0}", StyxWoW.Me.Agility);
            Logging.WriteDiagnostic("Intellect: {0}", StyxWoW.Me.Intellect);
            Logging.WriteDiagnostic("Spirit: {0}", StyxWoW.Me.Spirit);
            Logging.WriteDiagnostic("");
            Logging.WriteDiagnostic("Attack Power: {0}", _secondaryStats.AttackPower);
            Logging.WriteDiagnostic("Power: {0:F2}", _secondaryStats.Power);
            Logging.WriteDiagnostic("Hit(M/R): {0}/{1}", _secondaryStats.MeleeHit, _secondaryStats.SpellHit);
            Logging.WriteDiagnostic("Expertise: {0}", _secondaryStats.Expertise);
            Logging.WriteDiagnostic("Mastery: {0:F2}", _secondaryStats.Mastery);
            Logging.WriteDiagnostic("Mastery (CR): {0:F2}", _secondaryStats.MasteryCR);
            Logging.WriteDiagnostic("Crit: {0:F2}", _secondaryStats.Crit);
            Logging.WriteDiagnostic("Haste(M/R): {0} (+{1} % Haste) / {2} (+{3} % Haste)", _secondaryStats.MeleeHaste, Math.Round(_secondaryStats.MeleeHaste / 425.19, 2), _secondaryStats.SpellHaste, Math.Round(_secondaryStats.SpellHaste / 425.19, 2));
            Logging.WriteDiagnostic("SpellPen: {0}", _secondaryStats.SpellPen);
            Logging.WriteDiagnostic("PvP Resil: {0}", _secondaryStats.Resilience);
            Logging.WriteDiagnostic("PvP Power: {0}", _secondaryStats.PvpPower);
            Logging.WriteDiagnostic("Spell Power: {0}", _secondaryStats.SpellPower);
            Logging.WriteDiagnostic("");
        }

        internal class SecondaryStats
        {
            public float MeleeHit { get; set; }

            public float SpellHit { get; set; }

            public float Expertise { get; set; }

            public float MeleeHaste { get; set; }

            public float SpellHaste { get; set; }

            public float SpellPen { get; set; }

            public float Mastery { get; set; }

            public float MasteryCR { get; set; }

            public float Crit { get; set; }

            public float Resilience { get; set; }

            public float PvpPower { get; set; }

            public float AttackPower { get; set; }

            public float Power { get; set; }

            public float Intellect { get; set; }

            public float SpellPower { get; set; }

            public SecondaryStats()
            {
                Refresh();
            }

            public void Refresh()
            {
                try
                {
                    MeleeHit = Lua.GetReturnVal<float>("return GetCombatRating(CR_HIT_MELEE)", 0);
                    SpellHit = Lua.GetReturnVal<float>("return GetCombatRating(CR_HIT_SPELL)", 0);
                    Expertise = StyxWoW.Me.Expertise;
                    MeleeHaste = Lua.GetReturnVal<float>("return GetCombatRating(CR_HASTE_MELEE)", 0);
                    SpellHaste = Lua.GetReturnVal<float>("return GetCombatRating(CR_HASTE_SPELL)", 0);
                    SpellPen = Lua.GetReturnVal<float>("return GetSpellPenetration()", 0);
                    Mastery = StyxWoW.Me.Mastery;
                    MasteryCR = Lua.GetReturnVal<float>("return GetCombatRating(CR_MASTERY)", 0);
                    Crit = StyxWoW.Me.CritPercent;
                    Resilience = Lua.GetReturnVal<float>("return GetCombatRating(COMBAT_RATING_RESILIENCE_CRIT_TAKEN)", 0);
                    PvpPower = Lua.GetReturnVal<float>("return GetCombatRating(CR_PVP_POWER)", 0);
                    AttackPower = StyxWoW.Me.AttackPower;
                    Power = Lua.GetReturnVal<float>("return select(7,UnitDamage(\"player\"))", 0);
                    Intellect = StyxWoW.Me.Intellect;
                    SpellPower = Lua.GetReturnVal<float>("return math.max(GetSpellBonusDamage(1),GetSpellBonusDamage(2),GetSpellBonusDamage(3),GetSpellBonusDamage(4),GetSpellBonusDamage(5),GetSpellBonusDamage(6),GetSpellBonusDamage(7))", 0);
                }
                catch 
                {
                    Logging.WriteDiagnostic(" Lua Failed in SecondaryStats");
                }
              
            }
        }

        #endregion SecondryStats - Credit: Singular
    }
}