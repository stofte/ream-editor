using LinqEditor.Core.Settings;
using LinqEditor.UI.WinForm.Controls;
using LinqEditor.UI.WinForm.Resources;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class Main : Form
    {
        public const int MinHeight = 290;
        public const int MinWidth = 400;

        public const int DefaultHeight = 500;
        public const int DefaultWidth = 800;

        int _tabCounter = 1;

        TabControl2 _tabControl;
        TabPage _createTab;
        IConnectionStore _store;
        ISettingsStore _settings;

        public Main(IConnectionStore store, ISettingsStore settings)
        {
            _store = store;
            _settings = settings;
            InitializeControl();
        }

        void InitializeControl()
        {
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = ApplicationStrings.APPLICATION_TITLE;
            if (_settings.MainMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            _tabControl = new TabControl2();
            _tabControl.ImageList = new ImageList();
            _tabControl.ImageList.Images.Add(Resources.Icons.action_add_16xMD);
            _tabControl.ShowToolTips = true;
            _createTab = new TabPage();
            _createTab.ImageIndex = 0;
            _createTab.ToolTipText = ApplicationStrings.TOOLTIP_NEW_TAB;
            
            var lastSelected = 0;

            _tabControl.Multiline = true;

            _tabControl.Deselecting += delegate(object sender, TabControlCancelEventArgs e)
            {
                lastSelected = e.TabPageIndex;
            };

            _tabControl.Selecting += delegate(object sender, TabControlCancelEventArgs e)
            {
                if (e.TabPage == _createTab)
                {
                    if (_tabControl.CtrlTabSwitching)
                    {
                        _tabControl.SelectedIndex = lastSelected == 0 ? _tabControl.TabPages.Count - 2 : 0;
                    }
                    else
                    {
                        var newForm = Program.Container.Resolve<MainPanel>();
                        newForm.Dock = DockStyle.Fill;
                        var newTab = new TabPage();
                        newTab.Text = string.Format(" Query {0} ", _tabCounter++);
                        
                        newTab.Controls.Add(newForm);
                        _tabControl.TabPages.Insert(_tabControl.TabPages.Count - 1, newTab);
                        _tabControl.SelectedIndex = _tabControl.TabPages.Count - 2;
                        newForm.RemoveTab += delegate
                        {
                            _tabControl.TabPages.Remove(newTab);
                            Program.Container.Release(newForm);
                        };
                    }
                }
                _tabControl.CtrlTabSwitching = false;
            };

            // dummy tab we add, then remove to get the initial tab created in the selecting event
            var triggerTab = new TabPage(); 
            _tabControl.Controls.AddRange(new TabPage[] { triggerTab, _createTab });
            
            ClientSizeChanged += delegate
            {
                _tabControl.Size = ClientSize;
            };

            Width = DefaultWidth;
            Height = DefaultHeight;

            Load += async delegate
            {
                _tabControl.TabPages.Remove(triggerTab);
                if (_store.LoadingError)
                {
                    await Task.Delay(200);
                    var path = ApplicationSettings.FileName(typeof(ConnectionStore));
                    
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var folderName = Path.GetDirectoryName(path);

                    var msg = string.Format(ApplicationStrings.MESSAGE_BODY_ERROR_LOADING_CONNECTIONS, fileName, folderName);
                    
                    MessageBox.Show(msg, ApplicationStrings.MESSAGE_CAPTION_ERROR,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            SizeChanged += delegate
            {
                _settings.MainMaximized = WindowState == FormWindowState.Maximized;
            };

            SuspendLayout();
            Controls.Add(_tabControl);
            ResumeLayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.T))
            {
                _tabControl.SelectedTab = _createTab;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
