using LinqEditor.Core.Context;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Backend;
using LinqEditor.UI.WinForm.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinqEditor.Core.Settings;
using LinqEditor.UI.WinForm.Resources;

namespace LinqEditor.UI.WinForm.Forms
{
    public class Main : Form
    {
        public const int MinHeight = 290;
        public const int MinWidth = 400;
        int _tabCounter = 1;

        TabControl2 _tabControl;
        TabPage _createTab;

        public Main()
        {
            InitializeControl();
        }

        void InitializeControl()
        {
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = ApplicationStrings.APPLICATION_TITLE;
            MinimumSize = new Size(MinWidth, MinHeight);

            _tabControl = new TabControl2();
            var x = Resources.Icons.action_add_16xMD;
            _tabControl.ImageList = new ImageList();
            _tabControl.ImageList.Images.Add(x);
            _createTab = new TabPage();
            var createLabel = new Label();
            createLabel.Text = "empty";
            _createTab.ImageIndex = 0;
            var lastSelected = 0;
            var lastFocus = false;

            _tabControl.Deselecting += delegate(object sender, TabControlCancelEventArgs e)
            {
                lastSelected = e.TabPageIndex;
                lastFocus = e.TabPage.Focused;
            };

            _tabControl.Selecting += delegate(object sender, TabControlCancelEventArgs e)
            {
                if (e.TabPage == _createTab)
                {
                    //e.Cancel = true;

                    if (_tabControl.CtrlTabSwitching)
                    {
                        _tabControl.SelectedIndex = lastSelected == 0 ? _tabControl.TabPages.Count - 2 : 0;
                    }
                    else
                    {
                        var newForm = Program.Container.Resolve<MainPanel>();
                        newForm.Dock = DockStyle.Fill;
                        var newTab = new TabPage();
                        newTab.Text = string.Format("Query {0}", _tabCounter++);
                        newTab.Controls.Add(newForm);
                        _tabControl.TabPages.Insert(_tabControl.TabPages.Count - 1, newTab);
                        _tabControl.SelectedIndex = _tabControl.TabPages.Count - 2;

                        //_tabControl.SelectedTab = newTab;
                    }
                    if (lastFocus) _tabControl.SelectedTab.Focus();
                }
                _tabControl.CtrlTabSwitching = false;
            };
                        
            var defaultForm = Program.Container.Resolve<MainPanel>();

            var defaultTab = new TabPage();
            defaultTab.Text = "Query 1";
            defaultTab.Controls.Add(defaultForm);
            _tabControl.Controls.AddRange(new TabPage[] { defaultTab, _createTab });

            defaultTab.ClientSizeChanged += delegate
            {
                defaultForm.Size = defaultTab.ClientSize;
            };

            _tabControl.ClientSizeChanged += delegate
            {
                defaultTab.Size = _tabControl.ClientSize;
            };

            ClientSizeChanged += delegate
            {
                _tabControl.Size = ClientSize;
            };

            Width = 800;
            Height = 500;

            SuspendLayout();
            Controls.Add(_tabControl);
            ResumeLayout();
        }
    }
}
