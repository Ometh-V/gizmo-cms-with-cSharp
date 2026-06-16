using System;
using System.Drawing;
using System.Runtime.InteropServices;
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

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("Gdi32.dll")] private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int we, int he);

        public AddUserForm()
        {
            this.Text = "Add New User";
            this.Size = new Size(440, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;   // custom title bar
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
        }

        private void BuildForm()
        {
            // ── Custom title bar (matches MainForm style) ──────────────────
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(18, 18, 20)
            };
            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); }
            };

            var lblTitle = new Label
            {
                Text = "Add New User",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 165),
                AutoSize = true,
                Location = new Point(14, 9),
                BackColor = Color.Transparent
            };
            lblTitle.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); }
            };

            var btnClose = MakeTitleBarButton("✕");
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            btnClose.Click += (s, e) => this.Close();

            titleBar.Controls.AddRange(new Control[] { lblTitle, btnClose });
            titleBar.Resize += (s, e) => btnClose.Location = new Point(titleBar.Width - 42, 0);
            this.Controls.Add(titleBar);

            // ── Body ───────────────────────────────────────────────────────
            int x = 30, w = 360, y = 56;   // y starts below title bar

            AddLabel("Add New User", x, ref y,
                new Font("Segoe UI", 14f, FontStyle.Bold), AppColors.TextPrimary);
            AddLabel("Create a login for a team member.", x, ref y,
                new Font("Segoe UI", 9f), AppColors.TextSecondary);
            y += 8;

            // USERNAME
            AddFieldLabel("USERNAME *", x, ref y);
            txtUsername = AddTextBox(x, y, w); y += 46;

            // PASSWORD
            AddFieldLabel("PASSWORD *", x, ref y);
            txtPassword = AddTextBox(x, y, w);
            txtPassword.PasswordChar = '●'; y += 46;

            // CONFIRM PASSWORD
            AddFieldLabel("CONFIRM PASSWORD *", x, ref y);
            txtConfirmPassword = AddTextBox(x, y, w);
            txtConfirmPassword.PasswordChar = '●'; y += 46;

            // ROLE
            AddFieldLabel("ROLE *", x, ref y);
            cmbRole = BuildThemedCombo(x, y, w);
            cmbRole.Items.AddRange(new object[] { "User", "Admin" });
            cmbRole.SelectedIndex = 0;
            this.Controls.Add(cmbRole);
            y += 50;

            // Error label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(230, 90, 90),
                Location = new Point(x, y),
                Size = new Size(w, 22),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblError);
            y += 28;

            // ── Buttons flush to the bottom ────────────────────────────────
            int btnW = (w - 10) / 2;

            btnSave = MakeActionButton("Create User",
                Color.FromArgb(37, 99, 180), Color.White,
                new Point(x, y), new Size(btnW, 42));
            btnSave.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;

            btnCancel = MakeActionButton("Cancel",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(x + btnW + 10, y), new Size(btnW, 42));
            btnCancel.FlatAppearance.BorderColor = AppColors.Border;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            // Resize form to exactly fit content + 20px padding
            this.ClientSize = new Size(440, y + 42 + 20);
        }

        // ── Save ───────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string role = cmbRole.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(username)) { ShowFieldError("Username is required.", txtUsername); return; }
            if (username.Length < 3) { ShowFieldError("Username must be at least 3 characters.", txtUsername); return; }
            if (string.IsNullOrWhiteSpace(password)) { ShowFieldError("Password is required.", txtPassword); return; }
            if (password.Length < 6) { ShowFieldError("Password must be at least 6 characters.", txtPassword); return; }
            if (password != confirm) { ShowFieldError("Passwords do not match.", txtConfirmPassword); return; }

            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "Creating...";

                if (UserService.UsernameExists(username))
                {
                    ShowFieldError($"Username '{username}' is already taken.", txtUsername);
                    btnSave.Enabled = true; btnSave.Text = "Create User"; return;
                }

                UserService.AddUser(username, password, role);
                MessageBox.Show($"User '{username}' created successfully as {role}.",
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

        // ── Themed ComboBox (owner-draw so it matches the dark theme) ──────
        private ComboBox BuildThemedCombo(int x, int y, int w)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 32,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 26,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                Font = new Font("Segoe UI", 10f),
                FlatStyle = FlatStyle.Flat
            };

            cmb.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) != 0;
                Color bg = selected ? AppColors.NavActive : AppColors.SurfaceLight;
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawString(
                    cmb.Items[e.Index].ToString(),
                    cmb.Font,
                    new SolidBrush(AppColors.TextPrimary),
                    e.Bounds.X + 6, e.Bounds.Y + 4);
            };

            return cmb;
        }

        // ── UI helpers ─────────────────────────────────────────────────────
        private void AddLabel(string text, int x, ref int y, Font font, Color color)
        {
            var lbl = new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lbl);
            y += lbl.PreferredHeight + 4;
        }

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

        private Button MakeTitleBarButton(string text)
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

        private Button MakeActionButton(string text, Color bg, Color fg, Point loc, Size size)
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
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.1f);
            btn.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, size.Width, size.Height, 6, 6));
            return btn;
        }
    }
}