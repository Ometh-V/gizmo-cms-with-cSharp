using System;
using System.Drawing;
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
        private Label _lblBrand;
        private TextBox _txtSearch;
        private Button _btnAddContact;

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

            // docked panels
            BuildShell();         
            BuildHeader();        
            BuildSidebar();       

            _navManager = new NavigationManager(_contentPanel);
            SetActiveNav(_btnDashboard);
            _navManager.NavigateTo<DashboardView>();   
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
                Height = 65,
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
            // Brand label (left)
            _lblBrand = new Label
            {
                Text = "ContactManager",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 19)
            };

            // Search box (centre)
            
            _txtSearch = new TextBox
            {
                Width = 320,
                Font = new Font("Segoe UI", 10f),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextSecondary,
                BorderStyle = BorderStyle.FixedSingle,
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
                    _txtSearch.ForeColor = AppColors.TextSecondary;
                }
            };

            // Add contact button (right)
            _btnAddContact = new Button
            {
                Text = "  + Add contact",
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppColors.TextPrimary,
                BackColor = AppColors.SurfaceLight,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 36),
                Cursor = Cursors.Hand
            };
            _btnAddContact.FlatAppearance.BorderColor = AppColors.Border;
            _btnAddContact.FlatAppearance.BorderSize = 1;
            _btnAddContact.FlatAppearance.MouseOverBackColor = AppColors.NavHover;

            // Re-centre controls whene the header is resized
            _headerPanel.Resize += (s, e) => RepositionHeader();
            _headerPanel.Controls.AddRange(new Control[] { _lblBrand, _txtSearch, _btnAddContact });
            RepositionHeader();
        }

        private void RepositionHeader()
        {
            // centre everything in the header height Vertically
            int mid = (_headerPanel.Height - 36) / 2;
            _txtSearch.Location = new Point((_headerPanel.Width - _txtSearch.Width) / 2, mid + 2);
            _btnAddContact.Location = new Point(_headerPanel.Width - _btnAddContact.Width - 24, mid);
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