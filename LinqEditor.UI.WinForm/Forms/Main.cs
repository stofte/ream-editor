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
            _tabControl.Deselected += delegate(object sender, TabControlEventArgs e)
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
                        //_tabControl.SelectedTab = newTab;
                    }
                }
                _tabControl.CtrlTabSwitching = false;
            };

            //_tabControl.SelectedIndexChanged += delegate(object sender, EventArgs args)
            //{
            //    //if (_tabControl.SelectedTab == _createTab)
            //    //{
            //    //    //if (!_tabControl.CtrlTabSwitching)
            //    //    //{
            //    //        // proper mouse click on new tab
            //    //        var newForm = Program.Container.Resolve<MainPanel>();
            //    //        newForm.Dock = DockStyle.Fill;
            //    //        var newTab = new TabPage();
            //    //        newTab.Text = string.Format("Query {0}", _tabCounter++);
            //    //        newTab.Controls.Add(newForm);
            //    //        _tabControl.TabPages.Insert(_tabControl.TabPages.Count - 1, newTab);
            //    //        _tabControl.SelectedTab = newTab;
            //    //    //}
            //    //    //else
            //    //    //{
            //    //    //    // tabbing through panes with ctrl-tab, so skip the new tab
            //    //    //    var idx = _tabControl.CtrlTabSwitchingShift ? _tabControl.TabPages.Count - 2 : 0;
            //    //    //    _tabControl.SelectedIndex = idx;
            //    //    //    _tabControl.TabPages[idx].Refresh();
            //    //    //    //_tabControl.SelectedTab = _tabControl.TabPages[idx];
            //    //    //}
            //    //}
            //    Debug.Assert(_tabControl.SelectedTab != _createTab);
            //    //_tabControl.Refresh();
            //    //_tabControl.CtrlTabSwitching = false;
            //    //_tabControl.CtrlTabSwitchingShift = false;
            //};
            
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

        private void TabControl1_Selecting(Object sender, TabControlCancelEventArgs e)
        {

            System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "TabPage", e.TabPage);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "TabPageIndex", e.TabPageIndex);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "Action", e.Action);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "Cancel", e.Cancel);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "Selecting Event");
        }
    }
}
