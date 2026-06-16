using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class AddUserForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private ComboBox cmbRole;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public AddUserForm()
        {
            this.Text = "Add New User";
            this.Size = new Size(420, 440);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
        }

        private void BuildForm()
        {
            int x = 30, w = 340, y = 24;

            // ── Title ──────────────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Add New User",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 36;

            this.Controls.Add(new Label
            {
                Text = "Create a login for a team member.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 34;

            // ── Username ───────────────────────────────────────
            AddFieldLabel("USERNAME *", x, ref y);
            txtUsername = AddTextBox(x, y, w);
            y += 38;

            // ── Password ───────────────────────────────────────
            AddFieldLabel("PASSWORD *", x, ref y);
            txtPassword = AddTextBox(x, y, w);
            txtPassword.PasswordChar = '●';
            y += 38;

            // ── Confirm password ───────────────────────────────
            AddFieldLabel("CONFIRM PASSWORD *", x, ref y);
            txtConfirmPassword = AddTextBox(x, y, w);
            txtConfirmPassword.PasswordChar = '●';
            y += 38;

            // ── Role ───────────────────────────────────────────
            AddFieldLabel("ROLE *", x, ref y);
            cmbRole = new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                Font = new Font("Segoe UI", 10f)
            };
            cmbRole.Items.AddRange(new object[] { "User", "Admin" });
            cmbRole.SelectedIndex = 0;
            this.Controls.Add(cmbRole);
            y += 44;

            // ── Error label ────────────────────────────────────
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(230, 90, 90),
                Location = new Point(x, y),
                Size = new Size(w, 36),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblError);
            y += 40;

            // ── Buttons ────────────────────────────────────────
            btnSave = new Button
            {
                Text = "Create User",
                Location = new Point(x, y),
                Size = new Size(165, 42),
                BackColor = Color.FromArgb(37, 99, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(47, 115, 200);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(x + 175, y),
                Size = new Size(165, 42),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnCancel.FlatAppearance.BorderColor = AppColors.Border;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        // ── Save handler ───────────────────────────────────────
        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string role = cmbRole.SelectedItem.ToString();

            // ── Validation ──────────────────────────────────
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowFieldError("Username is required.", txtUsername);
                return;
            }
            if (username.Length < 3)
            {
                ShowFieldError("Username must be at least 3 characters.", txtUsername);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowFieldError("Password is required.", txtPassword);
                return;
            }
            if (password.Length < 6)
            {
                ShowFieldError("Password must be at least 6 characters.", txtPassword);
                return;
            }
            if (password != confirm)
            {
                ShowFieldError("Passwords do not match.", txtConfirmPassword);
                return;
            }

            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "Creating...";

                if (UserService.UsernameExists(username))
                {
                    ShowFieldError($"Username '{username}' is already taken.", txtUsername);
                    btnSave.Enabled = true;
                    btnSave.Text = "Create User";
                    return;
                }

                UserService.AddUser(username, password, role);

                MessageBox.Show(
                    $"User '{username}' created successfully as {role}.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                btnSave.Enabled = true;
                btnSave.Text = "Create User";
            }
        }

        private void ShowFieldError(string message, Control field)
        {
            lblError.Text = message;
            field.Focus();
        }

        // ── UI helpers ─────────────────────────────────────────
        private void AddFieldLabel(string text, int x, ref int y)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 18;
        }

        private TextBox AddTextBox(int x, int y, int w)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 30,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            this.Controls.Add(txt);
            return txt;
        }
    }
}