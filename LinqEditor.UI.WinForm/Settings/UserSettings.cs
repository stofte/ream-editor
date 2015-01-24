using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.UI.WinForm.Settings
{
    
    public class UserSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        public IDictionary<string, string> CachedSchemas { get; set; }
    }
}
