using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Forms
{
    public partial class MainForm : Form
    {
        // pannels
        private Panel _headerPanel;
        private Panel _sidebarPanel;
        private Panel _contentPanel;

        // Header controls
        private Panel _headerTitleBar;
        private Label _lblBrand;
        private Label _lblUser;
        private TextBox _txtSearch;
        private Button _btnAddContact;
        private Button _btnAddUser;
        private Button _btnMinimize;
        private Button _btnMaximize;
        private Button _btnClose;

        // Sidebar nav buttons
        private Button _btnDashboard;
        private Button _btnAllContacts;
        private Button _btnGroups;
        private Button _btnFavourites;
        private Button _btnImport;
        private Button _btnExport;
        private Button _btnSettings;

        // Tracks which button is highlighted
        private Button _activeNavButton;

        // Navigation
        private NavigationManager _navManager;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public MainForm()
        {
            InitializeComponent();

            // Form properties
            this.Text = "ContactManager";
            this.MinimumSize = new Size(1100, 650);
            this.Size = new Size(1280, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            this.FormBorderStyle = FormBorderStyle.None;

            // docked panels
            BuildShell();
            BuildHeader();
            BuildSidebar();

            _navManager = new NavigationManager(_contentPanel);
            SetActiveNav(_btnDashboard);
            _navManager.NavigateTo<DashboardView>();
        }

        // Windows drag support 
        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }


        // A. SHELL — three docked panels
        private void BuildShell()
        {

            // WinForms docking engine processes controls in REVERSE index order.
            // Fill (index 0) is evaluated last  → takes leftover space.
            // Left (index 1) is evaluated second → claims left edge below header.
            // Top  (index 2) is evaluated first  → claims full top width.

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background
            };

            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 215,
                BackColor = AppColors.Sidebar
            };

            // Right border line on sidebar
            _sidebarPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.Border, 1),
                    _sidebarPanel.Width - 1, 0,
                    _sidebarPanel.Width - 1, _sidebarPanel.Height);

            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = AppColors.Header
            };

            // Bottom border line on header
            _headerPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.Border, 1),
                    0, _headerPanel.Height - 1,
                    _headerPanel.Width, _headerPanel.Height - 1);

            this.Controls.Add(_contentPanel);
            this.Controls.Add(_sidebarPanel);
            this.Controls.Add(_headerPanel);
        }


        // B. HEADER — branding, search box, add-contact button

        private void BuildHeader()
        {
            // ── Row 1: custom title bar (drag + window controls) ──
            _headerTitleBar = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(18, 18, 20)
            };
            _headerTitleBar.MouseDown += Header_MouseDown;

            _lblBrand = new Label
            {
                Text = "ContactManager",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 165),
                AutoSize = true,
                Location = new Point(14, 9),
                BackColor = Color.Transparent
            };
            _lblBrand.MouseDown += Header_MouseDown;

            // Logged in user label
            _lblUser = new Label
            {
                Text = $"👤  {Session.UserName}  ({Session.Role})",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(130, 130, 155),
                AutoSize = true,
                Location = new Point(200, 10),
                BackColor = Color.Transparent
            };

            _btnMinimize = MakeTitleBarButton("─", 0);
            _btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            _btnMaximize = MakeTitleBarButton("□", 1);
            _btnMaximize.Click += (s, e) =>
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;

            _btnClose = MakeTitleBarButton("✕", 2);
            _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            _btnClose.Click += (s, e) => this.Close();

            _headerTitleBar.Controls.AddRange(new Control[]
                { _lblBrand, _lblUser, _btnMinimize, _btnMaximize, _btnClose });

            // Position window buttons on resize
            _headerTitleBar.Resize += (s, e) => RepositionTitleButtons();
            RepositionTitleButtons();

            // ── Row 2: toolbar (search + buttons) ─────────────
            var _toolbar = new Panel
            {
                Height = 64,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(25, 25, 35)
            };
            _toolbar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.Border, 1),
                    0, _toolbar.Height - 1, _toolbar.Width, _toolbar.Height - 1);

            _txtSearch = new TextBox
            {
                Width = 300,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(38, 38, 52),
                ForeColor = Color.FromArgb(130, 130, 155),
                BorderStyle = BorderStyle.None,
                Text = "Search contacts..."
            };
            _txtSearch.GotFocus += (s, e) =>
            {
                if (_txtSearch.Text == "Search contacts...")
                {
                    _txtSearch.Text = "";
                    _txtSearch.ForeColor = AppColors.TextPrimary;
                }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(_txtSearch.Text))
                {
                    _txtSearch.Text = "Search contacts...";
                    _txtSearch.ForeColor = Color.FromArgb(130, 130, 155);
                }
            };

            // Wrap search in a rounded panel
            var pnlSearch = new Panel
            {
                Size = new Size(310, 36),
                BackColor = Color.FromArgb(38, 38, 52)
            };
            pnlSearch.Controls.Add(_txtSearch);
            _txtSearch.Location = new Point(10, 8);
            pnlSearch.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 310, 36, 8, 8));

            // Add Contact button
            _btnAddContact = MakeToolbarButton("+ Add Contact",
                Color.FromArgb(37, 99, 180), Color.White);
            _btnAddContact.Click += (s, e) => OpenAddContactForm();

            // Add User button — only visible to admins
            _btnAddUser = MakeToolbarButton("+ Add User",
                Color.FromArgb(50, 50, 58), AppColors.TextPrimary);
            _btnAddUser.Visible = Session.IsAdmin;
            _btnAddUser.Click += (s, e) => OpenAddUserForm();

            _toolbar.Controls.AddRange(new Control[]
                { pnlSearch, _btnAddContact, _btnAddUser });

            _toolbar.Resize += (s, e) => RepositionToolbar(pnlSearch, _toolbar);

            _headerPanel.Controls.Add(_toolbar);
            _headerPanel.Controls.Add(_headerTitleBar);

            RepositionToolbar(pnlSearch, _toolbar);
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

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

        private Button MakeToolbarButton(string text, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 36),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.1f);
            btn.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 130, 36, 6, 6));
            return btn;
        }

        private void RepositionTitleButtons()
        {
            int w = _headerTitleBar.Width;
            _btnClose.Location = new Point(w - 42, 0);
            _btnMaximize.Location = new Point(w - 84, 0);
            _btnMinimize.Location = new Point(w - 126, 0);
        }

        private void RepositionToolbar(Panel pnlSearch, Panel toolbar)
        {
            int mid = (toolbar.Height - 36) / 2;
            pnlSearch.Location = new Point(20, mid);
            _btnAddUser.Location = new Point(toolbar.Width - 290, mid); // Add User first (left)
            _btnAddContact.Location = new Point(toolbar.Width - 150, mid); // Add Contact second (right)
        }

        private void RepositionHeader()
        {
            // centre everything in the header height Vertically
            int mid = (_headerPanel.Height - 36) / 2;
            _txtSearch.Location = new Point((_headerPanel.Width - _txtSearch.Width) / 2, mid + 2);
            _btnAddContact.Location = new Point(_headerPanel.Width - _btnAddContact.Width - 24, mid);
        }

        private void OpenAddContactForm()
        {
            MessageBox.Show("Add Contact form coming soon!", "Info",
    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenAddUserForm()
        {
            // Admin only — you will implement this
            MessageBox.Show("Add User form coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // C. SIDEBAR — section labels + nav buttons

        private void BuildSidebar()
        {
            int y = 20;

            AddSectionLabel("MENU", ref y);
            _btnDashboard = AddNavButton("⊞  Dashboard", ref y);
            _btnAllContacts = AddNavButton("☰  All contacts", ref y);
            _btnGroups = AddNavButton("⊡  Groups", ref y);
            _btnFavourites = AddNavButton("★  Favourites", ref y);

            y += 20;

            AddSectionLabel("TOOLS", ref y);
            _btnImport = AddNavButton("↓  Import", ref y);
            _btnExport = AddNavButton("↑  Export", ref y);
            _btnSettings = AddNavButton("⚙  Settings", ref y);

            // Wire up navigation 
            _btnDashboard.Click += (s, e) => { SetActiveNav(_btnDashboard); _navManager.NavigateTo<DashboardView>(); };
            _btnAllContacts.Click += (s, e) => { SetActiveNav(_btnAllContacts); _navManager.NavigateTo<AllContactsView>(); };

            // Wire Groups / Favourites 
        }

        private void AddSectionLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(18, y)
            };
            _sidebarPanel.Controls.Add(lbl);
            y += 22;
        }

        private Button AddNavButton(string text, ref int y)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                Size = new Size(200, 40),
                Location = new Point(7, y),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AppColors.NavHover;
            btn.FlatAppearance.MouseDownBackColor = AppColors.NavActive;

            _sidebarPanel.Controls.Add(btn);
            y += 44;
            return btn;
        }

        // Highlights one button and un-highlights the previous one
        private void SetActiveNav(Button btn)
        {
            if (_activeNavButton != null)
                _activeNavButton.BackColor = Color.Transparent;

            _activeNavButton = btn;
            _activeNavButton.BackColor = AppColors.NavActive;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Layout is built in the constructor — nothing needed here.
        }
    }
}