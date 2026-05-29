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

            pnlMain.SuspendLayout();
            pnlUserInput.SuspendLayout();
            pnlPassInput.SuspendLayout();
            SuspendLayout();

            // ── Form ───────────────────────────────────────────
            this.Text = "Login";
            this.Size = new Size(420, 560);
            this.MinimumSize = new Size(420, 560);
            this.MaximumSize = new Size(420, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = btnLogin;

            // ── pnlMain — centered card ────────────────────────
            pnlMain.BackColor = Color.FromArgb(36, 36, 40);
            pnlMain.Size = new Size(340, 460);
            pnlMain.Location = new Point(40, 50);
            pnlMain.Padding = new Padding(30);
            int r = 16;
            pnlMain.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 340, 460, r, r));
            pnlMain.Controls.AddRange(new Control[]
            {
                picAvatar, lblAppName, lblSubtitle,
                lblUsername, pnlUserInput,
                lblPassword, pnlPassInput,
                chkRemember, btnLogin
            });

            // ── Avatar circle ──────────────────────────────────
            picAvatar.BackColor = Color.FromArgb(55, 80, 130);
            picAvatar.Size = new Size(56, 56);
            picAvatar.Location = new Point(142, 30);
            picAvatar.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 56, 56, 28, 28));
            picAvatar.Controls.Add(lblInitials);

            lblInitials.Text = "CM";
            lblInitials.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblInitials.ForeColor = Color.White;
            lblInitials.AutoSize = false;
            lblInitials.Size = new Size(56, 56);
            lblInitials.TextAlign = ContentAlignment.MiddleCenter;

            // ── App name & subtitle ────────────────────────────
            lblAppName.Text = "Contact Manager";
            lblAppName.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.AutoSize = false;
            lblAppName.Size = new Size(280, 30);
            lblAppName.Location = new Point(30, 100);
            lblAppName.TextAlign = ContentAlignment.MiddleCenter;

            lblSubtitle.Text = "Sign in to your account";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = Color.FromArgb(160, 160, 170);
            lblSubtitle.AutoSize = false;
            lblSubtitle.Size = new Size(280, 22);
            lblSubtitle.Location = new Point(30, 132);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // ── Username label ─────────────────────────────────
            lblUsername.Text = "USERNAME";
            lblUsername.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(160, 160, 170);
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(30, 172);

            // ── Username input panel ───────────────────────────
            pnlUserInput.Location = new Point(30, 194);
            pnlUserInput.Size = new Size(280, 42);
            pnlUserInput.BackColor = Color.FromArgb(50, 50, 56);
            pnlUserInput.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 42, 8, 8));
            pnlUserInput.Controls.AddRange(new Control[] { picUserIcon, txtUsername });

            picUserIcon.Text = "👤";
            picUserIcon.Font = new Font("Segoe UI", 11f);
            picUserIcon.Location = new Point(10, 10);
            picUserIcon.Size = new Size(24, 24);
            picUserIcon.BackColor = Color.Transparent;
            picUserIcon.ForeColor = Color.FromArgb(160, 160, 170);

            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Font = new Font("Segoe UI", 10f);
            txtUsername.Location = new Point(42, 11);
            txtUsername.Size = new Size(230, 22);
            txtUsername.BackColor = Color.FromArgb(50, 50, 56);
            txtUsername.ForeColor = Color.White;
            txtUsername.PlaceholderText = "Enter username";
            txtUsername.TabIndex = 0;

            // ── Password label ─────────────────────────────────
            lblPassword.Text = "PASSWORD";
            lblPassword.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(160, 160, 170);
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(30, 252);

            // ── Password input panel ───────────────────────────
            pnlPassInput.Location = new Point(30, 274);
            pnlPassInput.Size = new Size(280, 42);
            pnlPassInput.BackColor = Color.FromArgb(50, 50, 56);
            pnlPassInput.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 42, 8, 8));
            pnlPassInput.Controls.AddRange(new Control[]
                { picLockIcon, txtPassword, btnShowPass });

            picLockIcon.Text = "🔒";
            picLockIcon.Font = new Font("Segoe UI", 11f);
            picLockIcon.Location = new Point(10, 10);
            picLockIcon.Size = new Size(24, 24);
            picLockIcon.BackColor = Color.Transparent;
            picLockIcon.ForeColor = Color.FromArgb(160, 160, 170);

            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 10f);
            txtPassword.Location = new Point(42, 11);
            txtPassword.Size = new Size(196, 22);
            txtPassword.BackColor = Color.FromArgb(50, 50, 56);
            txtPassword.ForeColor = Color.White;
            txtPassword.PasswordChar = '●';
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.TabIndex = 1;
            txtPassword.KeyDown += txtPassword_KeyDown;

            btnShowPass.Text = "👁";
            btnShowPass.FlatStyle = FlatStyle.Flat;
            btnShowPass.FlatAppearance.BorderSize = 0;
            btnShowPass.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnShowPass.Location = new Point(246, 8);
            btnShowPass.Size = new Size(28, 26);
            btnShowPass.BackColor = Color.Transparent;
            btnShowPass.ForeColor = Color.FromArgb(160, 160, 170);
            btnShowPass.TabStop = false;
            btnShowPass.Click += btnShowPassword_Click;

            // ── Remember me ────────────────────────────────────
            chkRemember.Text = "Remember me";
            chkRemember.Font = new Font("Segoe UI", 9f);
            chkRemember.ForeColor = Color.FromArgb(160, 160, 170);
            chkRemember.Location = new Point(30, 330);
            chkRemember.AutoSize = true;
            chkRemember.TabIndex = 2;

            // ── Sign in button ─────────────────────────────────
            btnLogin.Text = "Sign In";
            btnLogin.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(37, 99, 180);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Location = new Point(30, 370);
            btnLogin.Size = new Size(280, 44);
            btnLogin.TabIndex = 3;
            btnLogin.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 280, 44, 8, 8));
            btnLogin.Click += btnLogin_Click;

            this.Controls.Add(pnlMain);

            pnlMain.ResumeLayout(false);
            pnlUserInput.ResumeLayout(false);
            pnlPassInput.ResumeLayout(false);
            ResumeLayout(false);
        }

        // Rounded corners helper
        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

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