using LinqEditor.Core.Settings;
using LinqEditor.UI.WinForm.Controls;
using LinqEditor.UI.WinForm.Resources;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm.Forms
{
    public class ConnectionManager : Form
    {
        IConnectionStore _connectionStore;

        public ConnectionManager(IConnectionStore connectionStore)
        {
            Font = SystemFonts.MenuFont;
            _connectionStore = connectionStore;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var width = 380;
            var height = 456;
            var addOffset = 220;
            var lastOffset = 405;

            var font = new Font("Consolas", 10);


            var layoutEdit = new FlowLayoutPanel();
            var editSelector = new ComboBox();
            var editRuler = new Label();
            var editHeader = new Label();
            var editConnStr = new TextBox2();
            var editTitle = new TextBox();
            var editType = new TextBox();
            var deleteBtn = new Button();
            var saveBtn = new Button();

            var layoutAdd = new FlowLayoutPanel();
            var addRuler = new Label();
            var addHeader = new Label();
            var addDescription = new Label();
            var addConnStr = new TextBox2();
            var addTitle = new TextBox();
            var addType = new ComboBox();
            var addButton = new Button();

            layoutEdit.SuspendLayout();
            layoutAdd.SuspendLayout();
            SuspendLayout();

            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "Connection manager";

            var editOffset = 20;

            layoutEdit.BackColor = Color.Transparent;
            layoutEdit.FlowDirection = FlowDirection.LeftToRight;
            layoutEdit.Location = new Point(15, editOffset + 15);
            layoutEdit.Height = 165;
            layoutEdit.Padding = new System.Windows.Forms.Padding(0);
            editRuler.AutoSize = false;
            editRuler.Height = 2;
            editRuler.Text = "";
            editRuler.BorderStyle = BorderStyle.Fixed3D;
            editRuler.Location = new Point(0, editOffset);
            editHeader.Text = "Existing connections";
            editHeader.Width = 105;
            editHeader.Location = new Point(20, editOffset - 7);
            editType.Enabled = false;
            editType.Width = width / 4;
            editTitle.Enabled = false;
            editConnStr.Enabled = false;
            editConnStr.Height = 75;
            editConnStr.Multiline = true;
            editConnStr.WordWrap = true;
            editConnStr.Font = font;
            editConnStr.ScrollBars = ScrollBars.Vertical;
            deleteBtn.Text = "Delete";
            deleteBtn.Enabled = false;
            saveBtn.Text = "Save";
            saveBtn.Enabled = false;
            editSelector.Dock = DockStyle.Top;
            editSelector.DropDownStyle = ComboBoxStyle.DropDownList;

            /////////////////////////////////////////////////////////////

            layoutAdd.BackColor = Color.Transparent;
            layoutAdd.FlowDirection = FlowDirection.LeftToRight;
            layoutAdd.Location = new Point(15, addOffset + 15);
            layoutAdd.Height = 160;
            addRuler.AutoSize = false;
            addRuler.Height = 2;
            addRuler.Text = "";
            addRuler.BorderStyle = BorderStyle.Fixed3D;
            addRuler.Location = new Point(0, addOffset);
            addHeader.Text = "Add connection";
            addHeader.Width = 85;
            addHeader.Location = new Point(20, addOffset - 7);
            addType.Width = width / 4;
            addType.DropDownStyle = ComboBoxStyle.DropDownList;
            addType.Items.Add("SQL Server");
            addType.SelectedItem = addType.Items[0];
            addType.Enabled = false;
            addDescription.Text = "Enter a connection string and an optional display title";
            addConnStr.Multiline = true;
            addConnStr.WordWrap = true;
            addConnStr.Height = 75;
            addConnStr.Font = font;
            addConnStr.ScrollBars = ScrollBars.Vertical;
            addButton.Text = "Add";
            addButton.Enabled = false;

            ///////////////////////////////////////////////////////////////

            Action bindConnections = () =>
            {
                var current = editSelector.SelectedItem as Connection;
                editSelector.Items.Clear();
                foreach (var conn in _connectionStore.Connections)
                {
                    if (conn.Kind == Core.Models.Editor.ProgramType.SqlServer)
                    {
                        editSelector.Items.Add(conn);
                    }
                }
                editSelector.Enabled = editSelector.Items.Count > 0;

                if (current != null)
                {
                    foreach (Connection conn in editSelector.Items)
                    {
                        if (current.Id == conn.Id)
                        {
                            editSelector.SelectedItem = conn;
                            break;
                        }
                    }
                }
                else if (editSelector.Items.Count > 0)
                {
                    // if we had no previous, we just pick the first item
                    editSelector.SelectedIndex = 0;
                }
            };

            Action clearEdit = () =>
            {
                editConnStr.Text = "";
                editConnStr.Enabled = false;
                editType.Text = "";
                editType.Enabled = false;
                editTitle.Text = "";
                editTitle.Enabled = false;
                deleteBtn.Enabled = false;
                saveBtn.Enabled = false;
            };

            deleteBtn.Click += delegate
            {
                var obj = editSelector.SelectedItem as Connection;
                _connectionStore.Delete(obj);
                clearEdit();
                editSelector.SelectedItem = null;
                bindConnections();
            };

            saveBtn.Click += delegate
            {
                try
                {
                    var conn = editSelector.SelectedItem as Connection;
                    conn.ConnectionString = editConnStr.Text;
                    conn.DisplayName = editTitle.Text;
                    clearEdit();
                    _connectionStore.Update(conn);
                    bindConnections();
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(ApplicationStrings.MESSAGE_BODY_CONNECTION_STRING_PARSE_ERROR, ApplicationStrings.MESSAGE_CAPTION_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            
            addButton.Click += delegate
            {
                var id =  Guid.NewGuid();

                try
                {
                    // connection throws 
                    _connectionStore.Add(new Connection { Id = id, ConnectionString = addConnStr.Text, DisplayName = addTitle.Text });
                    addConnStr.Text = string.Empty;
                    addTitle.Text = string.Empty;
                    addButton.Enabled = false;
                    bindConnections();
                    foreach (Connection obj in editSelector.Items)
                    {
                        if (obj.Id == id)
                        {
                            editSelector.SelectedItem = obj;
                            break;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(ApplicationStrings.MESSAGE_BODY_CONNECTION_STRING_PARSE_ERROR, ApplicationStrings.MESSAGE_CAPTION_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            addConnStr.TextChanged += delegate
            {
                addButton.Enabled = addConnStr.Text.Length > 0;
            };

            editSelector.SelectedValueChanged += delegate(object sender, EventArgs args)
            {
                var list = sender as ComboBox;
                var selected = list.SelectedItem as Connection;
                if (selected != null)
                {
                    editConnStr.Text = selected.ConnectionString;
                    editConnStr.Enabled = true;
                    editTitle.Text = selected.DisplayName;
                    editTitle.Enabled = true;
                    editType.Text = "SQL Server";
                    saveBtn.Enabled = true;
                    deleteBtn.Enabled = true;
                }
            };

            ////////////////////////////////////////////////////////////////////

            var lastRuler = new Label();
            lastRuler.AutoSize = false;
            lastRuler.Height = 2;
            lastRuler.Text = "";
            lastRuler.BorderStyle = BorderStyle.Fixed3D;
            lastRuler.Location = new Point(0, lastOffset);

            var closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Location = new Point(width - 91, lastOffset + 15);
            closeButton.Click += delegate { Hide(); };
            CancelButton = closeButton;

            layoutEdit.ClientSizeChanged += delegate
            {
                editSelector.Width = layoutEdit.ClientSize.Width - 15;
                editConnStr.Width = layoutEdit.ClientSize.Width - 15;
                editTitle.Width = layoutEdit.ClientSize.Width - 15;
            };
            
            layoutAdd.ClientSizeChanged += delegate
            {
                addDescription.Width = layoutAdd.ClientSize.Width - 15;
                addConnStr.Width = layoutAdd.ClientSize.Width - 15;
                addTitle.Width = layoutAdd.ClientSize.Width - 15;
            };
            
            ClientSizeChanged += delegate 
            { 
                editRuler.Width = ClientSize.Width + 20;
                addRuler.Width = ClientSize.Width + 20;
                lastRuler.Width = ClientSize.Width + 20;
                layoutAdd.Width = ClientSize.Width - 20;
                layoutEdit.Width = ClientSize.Width - 20;
            };

            // resets the form when shown
            VisibleChanged += delegate
            {
                if (Visible)
                {
                    addConnStr.Text = string.Empty;
                    addTitle.Text = string.Empty;
                    clearEdit();
                    editSelector.SelectedItem = null;
                    bindConnections();
                }
            };

            Load += delegate
            {
                // triggers sizing of content
                ClientSize = new Size(width, height);
            };

            layoutEdit.Controls.AddRange(new Control[] { editSelector, editConnStr, editTitle, editType, deleteBtn, saveBtn });
            layoutAdd.Controls.AddRange(new Control[] { addDescription, addConnStr, addTitle, addType, addButton });
            Controls.AddRange(new Control[] { editHeader, editRuler, layoutEdit, addHeader, addRuler, layoutAdd, lastRuler, closeButton });

            layoutAdd.ResumeLayout(false);
            layoutAdd.PerformLayout();
            layoutEdit.ResumeLayout(false);
            layoutEdit.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
