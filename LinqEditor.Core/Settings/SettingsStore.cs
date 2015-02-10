using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    public class SettingsStore : ApplicationSettings, ISettingsStore
    {
        private int _mainX;
        private int _mainY;
        private bool _mainMaximized;
        private int _connectionManagerX;
        private int _connectionManagerY;
        private Guid _lastConnectionUsed;

        public int MainX 
        {
            get { return _mainX; }
            set { _mainX = value; Save(); }
        }

        public int MainY 
        {
            get { return _mainY; }
            set { _mainY = value; Save(); }
        }

        public bool MainMaximized 
        {
            get { return _mainMaximized; }
            set { _mainMaximized = value; Save(); }
        }

        public int ConnectionManagerX 
        {
            get { return _connectionManagerX; }
            set { _connectionManagerX = value; Save(); }
        }

        public int ConnectionManagerY 
        {
            get { return _connectionManagerY; }
            set { _connectionManagerY = value; Save(); }
        }

        public Guid LastConnectionUsed 
        {
            get { return _lastConnectionUsed; }
            set { _lastConnectionUsed = value; Save(); }
        }

        public static SettingsStore Instance
        {
            get
            {
                SettingsStore store = Read<SettingsStore>();
                if (store == null)
                {
                    store = new SettingsStore()
                    {
                        LastConnectionUsed = Guid.Empty
                    };
                }
                return store;
            }
        }
    }
}
