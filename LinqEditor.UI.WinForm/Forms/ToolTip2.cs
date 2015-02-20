using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class ToolTip2 : Form
    {
        public ToolTip2()
        {
            Height = 100;
            Width = 200;

            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            Load += delegate
            {
                ShowInTaskbar = false;
                TopMost = true;
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // sets toolwindow styling, which prevents window from appearing in alt-tab list
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }
    }
}
