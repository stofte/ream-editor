using LinqEditor.Core.Backend;
using LinqEditor.Core.Settings;
using LinqEditor.UI.WinForm.Forms;
using LinqEditor.UI.WinForm.Resources;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class MainPanel : UserControl
    {
        ToolStrip _toolbar;
        SplitContainer _mainContainer;
        ToolStripButton _executeButton;
        ToolStripButton _stopButton;
        ToolStripButton _databaseButton;
        ToolStripButton _closeButton;
        StatusStrip _statusBar;
        ToolStripStatusLabel _statusLabel;
        ToolStripStatusLabel _timeLabel;
        ToolStripStatusLabel _rowCountLabel;
        CodeEditor _editor;
        OutputPane _outputPane;
        ComboBox _contextSelector;
        Timer _statusTimer;
        int _busyIcon = 0;

        IBackgroundSession _session;
        IBackgroundSessionFactory _backgroundSessionFactory;
        IConnectionStore _connectionStore;
        ISettingsStore _settingsStore;
        bool _enableContextSelector = false;
        long _sessionLoadMs = 0;
        System.Threading.CancellationTokenSource _tokenSource;

        Form _connectionManager;

        Stopwatch _editorFocusTimer;
        bool _restoreEditorFocusOnSplitterMoved;

        public event Action RemoveTab;

        public MainPanel(IBackgroundSessionFactory sessionFactory, IConnectionStore connectionStore, ISettingsStore settingsStore,
            OutputPane outputPane, CodeEditor editor, ConnectionManager connectionManager)
        {
            _backgroundSessionFactory = sessionFactory;
            _connectionManager = connectionManager;
            _connectionStore = connectionStore;
            _settingsStore = settingsStore;
            _statusBar = new StatusStrip();
            _statusTimer = new Timer();
            _statusTimer.Interval = 20;
            // dont think this is that intensive, otherwise, ther animation skips updates it seems
            _statusTimer.Tick += delegate
            {
                _statusLabel.Invalidate();
            };
            

            _statusBar.SuspendLayout();
            SuspendLayout();

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
            var minHeight = Main.MinHeight;
            var minWidth = Main.MinWidth;
            _mainContainer.MinimumSize = new Size(minWidth - 15, minHeight - 100);
            // fudged values
            _mainContainer.SplitterDistance = minHeight / 10;
            _mainContainer.Panel1MinSize = minHeight / 4;
            _mainContainer.Panel2MinSize = minHeight / 3;
            _mainContainer.GotFocus += _mainContainer_GotFocus;
            _mainContainer.SplitterMoved += _mainContainer_SplitterMoved;
            
            // status
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.GripStyle = ToolStripGripStyle.Hidden;
            _statusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            _statusBar.SizingGrip = false;
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            _statusLabel.Image = Resources.Icons.spinner;
            _statusLabel.Alignment = ToolStripItemAlignment.Left;
            _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
            _timeLabel = new ToolStripStatusLabel();
            _timeLabel.Alignment = ToolStripItemAlignment.Left;
            _rowCountLabel = new ToolStripStatusLabel();
            _rowCountLabel.Alignment = ToolStripItemAlignment.Right;
            _statusBar.Items.AddRange(new[] { _statusLabel, _timeLabel, _rowCountLabel });

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

            var dropButton = new ToolStripDropDownButton();
            var dropDown = new ToolStripDropDown();
            var t1 = new ToolStripMenuItem();
            t1.Text = "foo";
            dropDown.Items.AddRange(new ToolStripItem[] { t1 });
            dropButton.DropDown = dropDown;

            // play button
            _executeButton = new ToolStripButton();
            _executeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _executeButton.Image = Resources.Icons.start;
            _executeButton.Click += _executeButton_Click;
            _executeButton.Enabled = false;
            _executeButton.ToolTipText = ApplicationStrings.TOOLTIP_EXECUTE;
            // stop button
            _stopButton = new ToolStripButton();
            _stopButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _stopButton.Image = Resources.Icons.Record_16xLG;
            _stopButton.Enabled = false;
            _stopButton.ToolTipText = ApplicationStrings.TOOLTIP_STOP_EXECUTION;
            // connection manager
            _databaseButton = new ToolStripButton();
            _databaseButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _databaseButton.Image = Resources.Icons.DatabaseOptions_12882;
            _databaseButton.ToolTipText = ApplicationStrings.TOOLTIP_CONNECTION_MANAGER;
            _databaseButton.Click += _databaseButton_Click;
            // close tab
            _closeButton = new ToolStripButton();
            _closeButton.Alignment = ToolStripItemAlignment.Right;
            _closeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _closeButton.Image = Resources.Icons.active_x_16xLG;
            _closeButton.ToolTipText = ApplicationStrings.TOOLTIP_CLOSE_TAB;
            _toolbar.Items.AddRange(new[] { _executeButton, _stopButton, _databaseButton, _closeButton });

            _closeButton.Click += delegate
            {
                if (RemoveTab != null) RemoveTab();
            };

            _executeButton.EnabledChanged += delegate(object sender, EventArgs args)
            {
                var btn = sender as ToolStripButton;
                if (btn != null)
                {
                    _stopButton.Enabled = !btn.Enabled;
                }
            };

            _stopButton.Click += delegate
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                }
            };

            // select context
            _contextSelector = new ComboBox();
            _contextSelector.Dock = DockStyle.Top;
            _contextSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _contextSelector.SelectedIndexChanged += async delegate
            {
                if (!_enableContextSelector) return;
                _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
                _statusLabel.Image = Icons.spinner;
                _timeLabel.Text = string.Empty;
                var selected = _contextSelector.SelectedItem as Connection;
                if (selected == null) return;
                _executeButton.Enabled = false;
                await BindSession(selected.Id);
                _executeButton.Enabled = true;
                _statusLabel.Text = ApplicationStrings.EDITOR_READY;
                _statusLabel.Image = Icons.ok_grey;
                _timeLabel.Text = string.Format(ApplicationStrings.EDITOR_TIMER_SESSION_LOADED_IN, _sessionLoadMs);
                _settingsStore.LastConnectionUsed = selected.Id;
            };
            BindConnections();

            _connectionStore.ConnectionsUpdated += delegate
            {
                _enableContextSelector = false;
                BindConnections();
                _enableContextSelector = true;
            };

            // add controls and resume
            _mainContainer.Panel1.Controls.Add(_editor);
            _mainContainer.Panel1.Controls.Add(_contextSelector);
            _mainContainer.Panel2.Controls.Add(_outputPane);
            Controls.Add(_mainContainer);
            Controls.Add(_toolbar);
            Controls.Add(_statusBar);
            
            _statusBar.ResumeLayout(false);
            _statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }


        void BindConnections()
        {
            var selected = _contextSelector.SelectedItem as Connection;
            _contextSelector.Items.Clear();
            foreach (var conn in _connectionStore.Connections)
            {
                _contextSelector.Items.Add(conn);
            }
            if (selected != null)
            {
                // see if the previous context still exists
                foreach (Connection conn in _contextSelector.Items)
                {
                    if (conn.Id == selected.Id)
                    {
                        _contextSelector.SelectedItem = conn;
                    }
                }
            }
        }

        async Task BindSession(Guid id)
        {
            if (_session != null)
            {
                _backgroundSessionFactory.Release(_session);
            }
            var sessionId = Guid.NewGuid();
            _session = _backgroundSessionFactory.Create(sessionId);
            _editor.Session(sessionId);
            var result = await _session.InitializeAsync(id);
            _sessionLoadMs = result.DurationMs;
            // loads appdomain and initializes connection
            var loadResult = await _session.LoadAppDomainAsync();
            _sessionLoadMs += loadResult.DurationMs;
        }

        void _databaseButton_Click(object sender, EventArgs e)
        {
            _connectionManager.ShowDialog();
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

        /// <summary>
        /// Handles the GotFocus event of the _mainContainer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
            else if (keyData == (Keys.Control | Keys.W))
            {
                _closeButton.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        async void Main_Load(object sender, EventArgs e)
        {
            _statusTimer.Enabled = true;
            // restore last used context
            var id = _settingsStore.LastConnectionUsed;
            foreach (Connection conn in _contextSelector.Items)
            {
                if (conn.Id == id)
                {
                    _contextSelector.SelectedItem = conn;
                    break;
                }
            }
            await BindSession(id);
            _statusLabel.Text = ApplicationStrings.EDITOR_READY;
            _timeLabel.Text = string.Format(ApplicationStrings.EDITOR_TIMER_SESSION_LOADED_IN, _sessionLoadMs);
            _statusTimer.Enabled = false;
            _statusLabel.Image = Icons.ok_grey;
            _executeButton.Enabled = true;
            _enableContextSelector = true;
        }

        async void _executeButton_Click(object sender, EventArgs e)
        {
            _tokenSource = new System.Threading.CancellationTokenSource();
            _statusLabel.Text = ApplicationStrings.EDITOR_QUERY_EXECUTING;
            _statusTimer.Enabled = true;
            _statusLabel.Image = Icons.spinner;
            _timeLabel.Text = string.Empty;
            var btn = sender as ToolStripButton;
            btn.Enabled = false;
            var result = await _session.ExecuteAsync(_editor.SourceCode, _tokenSource.Token);
            _outputPane.BindOutput(result);

            // if we cancelled out, we need to reinit the app domain
            if (result.Cancelled)
            {
                _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
                var reInitResult = await _session.ReinitializeAsync();
                _timeLabel.Text = string.Format(ApplicationStrings.EDITOR_TIMER_QUERY_CANCELLED_AFTER, result.DurationMs + reInitResult.DurationMs);
            }
            else
            {
                _timeLabel.Text = string.Format(ApplicationStrings.EDITOR_TIMER_QUERY_COMPLETED_IN, result.DurationMs);
            }

            _statusLabel.Text = ApplicationStrings.EDITOR_READY;
            _statusTimer.Enabled = false;
            _statusLabel.Image = Icons.ok_grey;
            btn.Enabled = true;
        }
    }
}
