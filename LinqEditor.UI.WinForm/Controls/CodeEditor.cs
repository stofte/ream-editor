using LinqEditor.Core.Session;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.UI.WinForm.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using LinqEditor.Core.Helpers;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla _editor;
        ScintillaNET.INativeScintilla _editorNative;
        IAsyncSession _session;
        IAsyncSessionFactory _sessionFactory;
        ToolTip2 _tooltip;
        
        Timer _timer;
        object _timerLock;
        bool _timerEnabled;

        /// <summary>
        /// The word start position under the current cursor location.
        /// </summary>
        int _wordPos = -1;
        int _previousPos = -2;
        bool _tipSignaled = false;
        object _tipLock;
        int _textLineHeight;
        

        public string SourceCode { get { return _editor.Text; } }

        public CodeEditor(IAsyncSessionFactory sessionFactory, ToolTip2 tooltip)
        {
            _sessionFactory = sessionFactory;
            _tooltip = tooltip;
            _tipLock = new object();
            _timerLock = new object();
            InitializeComponent();
        }

        public void Session(Guid id)
        {
            if (_session != null)
            {
                _sessionFactory.Release(_session);
            }

            _session = _sessionFactory.Create(id);
        }

        private void InitializeComponent()
        {
            var editorFont = new System.Drawing.Font("Consolas", 10);

            // set editor focus on tab changes
            VisibleChanged += delegate 
            {
                if (Visible)
                {
                    lock (_timerLock)
                    {
                        _timerEnabled = true;
                        _timer.Start();
                    }
                    _tooltip.CurrentOwner = this;
                    _editor.Focus();
                }
                else
                {
                    lock (_timerLock)
                    {
                        _timerEnabled = false;
                        _timer.Stop();
                    }
                }
            };

            _editor = new ScintillaNET.Scintilla();
            _editorNative = _editor as ScintillaNET.INativeScintilla;
            var initEditor = _editor as ISupportInitialize;
            initEditor.BeginInit();
            _editor.Dock = DockStyle.Fill;
            _editor.ConfigurationManager.Language = "C#";
            _editor.Margins[1].Width = 0; // not sure what this is
            _editor.MatchBraces = true;
            _editor.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.End;
            _editor.Caret.Width = 2;
            _editor.Font = editorFont;
            _editor.Name = "_scintilla";
            _editor.TabIndex = 0;
            _editor.Text = "var x = 10;\n\nx.Dump();";
            _editor.CharAdded += _editor_CharAdded;

            _editor.MouseLeave += delegate
            {
                lock (_tipLock)
                {
                    _tipSignaled = false;
                    _previousPos = _wordPos;
                    _wordPos = -1;
                }
            };

            _editor.MouseMove += delegate
            {
                var mousePos = _editor.PointToClient(MousePosition);
                var charPos = _editorNative.PositionFromPointClose(mousePos.X, mousePos.Y);

                var stopChars = new[] { ' ', '=', '(', ')', ';', '\n' };

                if (_editor.CharAt(charPos) == default(char) || 
                    charPos >= 0 && stopChars.Contains(_editor.CharAt(charPos)))
                {
                    charPos = -1;
                }

                if (charPos < 0)
                {
                    // terminate tooltip
                    lock (_tipLock)
                    {
                        _tipSignaled = false;
                        _previousPos = _wordPos;
                        _wordPos = -1;
                    }
                    return;
                }
                else
                {
                    var wStart = _editorNative.WordStartPosition(charPos, true);
                    //Debug.WriteLine("wStart:" + wStart);
                    var debugStr = string.Format("charPos{0}, wStart: {1}, _mouseHoverWordStartPosition: {2}", charPos, wStart, _wordPos);
                    //Debug.WriteLine(debugStr);

                    // moved to another word
                    lock (_tipLock)
                    {
                        if (wStart != _wordPos)
                        {
                            _tipSignaled = false;
                            _previousPos = _wordPos;
                            _wordPos = wStart;
                        }
                    }
                }
            };

            initEditor.EndInit();

            SuspendLayout();
            Controls.Add(_editor);
            ResumeLayout();

            // https://scintillanet.codeplex.com/discussions/75490
            var imageList = new ImageList();
            // same order as enum order
            imageList.Images.Add(Resources.Types.FieldIcon);
            imageList.Images.Add(Resources.Types.Property_501);
            imageList.Images.Add(Resources.Types.Method_636);
            imageList.Images.Add(Resources.Types.ExtensionMethod_9571);
            
            _editor.AutoComplete.RegisterImages(imageList, System.Drawing.Color.Magenta);
            _editor.AutoComplete.MaxHeight = 10;
            
            _editor.ConfigurationManager.Language = "cs";

            Load += delegate
            {
                // todo: runtime font-size changes
                _textLineHeight = _editorNative.TextHeight(0);
            };

            // timer handles reading changes to the currently hovered word (defined by scintilla.)
            // when the word changes, the timer attempts to get tooltip info for the new word.
            _timer = new Timer();
            _timer.Interval = 10;
            _timer.Tick += async delegate
            {
                lock (_timerLock)
                {
                    if (!_timerEnabled) return;
                    _timer.Stop();
                }

                int current = -1;
                int prev = -1;
                bool signalled = false;
                lock (_tipLock)
                {
                    prev = _previousPos;
                    current = _wordPos;
                    signalled = _tipSignaled;
                }

                if (prev != current && !signalled)
                {
                    //Debug.WriteLine("Changed noticed: " + current);
                    if (current == -1)
                    {
                        _tooltip.KillTip();
                    }
                    else
                    {
                        var end = _editorNative.WordEndPosition(current, true);
                        var startX = _editorNative.PointXFromPosition(current);
                        var startY = _editorNative.PointYFromPosition(current);
                        var endX = _editorNative.PointXFromPosition(end);
                        var endY = _editorNative.PointYFromPosition(end);

                        var startPos = _editor.PointToScreen(new System.Drawing.Point(startX, startY));
                        var analysisResult = await _session.AnalyzeAsync(_editor.Text, current);
                        
                        if (current == _wordPos && analysisResult.Context == UserContext.ToolTip)
                        {
                            DebugLogger.Log("analysisResult " + analysisResult.ToolTip.TypeAndName);
                            //Debug.WriteLine("analysisResult " + analysisResult.Success + ", " + analysisResult.Context);
                            var endPos = _editor.PointToScreen(new System.Drawing.Point(endX, endY));
                            _tooltip.PlaceTip(startPos, endPos, _textLineHeight, analysisResult.ToolTip);
                        }
                    }

                    // check if we're still current
                    lock (_tipLock)
                    {
                        if (current == _wordPos && prev == _previousPos)
                        {
                            _tipSignaled = true;
                        }
                    }
                }

                lock (_timerLock)
                {
                    if (!_timerEnabled) return;
                    _timer.Start();
                }
            };

        }

        async void _editor_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            if (_session == null) return;

            if (e.Ch == '.')
            {
                var result = await _session.AnalyzeAsync(_editor.Text, _editor.CurrentPos - 1);
                if (result.Context == UserContext.MemberCompletion)
                {
                    _editor.AutoComplete.FillUpCharacters = "";
                    _editor.AutoComplete.List = GetAutoCompleteList(result.MemberCompletions);
                    _editor.AutoComplete.Show();
                }
            }
        }

        private List<string> GetAutoCompleteList(IEnumerable<CompletionEntry> suggestions)
        {
            return suggestions.Select(x => string.Format("{0}?{1}", x.Value, (int)x.Kind)).ToList();
        }
    }
}
