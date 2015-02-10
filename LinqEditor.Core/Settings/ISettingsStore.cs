using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    public interface ISettingsStore
    {
        int MainX { get; set; }
        int MainY { get; set; }
        bool MainMaximized { get; set; }
        int ConnectionManagerX { get; set; }
        int ConnectionManagerY { get; set; }
        Guid LastConnectionUsed { get; set; }
    }
}
