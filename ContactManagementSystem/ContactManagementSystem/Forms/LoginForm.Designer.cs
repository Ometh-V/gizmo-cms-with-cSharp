using System.Drawing;
using System.Windows.Forms;

namespace ContactManagementSystem.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlTitleBar = new Panel();
            lblTitle = new Label();
            btnMinimize = new Button();
            btnClose = new Button();
            pnlMain = new Panel();
            picAvatar = new Panel();
            lblInitials = new Label();
            lblAppName = new Label();
            lblSubtitle = new Label();
            lblUsername = new Label();
            pnlUserInput = new Panel();
            picUserIcon = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            pnlPassInput = new Panel();
            picLockIcon = new Label();
            txtPassword = new TextBox();
            btnShowPass = new Button();
            chkRemember = new CheckBox();
            btnLogin = new Button();

            pnlTitleBar.SuspendLayout();
            pnlMain.SuspendLayout();
            picAvatar.SuspendLayout();
            pnlUserInput.SuspendLayout();
            pnlPassInput.SuspendLayout();
            SuspendLayout();

            // ── Form ───────────────────────────────────────────
            this.Text = "Contact Manager — Login";
            this.Size = new Size(420, 620);
            this.MinimumSize = new Size(420, 620);
            this.MaximumSize = new Size(420, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(18, 18, 20);
            this.ForeColor = Color.White;
            this.AcceptButton = btnLogin;

            // ── Title bar ──────────────────────────────────────
            pnlTitleBar.BackColor = Color.FromArgb(18, 18, 20);
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 36;
            pnlTitleBar.MouseDown += TitleBar_MouseDown;

            lblTitle.Text = "Contact Manager";
            lblTitle.Font = new Font("Segoe UI", 9f);
            lblTitle.ForeColor = Color.FromArgb(150, 150, 165);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(14, 10);
            lblTitle.BackColor = Color.Transparent;
            lblTitle.MouseDown += TitleBar_MouseDown;

            btnMinimize.Text = "─";
            btnMinimize.Font = new Font("Segoe UI", 9f);
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 65);
            btnMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 85);
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Size = new Size(42, 36);
            btnMinimize.Location = new Point(336, 0);
            btnMinimize.TabStop = false;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnClose.Text = "✕";
            btnClose.Font = new Font("Segoe UI", 9f);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 30, 20);
            btnClose.UseVisualStyleBackColor = false;
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = Color.White;
            btnClose.Size = new Size(42, 36);
            btnClose.Location = new Point(378, 0);
            btnClose.TabStop = false;
            btnClose.Click += (s, e) => this.Close();

            pnlTitleBar.Controls.AddRange(new Control[]
                { lblTitle, btnMinimize, btnClose });

            // ── Main card panel ────────────────────────────────
            pnlMain.BackColor = Color.FromArgb(36, 36, 42);
            pnlMain.Size = new Size(340, 490);
            pnlMain.Location = new Point(40, 65);
            pnlMain.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 340, 490, 18, 18));
            pnlMain.Controls.AddRange(new Control[]
            {
                picAvatar, lblAppName, lblSubtitle,
                lblUsername, pnlUserInput,
                lblPassword, pnlPassInput,
                chkRemember, btnLogin
            });

            // ── Avatar ─────────────────────────────────────────
            picAvatar.BackColor = Color.FromArgb(55, 82, 140);
            picAvatar.Size = new Size(58, 58);
            picAvatar.Location = new Point(141, 28);
            picAvatar.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 58, 58, 14, 14));
            picAvatar.Controls.Add(lblInitials);

            lblInitials.Text = "CM";
            lblInitials.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblInitials.ForeColor = Color.White;
            lblInitials.AutoSize = false;
            lblInitials.Size = new Size(58, 58);
            lblInitials.TextAlign = ContentAlignment.MiddleCenter;
            lblInitials.BackColor = Color.Transparent;

            // ── App name ───────────────────────────────────────
            lblAppName.Text = "Contact Manager";
            lblAppName.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.AutoSize = false;
            lblAppName.Size = new Size(300, 30);
            lblAppName.Location = new Point(20, 100);
            lblAppName.TextAlign = ContentAlignment.MiddleCenter;
            lblAppName.BackColor = Color.Transparent;

            // ── Subtitle ───────────────────────────────────────
            lblSubtitle.Text = "Sign in to your account";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = Color.FromArgb(150, 150, 165);
            lblSubtitle.AutoSize = false;
            lblSubtitle.Size = new Size(300, 22);
            lblSubtitle.Location = new Point(20, 134);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.BackColor = Color.Transparent;

            // ── Username label ─────────────────────────────────
            lblUsername.Text = "USERNAME";
            lblUsername.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(150, 150, 165);
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(30, 178);
            lblUsername.BackColor = Color.Transparent;

            // ── Username input ─────────────────────────────────
            pnlUserInput.Location = new Point(30, 198);
            pnlUserInput.Size = new Size(280, 44);
            pnlUserInput.BackColor = Color.FromArgb(50, 50, 58);
            pnlUserInput.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 44, 8, 8));
            pnlUserInput.Controls.AddRange(new Control[]
                { picUserIcon, txtUsername });

            picUserIcon.Text = "👤";
            picUserIcon.Font = new Font("Segoe UI", 11f);
            picUserIcon.Location = new Point(10, 11);
            picUserIcon.Size = new Size(24, 22);
            picUserIcon.BackColor = Color.Transparent;
            picUserIcon.ForeColor = Color.FromArgb(150, 150, 165);

            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Font = new Font("Segoe UI", 10f);
            txtUsername.Location = new Point(42, 12);
            txtUsername.Size = new Size(230, 22);
            txtUsername.BackColor = Color.FromArgb(50, 50, 58);
            txtUsername.ForeColor = Color.White;
            txtUsername.PlaceholderText = "Enter username";
            txtUsername.TabIndex = 0;

            // ── Password label ─────────────────────────────────
            lblPassword.Text = "PASSWORD";
            lblPassword.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(150, 150, 165);
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(30, 260);
            lblPassword.BackColor = Color.Transparent;

            // ── Password input ─────────────────────────────────
            pnlPassInput.Location = new Point(30, 280);
            pnlPassInput.Size = new Size(280, 44);
            pnlPassInput.BackColor = Color.FromArgb(50, 50, 58);
            pnlPassInput.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 44, 8, 8));
            pnlPassInput.Controls.AddRange(new Control[]
                { picLockIcon, txtPassword, btnShowPass });

            picLockIcon.Text = "🔒";
            picLockIcon.Font = new Font("Segoe UI", 11f);
            picLockIcon.Location = new Point(10, 11);
            picLockIcon.Size = new Size(24, 22);
            picLockIcon.BackColor = Color.Transparent;
            picLockIcon.ForeColor = Color.FromArgb(150, 150, 165);

            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 10f);
            txtPassword.Location = new Point(42, 12);
            txtPassword.Size = new Size(196, 22);
            txtPassword.BackColor = Color.FromArgb(50, 50, 58);
            txtPassword.ForeColor = Color.White;
            txtPassword.PasswordChar = '●';
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.TabIndex = 1;
            txtPassword.KeyDown += txtPassword_KeyDown;

            btnShowPass.Text = "👁";
            btnShowPass.FlatStyle = FlatStyle.Flat;
            btnShowPass.FlatAppearance.BorderSize = 0;
            btnShowPass.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnShowPass.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnShowPass.UseVisualStyleBackColor = false;
            btnShowPass.Location = new Point(246, 9);
            btnShowPass.Size = new Size(28, 26);
            btnShowPass.BackColor = Color.Transparent;
            btnShowPass.ForeColor = Color.FromArgb(150, 150, 165);
            btnShowPass.TabStop = false;
            btnShowPass.Click += btnShowPassword_Click;

            // ── Remember me ────────────────────────────────────
            chkRemember.Text = "Remember me";
            chkRemember.Font = new Font("Segoe UI", 9f);
            chkRemember.ForeColor = Color.FromArgb(150, 150, 165);
            chkRemember.Location = new Point(30, 342);
            chkRemember.AutoSize = true;
            chkRemember.BackColor = Color.Transparent;
            chkRemember.TabIndex = 2;

            // ── Sign In button ─────────────────────────────────
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.BackColor = Color.FromArgb(37, 99, 180);
            btnLogin.ForeColor = Color.White;
            btnLogin.Text = "Sign In";
            btnLogin.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(47, 115, 200);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 75, 150);
            btnLogin.Location = new Point(30, 390);
            btnLogin.Size = new Size(280, 46);
            btnLogin.TabIndex = 3;
            btnLogin.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 46, 8, 8));
            btnLogin.Click += btnLogin_Click;

            // ── Add everything to form ─────────────────────────
            this.Controls.Add(pnlTitleBar);
            this.Controls.Add(pnlMain);

            pnlTitleBar.ResumeLayout(false);
            pnlMain.ResumeLayout(false);
            picAvatar.ResumeLayout(false);
            pnlUserInput.ResumeLayout(false);
            pnlPassInput.ResumeLayout(false);
            ResumeLayout(false);
        }

        // ── Control declarations ───────────────────────────────
        private Panel pnlTitleBar;
        private Label lblTitle;
        private Button btnMinimize;
        private Button btnClose;
        private Panel pnlMain;
        private Panel picAvatar;
        private Label lblInitials;
        private Label lblAppName;
        private Label lblSubtitle;
        private Label lblUsername;
        private Panel pnlUserInput;
        private Label picUserIcon;
        private TextBox txtUsername;
        private Label lblPassword;
        private Panel pnlPassInput;
        private Label picLockIcon;
        private TextBox txtPassword;
        private Button btnShowPass;
        private CheckBox chkRemember;
        private Button btnLogin;
    }
}