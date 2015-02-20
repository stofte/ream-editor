using System;

namespace LinqEditor.Core.Settings
{
    public interface ISettingsStore
    {
        long MainX { get; set; }
        long MainY { get; set; }
        bool MainMaximized { get; set; }
        long ConnectionManagerX { get; set; }
        long ConnectionManagerY { get; set; }
        Guid LastConnectionUsed { get; set; }
    }
}
