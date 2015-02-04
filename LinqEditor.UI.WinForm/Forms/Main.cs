using LinqEditor.Core.Context;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Backend.Repository;
using LinqEditor.UI.WinForm.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class Main : Form
    {
        public static string TestConnectionString = "Data Source=.\\sqlexpress;Integrated Security=True;Initial Catalog=Opera18500DB";

        ToolStrip _toolbar;
        SplitContainer _mainContainer;
        ToolStripButton _executeButton;
        StatusStrip _statusBar;
        ToolStripStatusLabel _statusLabel;
        ToolStripStatusLabel _rowCountLabel;
        TextBox _connectionTextBox;
        CodeEditor _editor;
        OutputPane _outputPane;

        IBackgroundSession _connectionSession;

        Stopwatch _editorFocusTimer;
        bool _restoreEditorFocusOnSplitterMoved;

        public Main(IBackgroundSession session, OutputPane outputPane, CodeEditor editor)
        {
            _connectionSession = session;
            _statusBar = new StatusStrip();
            _statusBar.SuspendLayout();
            SuspendLayout();
            var minHeight = 300;
            var minWidth = 400;

            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Linq Editor";
            Width = 800;
            Height = 500;
            MinimumSize = new Size(minWidth, minHeight);

            // loads schema etc in async handlers
            Load += Main_Load;

            _editorFocusTimer = new Stopwatch();
            _mainContainer = new SplitContainer();
            _mainContainer.Location = new Point(0, 0);
            _mainContainer.Orientation = Orientation.Horizontal;
            _mainContainer.Dock = DockStyle.Fill;
            _mainContainer.TabStop = false;
            _mainContainer.FixedPanel = FixedPanel.None;
            // must set this for panel minsizes to work. subtract height for statusbar+toolbar
            _mainContainer.MinimumSize = new Size(minWidth - 10, minHeight- 80);
            // fudged values
            _mainContainer.SplitterDistance = minHeight / 10;
            _mainContainer.Panel1MinSize = minHeight / 4;
            _mainContainer.Panel2MinSize = minHeight / 3;
            _mainContainer.SizeChanged += _mainContainer_SizeChanged;
            _mainContainer.GotFocus +=_mainContainer_GotFocus;
            _mainContainer.SplitterMoved += _mainContainer_SplitterMoved;
            
            // status
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.GripStyle = ToolStripGripStyle.Hidden;
            _statusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.Alignment = ToolStripItemAlignment.Left;
            _statusLabel.Text = "Loading";
            _rowCountLabel = new ToolStripStatusLabel();
            _rowCountLabel.Alignment = ToolStripItemAlignment.Right;
            _statusBar.Items.AddRange(new[] { _statusLabel, _rowCountLabel });
            
            // connection
            _connectionTextBox = new TextBox();
            _connectionTextBox.Dock = DockStyle.Top;
            _connectionTextBox.ReadOnly = true;
            _connectionTextBox.Text = TestConnectionString;

            // scintilla
            _editor = editor;
            _editor.LostFocus += _editor_LostFocus;
            _editor.Dock = DockStyle.Fill;

            // output thingy
            _outputPane = outputPane;
            _outputPane.Dock = DockStyle.Fill;
            _outputPane.DisplayedRowCountUpdated += delegate(int count) 
            { 
                _rowCountLabel.Text = string.Format("{0} rows", count); 
            };

            // toolbar
            _toolbar = new ToolStrip();
            _toolbar.Dock = DockStyle.Top;
            
            // play button
            _executeButton = new ToolStripButton();
            _executeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _executeButton.Image = Resources.Icons.startwithoutdebugging_6556;
            _executeButton.Click += _executeButton_Click;
            _executeButton.Enabled = false;
            _toolbar.Items.Add(_executeButton);

            // add controls
            _mainContainer.Panel1.Controls.Add(_editor);
            _mainContainer.Panel1.Controls.Add(_connectionTextBox);
            _mainContainer.Panel2.Controls.Add(_outputPane);
            Controls.Add(_mainContainer);
            Controls.Add(_toolbar);
            Controls.Add(_statusBar);
            _statusBar.ResumeLayout(false);
            _statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // some custom focus juggling when resizing the splitcontainer when the editor had focus
        void _mainContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (_restoreEditorFocusOnSplitterMoved)
            {
                _editor.Focus();
            }
        }

        void _editor_LostFocus(object sender, EventArgs e)
        {
            _editorFocusTimer.Restart();
            _restoreEditorFocusOnSplitterMoved = false;
        }

        void _mainContainer_GotFocus(object sender, EventArgs e)
        {
            _editorFocusTimer.Stop();
            _restoreEditorFocusOnSplitterMoved = _editorFocusTimer.ElapsedMilliseconds < 50; // more fudging
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

        async void Main_Load(object sender, EventArgs e)
        {
            var result = await _connectionSession.InitializeAsync(_connectionTextBox.Text);
            // loads appdomain and initializes connection
            await _connectionSession.LoadAppDomainAsync();
            _statusLabel.Text = "Query ready";
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
