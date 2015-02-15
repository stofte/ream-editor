using System;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Controls
{
    // http://stackoverflow.com/questions/6745193/how-can-i-make-winforms-tabpage-header-width-fit-its-title
    class TabControl2 : TabControl
    {
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
