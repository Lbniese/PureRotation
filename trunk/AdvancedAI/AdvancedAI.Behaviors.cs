using System;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using Styx;
using Styx.Common;
using Styx.TreeSharp;

namespace AdvancedAI
{
    partial class AdvancedAI
    {
        private Composite _combat, _preCombatBuffs, _pull, _heal;
        public override Composite PreCombatBuffBehavior { get { return _preCombatBuffs; } }
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PullBehavior { get { return _pull; } }
        public override Composite HealBehavior { get { return _heal; } }
        
        public void AssignBehaviors()
        {
            //Set all to null
            _preCombatBuffs = null;
            _combat = null;
            _heal = null;
            _pull = null;

            WoWContext context = CurrentWoWContext;

            SetRotation();

            EnsureComposite(true, context, BehaviorType.Combat);
            EnsureComposite(false, context, BehaviorType.Heal);
            EnsureComposite(false, context, BehaviorType.PreCombatBuffs);
            EnsureComposite(false, context, BehaviorType.Pull);
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

            if (_pull == null)
            {
                Logging.Write("Initializing Pulling");
                _pull = new LockSelector(
                    new HookExecutor(HookName(BehaviorType.Pull)));
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
