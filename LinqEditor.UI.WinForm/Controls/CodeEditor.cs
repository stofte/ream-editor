using LinqEditor.Core.Context;
using LinqEditor.Core.CodeAnalysis.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using LinqEditor.Core.CodeAnalysis.Services;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla _editor;
        IAsyncTemplateCodeAnalysis _codeAnalyzer;

        public string SourceCode { get { return _editor.Text; } }

        public CodeEditor(IAsyncTemplateCodeAnalysis codeAnalyzer)
        {
            _codeAnalyzer = codeAnalyzer;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var font = new System.Drawing.Font("Consolas", 10);
            
            _editor = new ScintillaNET.Scintilla();
            var initEditor = _editor as ISupportInitialize;
            initEditor.BeginInit();
            _editor.Dock = DockStyle.Fill;
            _editor.ConfigurationManager.Language = "C#";
            _editor.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.End;
            _editor.Font = font;
            _editor.Name = "_scintilla";
            _editor.TabIndex = 0;
            _editor.Text = "CodeListItem.Take(10).Dump();";
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
            if (!_codeAnalyzer.IsReady) return;

            if (e.Ch == '.')
            {
                var result = await _codeAnalyzer.AnalyzeAsync(_editor.Text, _editor.CurrentPos-1);
                if (result.Context == EditContext.MemberCompletion)
                {
                    _editor.AutoComplete.FillUpCharacters = "";
                    _editor.AutoComplete.List = GetAutoCompleteList(result.MemberCompletions);
                    _editor.AutoComplete.Show();
                }
            }
        }

        private List<string> GetAutoCompleteList(IEnumerable<SuggestionEntry> suggestions)
        {
            return suggestions.Select(x => string.Format("{0}?{1}", x.Value, (int)x.Kind)).ToList();
        }
    }
}
