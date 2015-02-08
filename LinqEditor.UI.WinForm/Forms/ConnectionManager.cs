using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class ConnectionManager : Form
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        ComboBox _contextSelector;

        TextBox _addNewConnStr;
        TextBox _addNewTitle;
        GroupBox _addNewBox;
        Button _addNewButton;
        FlowLayoutPanel _layoutPanel;

        public ConnectionManager()
        {
            InitializeCompontent();
        }

        private void InitializeCompontent()
        {
            var width = 400;
            _layoutPanel = new FlowLayoutPanel();
            _addNewBox = new GroupBox();
            _layoutPanel.SuspendLayout();
            _addNewBox.SuspendLayout();
            SuspendLayout();
            Width = width;
            ShowInTaskbar = false;
            
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Padding = new System.Windows.Forms.Padding() { All = 10 };
            _contextSelector = new ComboBox();
            _contextSelector.Dock = DockStyle.Top;
            _contextSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _contextSelector.Enabled = false;

            _addNewBox.FlatStyle = FlatStyle.Standard;
            ClientSizeChanged += delegate { _addNewBox.Width = ClientSize.Width - 20; };
            _addNewBox.ClientSizeChanged += delegate { _layoutPanel.Width = _addNewBox.ClientSize.Width; };
            _layoutPanel.ClientSizeChanged += delegate { _addNewConnStr.Width = _layoutPanel.ClientSize.Width - 30; };
            _layoutPanel.BackColor = Color.Transparent;
            _layoutPanel.Location = new Point(10, 20);
            _addNewBox.Dock = DockStyle.Bottom;
            _addNewBox.Text = "New Connection";
            
            _addNewButton = new Button();
            _addNewButton.Text = "Add";
            _addNewConnStr = new TextBox();
            _addNewTitle = new TextBox();
            _layoutPanel.Controls.AddRange(new Control[] { _addNewConnStr, _addNewTitle, _addNewButton });
            _layoutPanel.FlowDirection = FlowDirection.LeftToRight;

            _addNewBox.Controls.Add(_layoutPanel);
            Controls.AddRange(new Control[] { _addNewBox, _contextSelector });

            _layoutPanel.ResumeLayout(false);
            _layoutPanel.PerformLayout();
            _addNewBox.ResumeLayout(false);
            _addNewBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

            SendMessage(_addNewConnStr.Handle, EM_SETCUEBANNER, 0, "Connection String");
            SendMessage(_addNewTitle.Handle, EM_SETCUEBANNER, 0, "Name");
        }
    }
}
