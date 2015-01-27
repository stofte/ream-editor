using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    public class CodeEditor : UserControl
    {
        ScintillaNET.Scintilla Editor;

        public CodeEditor()
        {
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            var font = new System.Drawing.Font("Consolas", 10);

            Editor = new ScintillaNET.Scintilla();
            ((System.ComponentModel.ISupportInitialize)(Editor)).BeginInit();
            Editor.Dock = DockStyle.Fill;
            Editor.ConfigurationManager.Language = "C#";
            Editor.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.End;
            Editor.Font = font;
            Editor.Name = "_scintilla";
            Editor.TabIndex = 0;
            Editor.Text = "CodeListItem.Take(10).Dump();";
            
            ((System.ComponentModel.ISupportInitialize)(Editor)).EndInit();

            SuspendLayout();
            Controls.Add(Editor);
            ResumeLayout();
        }

        public string SourceCode()
        {
            return Editor.Text;
        }
    }
}
