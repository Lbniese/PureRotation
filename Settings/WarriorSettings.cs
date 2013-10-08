using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class WarriorSettings : Styx.Helpers.Settings
    {

        public WarriorSettings() : base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Warrior.xml")) {}
    }
}
