using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using AdvancedAI.Settings;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.POI;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.GUI
{
    public partial class AdvancedAIGUI : Form
    {
        public AdvancedAIGUI()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK || DialogResult == DialogResult.Yes)
            {
                Logging.WriteDiagnostic("Settings saved, rebuilding behaviors...");
                AdvancedAI.Instance.AssignBehaviors();
                GeneralSettings.Instance.LogSettings();
            }
            base.OnClosing(e);
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {

            //HealTargeting.Instance.OnTargetListUpdateFinished += new Styx.Logic.TargetListUpdateFinishedDelegate(Instance_OnTargetListUpdateFinished);
            propertyGrid3.SelectedObject = GeneralSettings.Instance;

            Styx.Helpers.Settings toSelect = null;
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Warrior:
                    toSelect = GeneralSettings.Instance.Warrior();
                    break;
                case WoWClass.Paladin:
                    toSelect = GeneralSettings.Instance.Paladin();
                    break;
                case WoWClass.Hunter:
                    toSelect = GeneralSettings.Instance.Hunter();
                    break;
                case WoWClass.Rogue:
                    toSelect = GeneralSettings.Instance.Rogue();
                    break;
                case WoWClass.Priest:
                    toSelect = GeneralSettings.Instance.Priest();
                    break;
                case WoWClass.DeathKnight:
                    toSelect = GeneralSettings.Instance.DeathKnight();
                    break;
                case WoWClass.Shaman:
                    toSelect = GeneralSettings.Instance.Shaman();
                    break;
                case WoWClass.Mage:
                    toSelect = GeneralSettings.Instance.Mage();
                    break;
                case WoWClass.Warlock:
                    toSelect = GeneralSettings.Instance.Warlock();
                    break;
                case WoWClass.Druid:
                    toSelect = GeneralSettings.Instance.Druid();
                    break;
                case WoWClass.Monk:
                    toSelect = GeneralSettings.Instance.Monk();
                    break;
                default:
                    break;
            }

            if (toSelect != null)
            {
                propertyGrid2.SelectedObject = toSelect;
            }

            propertyGrid1.SelectedObject = GeneralSettings.Instance.Hotkeys();

            // chkDebugLogging.Checked = SingularSettings.Instance.EnableDebugLogging;
            // chkDebugSpellCanCast.Checked = SingularSettings.Instance.EnableDebugLoggingCanCast;

            if (!timer1.Enabled)
                timer1.Start();

            Screen screen = Screen.FromHandle(this.Handle);
            if (this.Left.Between(0, screen.WorkingArea.Width) && this.Top.Between(0, screen.WorkingArea.Height))
            {
                int height = screen.WorkingArea.Height - this.Top;
                if (height > 200)
                {
                    this.Height = height;
                }
            }

            tabControl1_SelectedIndexChanged(this, new EventArgs());
        }

        public static void SetLabelColumnWidth(PropertyGrid grid, int width)
        {
            if (grid == null)
                return;

            FieldInfo fi = grid.GetType().GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                return;

            Control view = fi.GetValue(grid) as Control;
            if (view == null)
                return;

            MethodInfo mi = view.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                return;
            mi.Invoke(view, new object[] { width });
        }

        private void saveExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // deal with Debug tab controls individually
                // SingularSettings.Instance.EnableDebugLogging = chkDebugLogging.Checked;
                // SingularSettings.Instance.EnableDebugLoggingCanCast = chkDebugSpellCanCast.Checked;


                // save property group settings from each tab
                ((GeneralSettings)propertyGrid3.SelectedObject).Save();

                if (propertyGrid2.SelectedObject != null)
                    ((Styx.Helpers.Settings)propertyGrid2.SelectedObject).Save();

                if (propertyGrid1.SelectedObject != null)
                    ((Styx.Helpers.Settings)propertyGrid1.SelectedObject).Save();

                // CleanseBlacklist.Instance.SpellList.Save();
                // PurgeWhitelist.Instance.SpellList.Save();
                // MageSteallist.Instance.SpellList.Save();

                Close();
            }
            catch (Exception ex)
            {
                Logging.Write("ERROR saving settings: {0}", ex.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            // update POI
            int i = 0;
            var sb = new StringBuilder();
            // poitype   distance 
            sb.Append(BotPoi.Current.Type.ToString());

            WoWObject o;
            try
            {
                o = BotPoi.Current.AsObject;
            }
            catch
            {
                o = null;
            }

            if (o != null)
                sb.Append(" @ " + o.Distance.ToString("F1") + " yds - " + o.SafeName());
            else if (BotPoi.Current.Type != PoiType.None)
                sb.Append(" @ " + BotPoi.Current.Location.Distance(StyxWoW.Me.Location).ToString("F1") + " yds - " + BotPoi.Current.Name);


            // update list of Targets
            i = 0;
            sb = new StringBuilder();
            foreach (WoWUnit u in Targeting.Instance.TargetList)
            {
                try
                {
                    sb.AppendLine(u.SafeName().AlignLeft(20) + " " + u.HealthPercent.ToString("F1").AlignRight(5) + "%  " + u.Distance.ToString("F1").AlignRight(5) + " yds");
                    if (++i == 5)
                        break;
                }
                catch (System.AccessViolationException)
                {
                }
                catch (Styx.InvalidObjectPointerException)
                {
                }
            }


            // update list of Heal Targets
            if (HealerManager.NeedHealTargeting)
            {
                i = 0;
                sb = new StringBuilder();
                foreach (WoWUnit u in HealerManager.Instance.TargetList)
                {
                    try
                    {
                        sb.AppendLine(u.SafeName().AlignLeft(22) + "- " + u.HealthPercent.ToString("F1").AlignRight(5) + "% @ " + u.Distance.ToString("F1").AlignRight(5) + " yds");
                        if (++i == 5)
                            break;
                    }
                    catch (System.AccessViolationException)
                    {
                    }
                    catch (Styx.InvalidObjectPointerException)
                    {
                    }
                }
            }
        }

        private void ConfigurationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                SetLabelColumnWidth(propertyGrid3, 205);
            else if (tabControl1.SelectedIndex == 1)
                SetLabelColumnWidth(propertyGrid2, 205);
        }
    }
}
