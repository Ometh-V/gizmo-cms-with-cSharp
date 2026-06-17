using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class GroupForm : Form
    {
        // ── Left panel controls ────────────────────────────────
        private ListBox lstGroups;
        private Button btnAddGroup;
        private Button btnRenameGroup;
        private Button btnDeleteGroup;
        private Label lblGroupCount;

        // ── Right panel controls ───────────────────────────────
        private Label lblGroupTitle;
        private ListBox lstInGroup;
        private ListBox lstAvailable;
        private Button btnAssign;   // ← (Available → InGroup)
        private Button btnRemove;   // → (InGroup → Available)
        private Label lblInGroup;
        private Label lblAvailable;

        private int _selectedGroupId = 0;

        // ── DllImports for borderless drag ────────────────────
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public GroupForm()
        {
            this.Text = "Manage Groups";
            this.Size = new Size(780, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;   // ← borderless like MainForm
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
            LoadGroups();
        }

        private void BuildForm()
        {
            // ── Custom title bar (matches MainForm style) ──────
            var titleBar = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(18, 18, 20)
            };
            titleBar.MouseDown += TitleBar_MouseDown;

            var lblTitle = new Label
            {
                Text = "⊡  Manage Groups",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 165),
                AutoSize = true,
                Location = new Point(14, 9),
                BackColor = Color.Transparent
            };
            lblTitle.MouseDown += TitleBar_MouseDown;

            var btnClose = MakeTitleBarButton("✕", 0);
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            btnClose.Click += (s, e) => this.Close();

            titleBar.Resize += (s, e) =>
                btnClose.Location = new Point(titleBar.Width - 42, 0);
            btnClose.Location = new Point(this.Width - 42, 0);

            titleBar.Controls.AddRange(new Control[] { lblTitle, btnClose });
            this.Controls.Add(titleBar);

            // ── Body panel (all content below title bar) ───────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Padding = new Padding(20, 16, 20, 20)
            };

            // Section title
            body.Controls.Add(new Label
            {
                Text = "Manage Groups",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(20, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            // ── LEFT PANEL — group list ────────────────────────
            var pnlLeft = new Panel
            {
                Location = new Point(20, 55),
                Size = new Size(230, 460),
                BackColor = AppColors.Surface
            };
            pnlLeft.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(AppColors.Border, 1),
                    0, 0, pnlLeft.Width - 1, pnlLeft.Height - 1);

            pnlLeft.Controls.Add(new Label
            {
                Text = "GROUPS",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(10, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            lblGroupCount = new Label
            {
                Text = "0 groups",
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(140, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlLeft.Controls.Add(lblGroupCount);

            lstGroups = new ListBox
            {
                Location = new Point(0, 32),
                Size = new Size(230, 340),
                BackColor = AppColors.Surface,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                SelectionMode = SelectionMode.One
            };
            lstGroups.SelectedIndexChanged += LstGroups_SelectedIndexChanged;
            pnlLeft.Controls.Add(lstGroups);

            // Group action buttons
            btnAddGroup = MakeButton("+ New",
                Color.FromArgb(37, 99, 180), Color.White,
                new Point(8, 382), new Size(68, 34));
            btnAddGroup.Click += BtnAddGroup_Click;
            pnlLeft.Controls.Add(btnAddGroup);

            btnRenameGroup = MakeButton("Rename",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(80, 382), new Size(72, 34));
            btnRenameGroup.Font = new Font("Segoe UI", 8f);
            btnRenameGroup.Enabled = false;
            btnRenameGroup.Click += BtnRenameGroup_Click;
            pnlLeft.Controls.Add(btnRenameGroup);

            btnDeleteGroup = MakeButton("Delete",
                Color.FromArgb(140, 40, 40), Color.White,
                new Point(156, 382), new Size(64, 34));
            btnDeleteGroup.Font = new Font("Segoe UI", 8f);
            btnDeleteGroup.Enabled = false;
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            pnlLeft.Controls.Add(btnDeleteGroup);

            body.Controls.Add(pnlLeft);

            // ── RIGHT PANEL — contacts in group ───────────────
            var pnlRight = new Panel
            {
                Location = new Point(265, 55),
                Size = new Size(490, 460),
                BackColor = AppColors.Surface
            };
            pnlRight.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(AppColors.Border, 1),
                    0, 0, pnlRight.Width - 1, pnlRight.Height - 1);

            lblGroupTitle = new Label
            {
                Text = "Select a group to manage members",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(12, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblGroupTitle);

            // IN THIS GROUP label + list
            lblInGroup = new Label
            {
                Text = "IN THIS GROUP",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(12, 46),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblInGroup);

            lstInGroup = new ListBox
            {
                Location = new Point(12, 66),
                Size = new Size(200, 340),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                SelectionMode = SelectionMode.One
            };
            pnlRight.Controls.Add(lstInGroup);

            // ── Arrow buttons ──────────────────────────────────
            // → button: moves selected contact FROM InGroup TO Available (Remove)
            btnRemove = MakeButton("→",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(224, 170), new Size(40, 36));
            btnRemove.Font = new Font("Segoe UI", 12f);
            btnRemove.Enabled = false;
            btnRemove.Click += BtnRemove_Click;
            pnlRight.Controls.Add(btnRemove);

            // ← button: moves selected contact FROM Available TO InGroup (Assign)
            btnAssign = MakeButton("←",
                Color.FromArgb(37, 99, 180), Color.White,
                new Point(224, 220), new Size(40, 36));
            btnAssign.Font = new Font("Segoe UI", 12f);
            btnAssign.Enabled = false;
            btnAssign.Click += BtnAssign_Click;
            pnlRight.Controls.Add(btnAssign);

            // AVAILABLE CONTACTS label + list
            lblAvailable = new Label
            {
                Text = "AVAILABLE CONTACTS",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(278, 46),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblAvailable);

            lstAvailable = new ListBox
            {
                Location = new Point(278, 66),
                Size = new Size(200, 340),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                SelectionMode = SelectionMode.One
            };
            pnlRight.Controls.Add(lstAvailable);

            body.Controls.Add(pnlRight);
            this.Controls.Add(body);
        }

        // ── Title bar drag ─────────────────────────────────────
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }

        // ── Load groups into left list ─────────────────────────
        private void LoadGroups()
        {
            try
            {
                lstGroups.Items.Clear();
                var groups = GroupService.GetAll();
                foreach (var g in groups)
                    lstGroups.Items.Add(g);

                lblGroupCount.Text = $"{groups.Count} group(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load groups.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Group selected — load its members ──────────────────
        private void LstGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem is not Group g) return;

            _selectedGroupId = g.GroupID;
            lblGroupTitle.Text = $"Group: {g.GroupName}";
            btnRenameGroup.Enabled = true;
            btnDeleteGroup.Enabled = true;
            btnAssign.Enabled = true;
            btnRemove.Enabled = true;

            LoadGroupMembers(g.GroupID);
        }

        // ── Load contacts in/out of selected group ─────────────
        private void LoadGroupMembers(int groupId)
        {
            try
            {
                lstInGroup.Items.Clear();
                var inGroup = GroupService.GetContactsInGroup(groupId);
                foreach (var c in inGroup)
                    lstInGroup.Items.Add(new ContactItem(c.ContactID, c.FullName));

                lstAvailable.Items.Clear();
                var available = GroupService.GetContactsNotInGroup(groupId);
                foreach (var c in available)
                    lstAvailable.Items.Add(new ContactItem(c.ContactID, c.FullName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load group members.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Add new group ──────────────────────────────────────
        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            string name = Prompt("Enter group name:");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                GroupService.Add(name);
                LoadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create group.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Rename group ───────────────────────────────────────
        private void BtnRenameGroup_Click(object sender, EventArgs e)
        {
            if (_selectedGroupId == 0) return;
            string newName = Prompt("Enter new name:");
            if (string.IsNullOrWhiteSpace(newName)) return;

            try
            {
                GroupService.Rename(_selectedGroupId, newName);
                LoadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename group.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Delete group ───────────────────────────────────────
        private void BtnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (_selectedGroupId == 0) return;

            var confirm = MessageBox.Show(
                "Delete this group? Contacts will not be deleted.",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                GroupService.Delete(_selectedGroupId);
                _selectedGroupId = 0;
                lstInGroup.Items.Clear();
                lstAvailable.Items.Clear();
                lblGroupTitle.Text = "Select a group to manage members";
                btnRenameGroup.Enabled = false;
                btnDeleteGroup.Enabled = false;
                btnAssign.Enabled = false;
                btnRemove.Enabled = false;
                LoadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete group.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── ← Assign: move from Available → InGroup ───────────
        private void BtnAssign_Click(object sender, EventArgs e)
        {
            if (lstAvailable.SelectedItem is not ContactItem item) return;

            try
            {
                GroupService.AssignContact(item.ContactID, _selectedGroupId);
                LoadGroupMembers(_selectedGroupId);
                LoadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to assign contact.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── → Remove: move from InGroup → Available ────────────
        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstInGroup.SelectedItem is not ContactItem item) return;

            try
            {
                GroupService.RemoveContact(item.ContactID, _selectedGroupId);
                LoadGroupMembers(_selectedGroupId);
                LoadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove contact.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Styled input prompt ────────────────────────────────
        private string Prompt(string message)
        {
            using var dlg = new Form();
            dlg.Text = "";
            dlg.Size = new Size(340, 160);
            dlg.FormBorderStyle = FormBorderStyle.None;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.BackColor = AppColors.Surface;

            // Mini title bar
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(18, 18, 20)
            };
            var barLabel = new Label
            {
                Text = message.Contains("new") || message.Contains("Enter group") ? "New Group" : "Rename Group",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 165),
                AutoSize = true,
                Location = new Point(12, 8),
                BackColor = Color.Transparent
            };
            var barClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel,
                Location = new Point(340 - 32, 0),
                TabStop = false
            };
            barClose.FlatAppearance.BorderSize = 0;
            barClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            bar.Controls.AddRange(new Control[] { barLabel, barClose });
            dlg.Controls.Add(bar);

            // Label
            var lbl = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(16, 44),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            dlg.Controls.Add(lbl);

            // Input box
            var txt = new TextBox
            {
                Location = new Point(16, 66),
                Width = 296,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            dlg.Controls.Add(txt);

            // OK button
            var ok = new Button
            {
                Text = "OK",
                Location = new Point(216, 102),
                Size = new Size(96, 30),
                BackColor = Color.FromArgb(37, 99, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9f)
            };
            ok.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(ok);

            dlg.AcceptButton = ok;
            dlg.CancelButton = barClose;

            // Allow dragging via the title bar
            bar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(dlg.Handle, 0xA1, 0x2, 0);
                }
            };
            barLabel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(dlg.Handle, 0xA1, 0x2, 0);
                }
            };

            txt.Focus();

            return dlg.ShowDialog(this) == DialogResult.OK
                ? txt.Text.Trim()
                : "";
        }

        // ── Button factory ─────────────────────────────────────
        private Button MakeButton(string text, Color bg, Color fg,
            Point loc, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ── Title bar button factory ───────────────────────────
        private Button MakeTitleBarButton(string text, int index)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(42, 36),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 65);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 85);
            btn.UseVisualStyleBackColor = false;
            return btn;
        }

        // ── Inner class for listbox items ──────────────────────
        private class ContactItem
        {
            public int ContactID { get; }
            public string Name { get; }

            public ContactItem(int id, string name)
            {
                ContactID = id;
                Name = name;
            }

            public override string ToString() => Name;
        }
    }
}