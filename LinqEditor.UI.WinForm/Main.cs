using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.Backend.Repository;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm
{
    public class Main : Form
    {
        public static string TestConnectionString = "Data Source=.\\sqlexpress;Integrated Security=True;Initial Catalog=Opera18500DB";

        ToolStrip Toolbar;
        SplitContainer MainContainer;
        ToolStripButton executeButton;
        TabControl TabControl;
        TabPage ConsoleTab;
        TabPage ResultTab;
        DataGridView ResultDataGrid;
        StatusStrip StatusBar;
        ToolStripStatusLabel EditorStatusBarLabel;

        TextBox ConnectionTextBox;

        ScintillaNET.Scintilla Editor;
        RichTextBox Console;
        ISession ConnectionSession;

        private IWindsorContainer IOCContainer;

        private void InitializeContainer()
        {
            IOCContainer = new WindsorContainer();
            IOCContainer.Install(FromAssembly.This());
        }

        public static Main Create()
        {
            return new Main();
        }

        public Main()
        {
            InitializeContainer();

            var font = new System.Drawing.Font("Consolas", 10);
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Linq Editor";
            Width = 800;
            Height = 500;
            FormClosed += Main_FormClosed;

            this.Load += Main_Load;
            

            MainContainer = new SplitContainer();
            MainContainer.Orientation = Orientation.Horizontal;
            MainContainer.Dock = DockStyle.Fill;
            MainContainer.TabStop = false;
            MainContainer.SizeChanged += MainContainer_SizeChanged;

            // status
            StatusBar = new StatusStrip();
            StatusBar.Dock = DockStyle.Bottom;
            StatusBar.GripStyle = ToolStripGripStyle.Hidden;
            StatusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            EditorStatusBarLabel = new ToolStripStatusLabel();
            
            // connection
            ConnectionTextBox = new TextBox();
            ConnectionTextBox.Dock = DockStyle.Top;
            ConnectionTextBox.Text = TestConnectionString;

            // scintilla
            Editor = new ScintillaNET.Scintilla();
            ((System.ComponentModel.ISupportInitialize)(Editor)).BeginInit();
            Editor.Dock = DockStyle.Fill;
            Editor.ConfigurationManager.Language = "C#";
            Editor.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.End;
            Editor.Font = font;
            Editor.Name = "_scintilla";
            Editor.TabIndex = 0;
            Editor.Text = "CodeListItem.Take(10).Dump();";
            Editor.CharAdded += Editor_CharAdded;
            ((System.ComponentModel.ISupportInitialize)(Editor)).EndInit();

            // toolbar
            Toolbar = new ToolStrip();
            Toolbar.Dock = DockStyle.Top;
            
            // play button
            executeButton = new ToolStripButton();
            executeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            executeButton.Image = Icons.play;
            executeButton.Click += executeButton_Click;
            executeButton.Enabled = false;
            Toolbar.Items.Add(executeButton);

            // tabControl
            TabControl = new TabControl();
            TabControl.Dock = DockStyle.Fill;
            ConsoleTab = new TabPage("Console");
            ResultTab = new TabPage("Results");
            ResultTab.ClientSizeChanged += ResultTab_ClientSizeChanged;
            TabControl.TabPages.AddRange(new[] { ConsoleTab, ResultTab });

            Console = new RichTextBox();
            Console.Dock = DockStyle.Fill;
            Console.ReadOnly = true;
            Console.Font = font;
            Console.BackColor = Color.White;
            ConsoleTab.Controls.Add(Console);

            ResultDataGrid = new DataGridView();
            ResultDataGrid.ReadOnly = true;
            ResultDataGrid.AllowUserToAddRows = false;
            ResultDataGrid.DataBindingComplete += ResultDataGrid_DataBindingComplete;
            ResultTab.Controls.Add(ResultDataGrid);

            StatusBar.SuspendLayout();
            SuspendLayout();
            // add controls

            MainContainer.Panel1.Controls.Add(Editor);
            MainContainer.Panel1.Controls.Add(ConnectionTextBox);
            MainContainer.Panel2.Controls.Add(TabControl);

            Controls.Add(MainContainer);
            Controls.Add(Toolbar);
            Controls.Add(StatusBar);

            StatusBar.ResumeLayout(false);
            StatusBar.PerformLayout();

            ResumeLayout(false);
            PerformLayout();

            Editor.ConfigurationManager.Language = "cs";

            ConnectionSession = IOCContainer.Resolve<ISession>(new Arguments(new { connectionString = ConnectionTextBox.Text }));
        }

        async void Main_Load(object sender, EventArgs e)
        {
            await ConnectionSession.Initialize();
            executeButton.Enabled = true;
        }

        void Editor_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            if (e.Ch == '.' && false)
            {
                var l = new List<string>();
                l.Add("AAA");
                l.Add("BBBBB");

                l.Sort();
                //Editor.AutoComplete.List = l;
                //Editor.AutoComplete.SingleLineAccept = false;
                Editor.AutoComplete.AutoHide = false;
                Editor.AutoComplete.FillUpCharacters = "";
                Editor.AutoComplete.Show(l);
            }
        }

        void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            IOCContainer.Release(ConnectionSession);
            IOCContainer.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                executeButton.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        async void executeButton_Click(object sender, EventArgs e)
        {
            var btn = sender as ToolStripButton;
            btn.Enabled = false;
            var tables = await Execute();
            ResultDataGrid.DataSource = tables.First();
            btn.Enabled = true;
        }

        private async Task<IEnumerable<DataTable>> Execute()
        {
            
            var tables = await ConnectionSession.Execute(Editor.Text);
            return tables;
            
        }

        void MainContainer_SizeChanged(object sender, EventArgs e)
        {
            var container = sender as SplitContainer;
            ConnectionTextBox.Width = container.ClientSize.Width;
        }

        void ResultTab_ClientSizeChanged(object sender, EventArgs e)
        {
            var tab = sender as TabPage;
            ResultDataGrid.Height = tab.ClientSize.Height;
            ResultDataGrid.Width = tab.ClientSize.Width;
        }

        void ResultDataGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var view = sender as DataGridView;
            for (var i = 0; i < view.Columns.Count; i++)
            {
                view.Columns[i].AutoSizeMode = i < view.Columns.Count - 1 ?
                    DataGridViewAutoSizeColumnMode.AllCells : DataGridViewAutoSizeColumnMode.Fill;
            }
            TabControl.SelectedTab = ResultTab;
        }
    }
}
