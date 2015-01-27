using LinqEditor.Core.Backend.Models;
using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class OutputPane : UserControl
    {
        RichTextBox _console;
        DataGridView _diagnosticsList;
        TabControl _tabControl;
        TabPage _consoleTab;
        TabPage _diagnosticsTab;
        
        List<TabPage> _resultTabs;
        DataTable _diagnostics;

        public OutputPane()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _consoleTab = new TabPage("Console");
            _diagnosticsTab = new TabPage("Errors");
            _diagnosticsTab.ClientSizeChanged += TabClientSizeChanged;
            _diagnosticsList = new DataGridView();
            _diagnosticsList.ReadOnly = true;
            _diagnosticsList.AllowUserToAddRows = false;
            _diagnosticsList.DataBindingComplete += DiagnosticsListDataBindingComplete;
            _diagnosticsList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _diagnosticsList.DataSource = _diagnostics;
            _diagnosticsTab.Controls.Add(_diagnosticsList);
            _tabControl.TabPages.AddRange(new[] { _consoleTab, _diagnosticsTab });
            _console = new RichTextBox();
            _console.Dock = DockStyle.Fill;
            _console.ReadOnly = true;
            _console.BackColor = System.Drawing.Color.White;
            _consoleTab.Controls.Add(_console);

            _resultTabs = new List<TabPage>();
            _diagnostics = new DataTable();
            _diagnostics.Columns.AddRange(new[]{
                new DataColumn("Category", typeof(string)),
                new DataColumn("Location", typeof(string)),
                new DataColumn("Message", typeof(string))
            });

            SuspendLayout();
            Controls.Add(_tabControl);
            ResumeLayout();
        }

        void DiagnosticsListDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var view = sender as DataGridView;
            view.Columns[0].Width = 80;
            view.Columns[1].Width = 65;
            view.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        void TabClientSizeChanged(object sender, EventArgs e)
        {
            var tab = sender as TabPage;
            var grid = tab.Controls[0] as DataGridView;
            grid.Height = tab.ClientSize.Height;
            grid.Width = tab.ClientSize.Width;
        }

        void GridDataBindingCompleteAutoSize(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var view = sender as DataGridView;
            for (var i = 0; i < view.Columns.Count; i++)
            {
                view.Columns[i].AutoSizeMode = i < view.Columns.Count - 1 ?
                    DataGridViewAutoSizeColumnMode.AllCells : DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        public void BindOutput(ExecuteResult result)
        {
            if (result.Success)
            {
                
                _console.AppendText("Query Text\n");
                _console.AppendText(result.QueryText);
                BindResults(result.Tables);
                BindDiagnostics(new Error[]{}, result.Warnings);
                if (result.Tables.Count() > 0)
                {
                    _tabControl.SelectedTab = _tabControl.TabPages[_tabControl.TabCount-1];
                }
            }
            else
            {
                BindDiagnostics(result.Errors, result.Warnings);
                if (result.Warnings != null && result.Warnings.Count() > 0 ||
                    result.Errors != null && result.Errors.Count() > 0)
                {
                    _tabControl.SelectedTab = _tabControl.TabPages[1];
                }
            }
        }

        private void BindResults(IEnumerable<DataTable> tables)
        {
            SuspendLayout();
            foreach (var tab in _resultTabs)
            {
                _tabControl.TabPages.Remove(tab);
            }
            var newTabs = new List<TabPage>();
            foreach (var table in tables)
            {
                var tab = new TabPage(table.TableName);
                tab.ClientSizeChanged += TabClientSizeChanged;

                var grid = new DataGridView();
                grid.ReadOnly = true;
                grid.AllowUserToAddRows = false;
                grid.DataBindingComplete += GridDataBindingCompleteAutoSize;
                grid.DataSource = table;
                tab.Controls.Add(grid);
                newTabs.Add(tab);
            }
            _resultTabs = newTabs;
            _tabControl.TabPages.AddRange(_resultTabs.ToArray());
            ResumeLayout();
        }

        private void BindDiagnostics(IEnumerable<Error> errors, IEnumerable<Warning> warnings)
        {
            _diagnostics.Rows.Clear();

            var content = errors.Select(x => new
            {
                Category = "Error",
                Line = x.Location.StartLine,
                Column = x.Location.StartColumn,
                Location = string.Format("{0},{1}", x.Location.StartLine, x.Location.StartColumn),
                Message = x.Message
            }).Concat(warnings.Select(z => new
            {
                Category = "Warning",
                Line = z.Location.StartLine,
                Column = z.Location.StartColumn,
                Location = string.Format("{0},{1}", z.Location.StartLine, z.Location.StartColumn),
                Message = z.Message
            })).OrderByDescending(y => y.Line).ThenByDescending(u => u.Column);

            foreach (var item in content)
            {
                _diagnostics.Rows.Add(new[] { item.Category, item.Location, item.Message });
            }
        }
    }
}
