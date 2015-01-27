using LinqEditor.Core.CodeAnalysis.Editor;
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
        public IBackgroundCompletion CompletionHelper { get; set; }
        ScintillaNET.Scintilla _editor;

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
            initEditor.EndInit();

            SuspendLayout();
            Controls.Add(_editor);
            ResumeLayout();

            _editor.ConfigurationManager.Language = "cs";
        }

        public string SourceCode()
        {
            return _editor.Text;
        }
    }
}
