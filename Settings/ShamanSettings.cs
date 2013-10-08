using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class ShamanSettings : Styx.Helpers.Settings
    {
        public ShamanSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Shaman.xml")) {}
    }
}
