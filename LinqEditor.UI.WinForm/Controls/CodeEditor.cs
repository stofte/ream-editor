using LinqEditor.Common.Context;
using LinqEditor.Core.CodeAnalysis.Editor;
using LinqEditor.Core.CodeAnalysis.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla _editor;
        IContext _context;
        IBackgroundCompletion _completionHelper { get; set; }

        public string SourceCode { get { return _editor.Text; } }

        public CodeEditor(IBackgroundCompletion completion, IContext context)
        {
            _completionHelper = completion;
            _context = context;
            _context.ContextUpdated += async delegate(string path, string schema) 
            {
                await _completionHelper.InitializeAsync(path, schema);
            };

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
            if (_completionHelper == null) return;

            if (e.Ch == '.')
            {                
                await _completionHelper.UpdateFragmentAsync(_editor.Text);
                var result = await _completionHelper.MemberAccessExpressionCompletionsAsync(_editor.CurrentPos);
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
