using LinqEditor.Core.Settings;
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
        // http://stackoverflow.com/questions/11873378/adding-placeholder-text-to-textbox
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        ComboBox _editSelector;

        GroupBox _editBox;
        FlowLayoutPanel _editLayout;

        TextBox _addConnStr;
        TextBox _addTitle;
        GroupBox _addBox;
        Button _addButton;
        FlowLayoutPanel _addLayout;

        ISchemaStore _schemaStore;

        public ConnectionManager(ISchemaStore schemaStore)
        {
            _schemaStore = schemaStore;
            InitializeCompontent();
        }

        private void InitializeCompontent()
        {
            var width = 400;

            _addLayout = new FlowLayoutPanel();
            _addBox = new GroupBox();

            _editLayout = new FlowLayoutPanel();
            _editBox = new GroupBox();

            _addLayout.SuspendLayout();
            _addBox.SuspendLayout();
            SuspendLayout();

            Width = width;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Padding = new System.Windows.Forms.Padding() { All = 10 };
            ClientSizeChanged += delegate 
            {
                _editBox.Width = ClientSize.Width - 20;
                _addBox.Width = ClientSize.Width - 20; 
            };

            _editBox.FlatStyle = FlatStyle.Standard;
            _editBox.Dock = DockStyle.Top;
            _editBox.Text = "Edit Connection";
            _editBox.ClientSizeChanged += delegate { _editLayout.Width = _editBox.ClientSize.Width; };
            _editLayout.BackColor = Color.Transparent;
            _editLayout.Location = new Point(10, 20);
            _editLayout.ClientSizeChanged += delegate { _editSelector.Width = _editLayout.ClientSize.Width - 30; };
            _editSelector = new ComboBox();
            _editSelector.Dock = DockStyle.Top;
            _editSelector.DropDownStyle = ComboBoxStyle.DropDownList;

            _addBox.FlatStyle = FlatStyle.Standard;
            _addBox.ClientSizeChanged += delegate { _addLayout.Width = _addBox.ClientSize.Width; };
            _addLayout.ClientSizeChanged += delegate { _addConnStr.Width = _addLayout.ClientSize.Width - 30; };
            _addLayout.BackColor = Color.Transparent;
            _addLayout.Location = new Point(10, 20);
            _addBox.Dock = DockStyle.Bottom;
            _addBox.Text = "New Connection";
            _addButton = new Button();
            _addButton.Text = "Add";
            _addButton.Click += _addNewButton_Click;
            _addConnStr = new TextBox();
            _addTitle = new TextBox();
            _addLayout.FlowDirection = FlowDirection.LeftToRight;

            _editLayout.Controls.Add(_editSelector);
            _editBox.Controls.Add(_editLayout);
            _addLayout.Controls.AddRange(new Control[] { _addConnStr, _addTitle, _addButton });
            _addBox.Controls.Add(_addLayout);
            Controls.AddRange(new Control[] { _addBox, _editBox });

            _addLayout.ResumeLayout(false);
            _addLayout.PerformLayout();
            _addBox.ResumeLayout(false);
            _addBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

            BindConnections();
            SendMessage(_addConnStr.Handle, EM_SETCUEBANNER, 0, "Connection String");
            SendMessage(_addTitle.Handle, EM_SETCUEBANNER, 0, "Name");
        }

        void _addNewButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_addTitle.Text) &&
                !string.IsNullOrWhiteSpace(_addConnStr.Text))
            {
                _schemaStore.AddConnectionString(_addConnStr.Text, _addTitle.Text);
                _addConnStr.Text = string.Empty;
                _addTitle.Text = string.Empty;
            }
            BindConnections();
        }

        class BasicComboBoxItem
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", Name, ConnectionString);
            }
        }

        private void BindConnections()
        {
            _editSelector.Items.Clear();
            foreach (string connStr in _schemaStore.ConnectionNames.Keys)
            {
                _editSelector.Items.Add(new BasicComboBoxItem { Name = _schemaStore.ConnectionNames[connStr], ConnectionString = connStr });
            }
            _editSelector.Enabled = _editSelector.Items.Count > 0;
        }
    }
}
