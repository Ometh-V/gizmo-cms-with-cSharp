using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class UserManagementForm : Form
    {
        private ListView lstUsers;
        private Button btnAddUser;
        private Button btnToggleActive;
        private Button btnChangeRole;
        private Button btnResetPassword;
        private Label lblCount;

        public UserManagementForm()
        {
            this.Text = "Manage Users";
            this.Size = new Size(700, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
            LoadUsers();
        }

        private void BuildForm()
        {
            // ── Title ──────────────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Manage Users",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(24, 28),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            lblCount = new Label
            {
                Text = "0 users",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 60),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblCount);

            // ── + Add User button ──────────────────────────────
            btnAddUser = MakeButton("+ Add User",
                Color.FromArgb(37, 99, 180), Color.White,
                new Point(556, 50), new Size(120, 36));
            btnAddUser.Click += BtnAddUser_Click;
            this.Controls.Add(btnAddUser);

            // ── User list ──────────────────────────────────────
            lstUsers = new ListView
            {
                Location = new Point(24, 96),
                Size = new Size(652, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                MultiSelect = false,
                BackColor = AppColors.Surface,
                ForeColor = AppColors.TextPrimary,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstUsers.Columns.Add("Username", 180);
            lstUsers.Columns.Add("Role", 100);
            lstUsers.Columns.Add("Status", 100);
            lstUsers.Columns.Add("Created", 140);
            lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;
            this.Controls.Add(lstUsers);

            // ── Action buttons ─────────────────────────────────
            int by = 408;

            btnToggleActive = MakeButton("Deactivate",
                Color.FromArgb(140, 40, 40), Color.White,
                new Point(24, by), new Size(140, 38));
            btnToggleActive.Enabled = false;
            btnToggleActive.Click += BtnToggleActive_Click;
            this.Controls.Add(btnToggleActive);

            btnChangeRole = MakeButton("Change Role",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(174, by), new Size(140, 38));
            btnChangeRole.Enabled = false;
            btnChangeRole.Click += BtnChangeRole_Click;
            this.Controls.Add(btnChangeRole);

            btnResetPassword = MakeButton("Reset Password",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(324, by), new Size(150, 38));
            btnResetPassword.Enabled = false;
            btnResetPassword.Click += BtnResetPassword_Click;
            this.Controls.Add(btnResetPassword);

            // ── Note ───────────────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Note: Deactivated users cannot log in. The last active admin cannot be deactivated or demoted.",
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 462),
                Size = new Size(650, 36),
                BackColor = Color.Transparent
            });
        }

        // ── Load users into the list ────────────────────────────
        private void LoadUsers()
        {
            try
            {
                lstUsers.Items.Clear();
                var table = UserService.GetAllUsers();

                foreach (System.Data.DataRow row in table.Rows)
                {
                    int userId = Convert.ToInt32(row["UserID"]);
                    string username = row["Username"].ToString();
                    string role = row["Role"].ToString();
                    bool isActive = Convert.ToBoolean(row["IsActive"]);
                    DateTime created = Convert.ToDateTime(row["CreatedAt"]);

                    var item = new ListViewItem(username);
                    item.SubItems.Add(role);
                    item.SubItems.Add(isActive ? "Active" : "Deactivated");
                    item.SubItems.Add(created.ToString("dd MMM yyyy"));
                    item.Tag = new UserRow
                    {
                        UserID = userId,
                        Username = username,
                        Role = role,
                        IsActive = isActive
                    };

                    if (!isActive)
                        item.ForeColor = AppColors.TextSecondary;

                    lstUsers.Items.Add(item);
                }

                lblCount.Text = $"{table.Rows.Count} user(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Selection changed ───────────────────────────────────
        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lstUsers.SelectedItems.Count > 0;
            btnToggleActive.Enabled = hasSelection;
            btnChangeRole.Enabled = hasSelection;
            btnResetPassword.Enabled = hasSelection;

            if (hasSelection)
            {
                var data = (UserRow)lstUsers.SelectedItems[0].Tag;
                btnToggleActive.Text = data.IsActive ? "Deactivate" : "Activate";
                btnToggleActive.BackColor = data.IsActive
                    ? Color.FromArgb(140, 40, 40)
                    : Color.FromArgb(37, 140, 80);
            }
        }

        // ── Add user ─────────────────────────────────────────────
        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            var form = new AddUserForm();
            if (form.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        // ── Activate / Deactivate ────────────────────────────────
        private void BtnToggleActive_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0) return;
            var data = (UserRow)lstUsers.SelectedItems[0].Tag;

            // Prevent deactivating yourself
            if (data.UserID == Session.UserID)
            {
                MessageBox.Show("You cannot deactivate your own account.",
                    "Action Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Prevent deactivating the last active admin
            if (data.IsActive && data.Role == "Admin" &&
                UserService.GetActiveAdminCount() <= 1)
            {
                MessageBox.Show(
                    "Cannot deactivate the last active admin.\nPromote another user to Admin first.",
                    "Action Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (data.IsActive)
                    UserService.DeactivateUser(data.UserID);
                else
                    UserService.ActivateUser(data.UserID);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Action failed.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Change role ──────────────────────────────────────────
        private void BtnChangeRole_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0) return;
            var data = (UserRow)lstUsers.SelectedItems[0].Tag;

            string newRole = data.Role == "Admin" ? "User" : "Admin";

            // Prevent demoting the last active admin
            if (data.Role == "Admin" && UserService.GetActiveAdminCount() <= 1)
            {
                MessageBox.Show(
                    "Cannot demote the last active admin.\nPromote another user to Admin first.",
                    "Action Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Change {data.Username}'s role from {data.Role} to {newRole}?",
                "Confirm Role Change",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                UserService.ChangeRole(data.UserID, newRole);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change role.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Reset password ──────────────────────────────────────
        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0) return;
            var data = (UserRow)lstUsers.SelectedItems[0].Tag;

            string newPassword = PromptForPassword(
                $"Enter a new password for '{data.Username}':");

            if (string.IsNullOrWhiteSpace(newPassword)) return;

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UserService.ResetPassword(data.UserID, newPassword);
                MessageBox.Show(
                    $"Password for '{data.Username}' has been reset.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reset password.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Small input dialog for password reset ───────────────
        private string PromptForPassword(string message)
        {
            using var form = new Form();
            var label = new Label
            { Text = message, Location = new Point(10, 10), Size = new Size(280, 40) };
            var textBox = new TextBox
            {
                Location = new Point(10, 55),
                Width = 260,
                PasswordChar = '●',
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary
            };
            var btnOk = new Button
            {
                Text = "Reset Password",
                Location = new Point(10, 90),
                Width = 130,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(37, 99, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            form.Text = "Reset Password";
            form.Size = new Size(310, 170);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.BackColor = AppColors.Background;
            form.ForeColor = AppColors.TextPrimary;
            form.AcceptButton = btnOk;
            form.Controls.AddRange(new Control[] { label, textBox, btnOk });

            return form.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }

        // ── Button factory ───────────────────────────────────────
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
                Font = new Font("Segoe UI", 9.5f),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ── Small data holder for ListView tags ──────────────────
        private class UserRow
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
            public bool IsActive { get; set; }
        }
    }
}