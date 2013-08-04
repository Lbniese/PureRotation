using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAI.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals.DBC;

namespace AdvancedAI
{
    #region Nested type: LocationContextEventArg
    public class WoWContextEventArg : EventArgs
    {
        public readonly WoWContext CurrentWoWContext;
        public readonly WoWContext PreviousWoWContext;

        public WoWContextEventArg(WoWContext currentWoWContext, WoWContext prevWoWContext)
        {
            CurrentWoWContext = currentWoWContext;
            PreviousWoWContext = prevWoWContext;
        }
    }
    #endregion Nested type: LocationContextEventArg

    partial class AdvancedAI
    {
        private bool _contextEventSubscribed;

        private static WoWContext LastWoWContext { get; set; }

        private static GroupType GroupType
        {
            get
            {
                if (Me.GroupInfo.IsInRaid)
                {
                    return GroupType.Raid;
                }

                return Me.GroupInfo.IsInParty ? GroupType.Party : GroupType.Solo;
            }
        }

        internal static WoWContext CurrentWoWContext
        {
            get
            {
                if (!StyxWoW.IsInGame)
                    return WoWContext.None;

                Map map = Me.CurrentMap;

                if (map.IsBattleground || map.IsArena)
                {
                    return WoWContext.Battlegrounds;
                }

                if (Me.IsInGroup())
                {
                    if (Me.IsInInstance || map.IsDungeon || map.IsRaid || map.IsScenario)
                    {
                        return WoWContext.Instances;
                    }
                }

                return WoWContext.Normal;
            }
        }

        internal static event EventHandler<WoWContextEventArg> OnWoWContextChanged;

        private void UpdateContext()
        {
            // Subscribe to the map change event, so we can automatically update the context.
            if (!_contextEventSubscribed)
            {
                // Subscribe to OnBattlegroundEntered. Just 'cause.
                BotEvents.Battleground.OnBattlegroundEntered += e => UpdateContext();
                _contextEventSubscribed = true;
            }

            var current = CurrentWoWContext;

            // Can't update the context when it doesn't exist.
            if (current == WoWContext.None)
                return;

            if (current != LastWoWContext && OnWoWContextChanged != null)
            {
                try
                {
                    OnWoWContextChanged(this, new WoWContextEventArg(current, LastWoWContext));
                }
                catch
                {
                    // Eat any exceptions thrown.
                }
                LastWoWContext = current;
            }
        }
    }
}
