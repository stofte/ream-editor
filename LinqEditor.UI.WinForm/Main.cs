using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.Backend.Models;
using LinqEditor.Core.Backend.Repository;
using LinqEditor.Core.CodeAnalysis.Editor;
using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.UI.WinForm.Controls;
using ScintillaNET;
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

        ToolStrip _toolbar;
        SplitContainer _mainContainer;
        ToolStripButton _executeButton;
        StatusStrip _statusBar;
        ToolStripStatusLabel _editorStatusBarLabel;
        TextBox _connectionTextBox;
        CodeEditor _editor;
        OutputPane _outputPane;

        IBackgroundSession _connectionSession;
        IBackgroundCompletion _completionHelper;
        IWindsorContainer _container;

        private void InitializeWindsor()
        {
            _container = new WindsorContainer();
            _container.Install(FromAssembly.This());

            _connectionSession = _container.Resolve<IBackgroundSession>(new Arguments(new { connectionString = _connectionTextBox.Text }));
            _completionHelper = _container.Resolve<IBackgroundCompletion>();
        }

        public Main()
        {
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Linq Editor";
            Width = 800;
            Height = 500;

            // loads schema etc in async handlers
            Load += Main_Load;
            FormClosed += Main_FormClosed;
            
            _mainContainer = new SplitContainer();
            _mainContainer.Orientation = Orientation.Horizontal;
            _mainContainer.Dock = DockStyle.Fill;
            _mainContainer.TabStop = false;
            _mainContainer.SizeChanged += _mainContainer_SizeChanged;

            // status
            _statusBar = new StatusStrip();
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.GripStyle = ToolStripGripStyle.Hidden;
            _statusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            _editorStatusBarLabel = new ToolStripStatusLabel();
            
            // connection
            _connectionTextBox = new TextBox();
            _connectionTextBox.Dock = DockStyle.Top;
            _connectionTextBox.ReadOnly = true;
            _connectionTextBox.Text = TestConnectionString;

            // scintilla
            _editor = new CodeEditor();
            _editor.Dock = DockStyle.Fill;

            // output thingy
            _outputPane = new OutputPane();
            _outputPane.Dock = DockStyle.Fill;

            // toolbar
            _toolbar = new ToolStrip();
            _toolbar.Dock = DockStyle.Top;
            
            // play button
            _executeButton = new ToolStripButton();
            _executeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _executeButton.Image = Icons.play;
            _executeButton.Click += _executeButton_Click;
            _executeButton.Enabled = false;

            _toolbar.Items.Add(_executeButton);

            // add controls
            SuspendLayout();
            _statusBar.SuspendLayout();
            _mainContainer.Panel1.Controls.Add(_editor);
            _mainContainer.Panel1.Controls.Add(_connectionTextBox);
            _mainContainer.Panel2.Controls.Add(_outputPane);
            Controls.Add(_mainContainer);
            Controls.Add(_toolbar);
            Controls.Add(_statusBar);
            _statusBar.PerformLayout();
            ResumeLayout();

            InitializeWindsor();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                _executeButton.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task<ExecuteResult> Execute()
        {
            return await _connectionSession.ExecuteAsync(_editor.SourceCode);
        }

        void _mainContainer_SizeChanged(object sender, EventArgs e)
        {
            var container = sender as SplitContainer;
            _connectionTextBox.Width = container.ClientSize.Width;
        }

        void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            _container.Release(_connectionSession);
            _container.Release(_completionHelper);
            _container.Dispose();
        }

        async void Main_Load(object sender, EventArgs e)
        {
            var result = await _connectionSession.InitializeAsync(_connectionTextBox.Text);
            // init the completion helper with schema data
            await _completionHelper.InitializeAsync(result.AssemblyPath, result.SchemaNamespace);
            // passes helper to editor control
            _editor.CompletionHelper = _completionHelper;
            // loads appdomain and initializes connection
            await _connectionSession.LoadAppDomainAsync();
            _executeButton.Enabled = true;
        }

        async void _executeButton_Click(object sender, EventArgs e)
        {
            var btn = sender as ToolStripButton;
            btn.Enabled = false;
            var result = await Execute();
            _outputPane.BindOutput(result);
            btn.Enabled = true;
        }
    }
}
