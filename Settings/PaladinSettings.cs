using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class PaladinSettings : Styx.Helpers.Settings
    {
        public PaladinSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Paladin.xml")) {}
    }
}
