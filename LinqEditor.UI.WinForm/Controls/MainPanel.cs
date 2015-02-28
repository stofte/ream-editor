using LinqEditor.Core.Session;
using LinqEditor.Core.Models.Editor;
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
        ToolStripStatusLabel _elapsedLabel;
        CodeEditor _editor;
        OutputPane _outputPane;
        ComboBox _contextSelector;
        Timer _statusTimer;
        Stopwatch _elapsedTimer;

        IAsyncSession _session = null;
        IAsyncSessionFactory _backgroundSessionFactory = null;
        IConnectionStore _connectionStore = null;
        ISettingsStore _settingsStore = null;
        bool _enableContextSelector = false;
        long _sessionLoadMs = 0;
        bool _sessionLoaded = false;
        string _initErrorMsg = string.Empty;
        Guid _contextId = Guid.Empty;
        System.Threading.CancellationTokenSource _tokenSource;

        Form _connectionManager;

        Stopwatch _editorFocusTimer;
        bool _restoreEditorFocusOnSplitterMoved;

        public event Action RemoveTab;
        
        public MainPanel(IAsyncSessionFactory sessionFactory, IConnectionStore connectionStore, ISettingsStore settingsStore,
            OutputPane outputPane, CodeEditor editor, ConnectionManager connectionManager)
        {
            _backgroundSessionFactory = sessionFactory;
            _connectionManager = connectionManager;
            _connectionStore = connectionStore;
            _settingsStore = settingsStore;
            _statusBar = new StatusStrip();
            _mainContainer = new SplitContainer();
            _statusTimer = new Timer();
            _editorFocusTimer = new Stopwatch();
            _elapsedTimer = new Stopwatch();
            _statusTimer.Interval = 20;
            // dont think this is that intensive, otherwise, ther animation skips updates it seems
            _statusTimer.Tick += delegate
            {
                _statusLabel.Invalidate();
                var elapsed = _elapsedTimer.Elapsed;
                // assumes 24 hours is enough
                _elapsedLabel.Text = string.Format("{0}:{1}:{2}:{3}", 
                    elapsed.Hours, 
                    elapsed.Minutes.ToString("D2"), 
                    elapsed.Seconds.ToString("D2"), 
                    elapsed.Milliseconds.ToString("D3"));
            };
            _statusTimer.Start();
            
            _mainContainer.SuspendLayout();
            _statusBar.SuspendLayout();
            SuspendLayout();

            // loads schema etc in async handlers
            Load += Main_Load;

            _mainContainer.Location = new Point(0, 0);
            _mainContainer.Orientation = Orientation.Horizontal;
            _mainContainer.Dock = DockStyle.Fill;
            _mainContainer.TabStop = false;
            // splitcontainer is tricky, set height from main form (approx seems ok)
            _mainContainer.FixedPanel = FixedPanel.None;
            _mainContainer.Height = Main.DefaultHeight;
            _mainContainer.SplitterDistance = 200;
            _mainContainer.Panel1MinSize = 45;
            _mainContainer.Panel2MinSize = 20;
            _mainContainer.GotFocus += _mainContainer_GotFocus;
            _mainContainer.SplitterMoved += _mainContainer_SplitterMoved;
            _mainContainer.LostFocus += _mainContainer_LostFocus;

            // status
            var secondStrip = new StatusStrip();
            secondStrip.Dock = DockStyle.Bottom;
            _statusBar.Dock = DockStyle.Top;
            _statusBar.GripStyle = ToolStripGripStyle.Hidden;
            _statusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            _statusBar.SizingGrip = false;
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            _statusLabel.Image = Resources.Icons.spinner;
            _statusLabel.Alignment = ToolStripItemAlignment.Left;
            _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
            _timeLabel = new ToolStripStatusLabel();
            _timeLabel.Alignment = ToolStripItemAlignment.Right;
            _rowCountLabel = new ToolStripStatusLabel();
            _rowCountLabel.Alignment = ToolStripItemAlignment.Right;
            _elapsedLabel = new ToolStripStatusLabel();
            _elapsedLabel.Alignment = ToolStripItemAlignment.Right;
            _statusBar.Items.AddRange(new[] { _statusLabel, _elapsedLabel, _rowCountLabel });

            // scintilla
            _editor = editor;
            _editor.LostFocus += _editor_LostFocus;
            _editor.Dock = DockStyle.Fill;

            // output thingy
            _outputPane = outputPane;
            _outputPane.Dock = DockStyle.Fill;
            _outputPane.DisplayedRowCountUpdated += delegate(int? count)
            {
                if (count.HasValue)
                {
                    
                    _rowCountLabel.Text = string.Format("{0} rows", count);
                }
                _rowCountLabel.Visible = count.HasValue;
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

            _stopButton.Click += delegate
            {
                if (_tokenSource != null)
                {
                    _stopButton.Enabled = false;
                    _tokenSource.Cancel();
                }
            };

            // select context
            _contextSelector = new ComboBox();
            _contextSelector.Dock = DockStyle.Top;
            _contextSelector.DropDownStyle = ComboBoxStyle.DropDownList;

            _contextSelector.SelectedIndexChanged += async delegate(object sender, EventArgs e)
            {
                if (!_enableContextSelector) return;
                var selected = _contextSelector.SelectedItem as Connection;
                if (selected == null) return;
                if (_contextId == selected.Id && _sessionLoaded) return;

                _elapsedTimer.Restart();
                _contextId = selected.Id;
                // prevents reselecting the same menu item, if session is still loading.
                // BindSession updates _sessionLoaded to the actual outcome
                _sessionLoaded = true;
            
                _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
                _statusLabel.Image = Icons.spinner;
                _timeLabel.Text = string.Empty;

                _executeButton.Enabled = false;
                await BindSession(selected.Id);

                // if the context changed, dont do anything
                if (_contextSelector.SelectedItem != selected) return;

                _elapsedTimer.Stop();

                if (_sessionLoaded)
                {
                    _executeButton.Enabled = true;
                    _statusLabel.Text = ApplicationStrings.EDITOR_READY;
                    _statusLabel.Image = Icons.ok_grey;
                    _timeLabel.Text = ApplicationStrings.EDITOR_TIMER_SESSION_LOADED_IN;
                    _settingsStore.LastConnectionUsed = selected.Id;
                }
                else
                {
                    _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_INITIALIZATION_ERROR;
                    _statusLabel.Image = Icons.critical;
                    _timeLabel.Text = ApplicationStrings.EDITOR_TIMER_COMPLETED_IN;
                    var msg = string.Format(ApplicationStrings.MESSAGE_BODY_SESSION_INITIALIZATION_ERROR, selected.ToString(), _initErrorMsg);
                    MessageBox.Show(msg, ApplicationStrings.MESSAGE_CAPTION_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
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
            _mainContainer.Panel2.Controls.Add(_statusBar);
            Controls.Add(_mainContainer);
            Controls.Add(_toolbar);
            _mainContainer.ResumeLayout(false);
            _mainContainer.PerformLayout();
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

            int oldSession = -1;
            if (_session != null)
            {
                oldSession = _session.GetHashCode();
                _backgroundSessionFactory.Release(_session);
            }
            var sessionId = Guid.NewGuid();
            _session = _backgroundSessionFactory.Create(sessionId);
            Debug.Assert(oldSession != _session.GetHashCode() && sessionId == _session.Id);
            var result = await _session.InitializeAsync(id);
            _sessionLoaded = result.Error == null;
            _sessionLoadMs = result.DurationMs;
            if (_sessionLoaded)
            {
                _editor.Session(sessionId);
                var loadResult = await _session.LoadAppDomainAsync();
                // loads appdomain and initializes connection
                _sessionLoadMs += loadResult.DurationMs;
            }
            else
            {
                _initErrorMsg = result.Error.Message;
            }
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

        void _mainContainer_LostFocus(object sender, EventArgs e)
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
            else if (keyData == (Keys.F5 | Keys.Shift))
            {
                _stopButton.PerformClick();
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
            _elapsedTimer.Restart();
            // restore last used context
            var id = _settingsStore.LastConnectionUsed;
            _contextId = id;
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
            _timeLabel.Text = ApplicationStrings.EDITOR_TIMER_SESSION_LOADED_IN;
            
            _statusLabel.Image = Icons.ok_grey;
            _executeButton.Enabled = true;
            _enableContextSelector = true;
            _elapsedTimer.Stop();
        }

        async void _executeButton_Click(object sender, EventArgs e)
        {
            _elapsedTimer.Restart();
            _tokenSource = new System.Threading.CancellationTokenSource();
            _statusLabel.Text = ApplicationStrings.EDITOR_QUERY_EXECUTING;
            
            _statusLabel.Image = Icons.spinner;
            _timeLabel.Text = string.Empty;
            var btn = sender as ToolStripButton;
            btn.Enabled = false;
            _stopButton.Enabled = true;
            var result = await _session.ExecuteAsync(_editor.SourceCode, _tokenSource.Token);
            _outputPane.BindOutput(result);

            // if we cancelled out, we need to reinit the app domain
            if (result.Cancelled)
            {
                _statusLabel.Text = ApplicationStrings.EDITOR_SESSION_LOADING;
                var reInitResult = await _session.ReinitializeAsync();
                _timeLabel.Text = ApplicationStrings.EDITOR_TIMER_QUERY_CANCELLED_AFTER;
            }
            else
            {
                _timeLabel.Text = ApplicationStrings.EDITOR_TIMER_COMPLETED_IN;
            }

            _statusLabel.Text = ApplicationStrings.EDITOR_READY;
            _statusLabel.Image = Icons.ok_grey;
            btn.Enabled = true;
            _stopButton.Enabled = false;
            _elapsedTimer.Stop();
        }
    }
}
