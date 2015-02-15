using LinqEditor.Core.Backend;
using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla _editor;
        IBackgroundSession _session;
        IBackgroundSessionFactory _sessionFactory;

        public string SourceCode { get { return _editor.Text; } }

        public CodeEditor(IBackgroundSessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
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
                if (Visible) _editor.Focus();
            };

            _editor = new ScintillaNET.Scintilla();
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
            _editor.CharAdded += _editor_CharAdded;
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
