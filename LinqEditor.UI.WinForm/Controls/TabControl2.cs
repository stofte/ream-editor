using LinqEditor.UI.WinForm.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    // http://stackoverflow.com/questions/6745193/how-can-i-make-winforms-tabpage-header-width-fit-its-title
    class TabControl2 : TabControl
    {
        public TabControl2()
        {
            //DrawMode = TabDrawMode.OwnerDrawFixed;
            //DrawItem += delegate (object sender, DrawItemEventArgs e)
            //{
            //    // the create tab
            //    if (e.Index == this.TabCount - 1)
            //    {
            //        e.Graphics.DrawImage(Icons.action_add_16xMD, e.Bounds.Left + 3, e.Bounds.Top + 2);
            //    }
            //    else
            //    {
            //        e.Graphics.DrawString("x", e.Font, Brushes.Black, e.Bounds.Right - 15, e.Bounds.Top + 4);
            //        e.Graphics.DrawString(TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + 3, e.Bounds.Top + 3);
            //    }
            //};
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
        }
        
        public bool CtrlTabSwitching { get; set; }
        public bool CtrlTabSwitchingShift { get; set; }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Send TCM_SETMINTABWIDTH
            SendMessage(this.Handle, 0x1300 + 49, IntPtr.Zero, (IntPtr)10);
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            CtrlTabSwitching = keyData == (Keys.Control | Keys.Tab) ||
                keyData == (Keys.Control | Keys.Shift | Keys.Tab);
            CtrlTabSwitchingShift = CtrlTabSwitching && keyData == Keys.Shift;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
