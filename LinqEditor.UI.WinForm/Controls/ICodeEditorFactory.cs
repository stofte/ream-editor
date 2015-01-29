using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.UI.WinForm.Controls
{
    public interface ICodeEditorFactory
    {
        CodeEditor CreateCodeEditor();
    }
}
