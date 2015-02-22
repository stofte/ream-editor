using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class ToolTip2 : Form
    {
        Color _borderColor = ColorTranslator.FromHtml("#CCCEDB");
        Color _backColor = ColorTranslator.FromHtml("#F6F6F6");
            

        public ToolTip2()
        {
            Height = 100;
            Width = 200;
            BackColor = _backColor;
            FormBorderStyle = FormBorderStyle.None;

            Load += delegate
            {
                ShowInTaskbar = false;
                TopMost = true;
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, _borderColor, ButtonBorderStyle.Solid);
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
