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

            _tabControl.Deselecting += delegate(object sender, TabControlCancelEventArgs e)
            {
                lastSelected = e.TabPageIndex;
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

            Width = 800;
            Height = 500;

            Load += delegate
            {
                _tabControl.TabPages.Remove(triggerTab);
            };

            SuspendLayout();
            Controls.Add(_tabControl);
            ResumeLayout();
        }
    }
}
