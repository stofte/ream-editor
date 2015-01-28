using LinqEditor.Core.CodeAnalysis.Editor;
using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla _editor;
        IDictionary<SuggestionType, int> _iconMap;

        public IBackgroundCompletion CompletionHelper { get; set; }
        public string SourceCode { get { return _editor.Text; } }

        
        public CodeEditor()
        {
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
            imageList.Images.Add(Resources.Types.Field);
            imageList.Images.Add(Resources.Types.Property);
            imageList.Images.Add(Resources.Types.Method);
            imageList.Images.Add(Resources.Types.ExtensionMethod);
            
            _editor.AutoComplete.RegisterImages(imageList, System.Drawing.Color.Magenta);
            _editor.AutoComplete.MaxHeight = 10;
            
            _editor.ConfigurationManager.Language = "cs";
        }

        async void _editor_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            if (e.Ch == '.')
            {
                await CompletionHelper.UpdateFragmentAsync(_editor.Text);
                var result = await CompletionHelper.MemberAccessExpressionCompletionsAsync(_editor.CurrentPos);
                _editor.AutoComplete.FillUpCharacters = "";
                _editor.AutoComplete.List = GetAutoCompleteList(result.Suggestions);
                _editor.AutoComplete.Show();
            }
        }

        private List<string> GetAutoCompleteList(IEnumerable<SuggestionEntry> suggestions)
        {
            return suggestions.Select(x => string.Format("{0}?{1}", x.Value, (int)x.Kind)).ToList();
        }
    }
}
