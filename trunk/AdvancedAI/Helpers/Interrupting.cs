using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Styx.Common;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Helpers
{
    class Interrupting
    {
        [UsedImplicitly]
        public class UnitSpellcastingInfo
        {
            private const string UnitID = "";
            public static WoWUnit Unit = null;
            public static bool UnitExists = false;
            public readonly bool Included;

            private static string sUnitGUID = "";
            private static ulong ulUnitGUID = 0;
            public static bool IsUnitEnemy = false;

            public static bool IsCasting = false;
            public static bool IsChannelling = false;
            private static string CastName = "";
            private static int SpellID = 0;
            public static bool Interruptible = false;

            private static float StartTime = 0;
            private static float EndTime = 0;

            public static float msCastTimeLeft
            {
                get
                {
                    return
                        EndTime > 0 ?
                        EndTime - Lua.GetReturnVal<float>("return GetTime() * 1000", 0)
                        : 99999;
                }
            }
            public float secCastTimeLeft
            {
                get
                {
                    return
                        EndTime / 1000 > 0 ?
                        EndTime - Lua.GetReturnVal<float>("return GetTime()", 0)
                        : 99999;
                }
            }
            public static float msCastTimeElapsed
            {
                get
                {
                    return
                        StartTime > 0 ?
                        Lua.GetReturnVal<float>("return GetTime() * 1000", 0) - StartTime
                        : 0;
                }
            }
            public float secCastlTimeElapsed
            {
                get
                {
                    return
                        StartTime / 1000 > 0 ?
                        Lua.GetReturnVal<float>("return GetTime()", 0) - StartTime
                        : 0;
                }
            }
            
            private static float _totalCastTime = 0;

            public UnitSpellcastingInfo(bool included)
            {
                Included = included;
            }

            public static void Update()
            {
                Logging.WriteDiagnostic("Updating: " + UnitID);
                UnitExists = Lua.GetReturnVal<bool>("return UnitExists('" + UnitID + "')", 0);
                Logging.WriteDiagnostic(UnitExists.ToString());
                if (UnitExists)
                {
                    Logging.WriteDiagnostic("unit exists");
                    sUnitGUID = Lua.GetReturnVal<string>(
                        String.Format("return UnitGUID(\"{0}\")", UnitID), 0);
                    IsUnitEnemy = Lua.GetReturnVal<bool>(
                        String.Format("return UnitCanAttack(\"player\", \"{0}\")", UnitID), 0);

                    string str = sUnitGUID.Replace("0x", "");
                    ulUnitGUID = UInt64.Parse(str, NumberStyles.HexNumber);

                    Unit = ObjectManager.GetAnyObjectByGuid<WoWUnit>(ulUnitGUID);

                    List<string> unitCastingInfo = Lua.GetReturnValues(
                                                    String.Format("return UnitCastingInfo(\"{0}\")", UnitID));

                    if (unitCastingInfo != null)
                    {
                        Logging.WriteDiagnostic("casting detected");
                        CastName = unitCastingInfo[0];
                        IsCasting = true;
                        IsChannelling = false;
                        SpellID = Unit.CastingSpellId;

                        Interruptible = !unitCastingInfo[8].ToBoolean();

                        StartTime = unitCastingInfo[4].ToFloat();
                        EndTime = unitCastingInfo[5].ToFloat();
                        _totalCastTime = EndTime - StartTime;
                        return;
                    }

                    unitCastingInfo = Lua.GetReturnValues(String.Format("return UnitChannelInfo(\"{0}\")", UnitID));

                    if (unitCastingInfo != null)
                    {
                        Logging.WriteDiagnostic("channeling detected");
                        CastName = unitCastingInfo[0];
                        IsCasting = false;
                        IsChannelling = true;
                        SpellID = Unit.CastingSpellId;
                        
                        Interruptible = !unitCastingInfo[7].ToBoolean();
                        
                        StartTime = unitCastingInfo[4].ToFloat();
                        EndTime = unitCastingInfo[5].ToFloat();
                        _totalCastTime = EndTime - StartTime;
                        return;
                    }
                }
                Reset();
            }

            private static void Reset()
            {
                sUnitGUID = "0";
                ulUnitGUID = 0;
                Unit = null;
                
                IsCasting = false;
                CastName = "";
                IsChannelling = false;
                Interruptible = false;
                
                StartTime = -1;
                EndTime = -1;
                _totalCastTime = -1;
            }
        }

        private static class SpellcastTracker
        {
            public static readonly Dictionary<string, UnitSpellcastingInfo> Targets = new Dictionary<string, UnitSpellcastingInfo>();
        }

        //private readonly Interrupting.SpellcastTracker SpellTracker = new Interrupting.SpellcastTracker();



        public static WoWUnit GetSpellcastingUnit
        {
            get
            {
                var result = (from UnitSpellcastingInfo sc in SpellcastTracker.Targets.Values.Where(v =>
                        UnitSpellcastingInfo.UnitExists &&
                        UnitSpellcastingInfo.Unit != null &&
                        v.Included && UnitSpellcastingInfo.IsUnitEnemy &&
                        ((UnitSpellcastingInfo.IsCasting && UnitSpellcastingInfo.msCastTimeLeft < 500) ||
                            (UnitSpellcastingInfo.IsChannelling && UnitSpellcastingInfo.msCastTimeElapsed > 500)) &&
                        UnitSpellcastingInfo.Interruptible)
                              where UnitSpellcastingInfo.Unit.IsValid
                              orderby UnitSpellcastingInfo.Unit.DistanceSqr ascending
                              select UnitSpellcastingInfo.Unit);

                var woWUnits = result as WoWUnit[] ?? result.ToArray();
                return woWUnits.Any() ? woWUnits.FirstOrDefault() : null;
            }
        }
    }
}
