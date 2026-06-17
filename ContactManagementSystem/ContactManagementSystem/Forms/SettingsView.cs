#nullable disable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class SettingsView : UserControl, INavigationAware
    {
        // ── Sub-navigation (General / Account / Database / About) ───────────
        private Button _tabGeneral;
        private Button _tabAccount;
        private Button _tabDatabase;
        private Button _tabAbout;
        private Button _activeTab;

        private Panel _pnlGeneral;
        private Panel _pnlAccount;
        private Panel _pnlDatabase;
        private Panel _pnlAbout;

        // ── Preferences (loaded from local JSON file) ────────────────────────
        private AppPreferences _prefs;

        // ── General tab fields ───────────────────────────────────────────────
        private ComboBox _cmbLandingPage;
        private ComboBox _cmbSortOrder;
        private CheckBox _chkConfirmDelete;
        private Label _lblGeneralStatus;

        // ── Account tab fields ───────────────────────────────────────────────
        private Label _lblUsername;
        private Panel _badgeRole;
        private TextBox _txtCurrentPwd;
        private TextBox _txtNewPwd;
        private TextBox _txtConfirmPwd;
        private Label _lblPwdStatus;

        // ── Database tab fields ──────────────────────────────────────────────
        private Label _lblDbStatusDot;
        private Label _lblDbStatusText;

        public SettingsView()
        {
            this.BackColor = AppColors.Background;
            this.DoubleBuffered = true;
            _prefs = PreferencesService.Load();
            BuildUI();
        }

        // Always land back on Account tab when navigating here, refresh
        // preferences from disk (in case changed elsewhere) and re-read
        // the username/role in case it changed.
        public void OnNavigatedTo()
        {
            _prefs = PreferencesService.Load();
            ShowTab(_tabAccount, _pnlAccount);
            RefreshAccountInfo();
        }

        // ════════════════════════════════════════════════════════════════════
        // LAYOUT
        // ════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Controls.Add(new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(40, 36)
            });

            this.Controls.Add(new Label
            {
                Text = "Manage your account, preferences, and view system information",
                Font = new Font("Segoe UI", 11f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(40, 82)
            });

            BuildSubNav();

            _pnlGeneral = BuildGeneralPanel();
            _pnlAccount = BuildAccountPanel();
            _pnlDatabase = BuildDatabasePanel();
            _pnlAbout = BuildAboutPanel();

            this.Controls.Add(_pnlGeneral);
            this.Controls.Add(_pnlAccount);
            this.Controls.Add(_pnlDatabase);
            this.Controls.Add(_pnlAbout);

            RefreshGeneralTab();
            ShowTab(_tabAccount, _pnlAccount);
        }

        // ── Sub-nav tab strip ───────────────────────────────────────────────
        private void BuildSubNav()
        {
            _tabGeneral = MakeTabButton("General", new Point(40, 130));
            _tabAccount = MakeTabButton("Account", new Point(160, 130));
            _tabDatabase = MakeTabButton("Database", new Point(280, 130));
            _tabAbout = MakeTabButton("About", new Point(400, 130));

            _tabGeneral.Click += (s, e) => ShowTab(_tabGeneral, _pnlGeneral);
            _tabAccount.Click += (s, e) => ShowTab(_tabAccount, _pnlAccount);
            _tabDatabase.Click += (s, e) => ShowTab(_tabDatabase, _pnlDatabase);
            _tabAbout.Click += (s, e) => ShowTab(_tabAbout, _pnlAbout);

            this.Controls.Add(_tabGeneral);
            this.Controls.Add(_tabAccount);
            this.Controls.Add(_tabDatabase);
            this.Controls.Add(_tabAbout);

            this.Controls.Add(new Panel
            {
                BackColor = AppColors.Border,
                Size = new Size(900, 1),
                Location = new Point(40, 168)
            });
        }

        private Button MakeTabButton(string text, Point location)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(110, 36),
                Location = location,
                BackColor = Color.Transparent,
                ForeColor = AppColors.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void ShowTab(Button tabButton, Panel panel)
        {
            _pnlGeneral.Visible = panel == _pnlGeneral;
            _pnlAccount.Visible = panel == _pnlAccount;
            _pnlDatabase.Visible = panel == _pnlDatabase;
            _pnlAbout.Visible = panel == _pnlAbout;

            foreach (var btn in new[] { _tabGeneral, _tabAccount, _tabDatabase, _tabAbout })
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = AppColors.TextSecondary;
            }
            tabButton.BackColor = AppColors.NavActive;
            tabButton.ForeColor = AppColors.TextPrimary;
            _activeTab = tabButton;

            if (panel == _pnlDatabase) RefreshDatabaseStatus();
            if (panel == _pnlGeneral) RefreshGeneralTab();
        }

        // ════════════════════════════════════════════════════════════════════
        // GENERAL TAB
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildGeneralPanel()
        {
            var panel = new Panel { Location = new Point(40, 190), Size = new Size(900, 500), BackColor = AppColors.Background, Visible = false };

            var card = MakeCard(new Point(0, 0), new Size(460, 300));

            card.Controls.Add(new Label
            {
                Text = "General Preferences",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16)
            });

            card.Controls.Add(new Label
            {
                Text = "DEFAULT LANDING PAGE",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 52)
            });
            _cmbLandingPage = new ComboBox
            {
                Location = new Point(16, 70),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            _cmbLandingPage.Items.AddRange(new object[] { "Dashboard", "All Contacts" });
            card.Controls.Add(_cmbLandingPage);

            card.Controls.Add(new Label
            {
                Text = "DEFAULT CONTACT SORT ORDER",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 110)
            });
            _cmbSortOrder = new ComboBox
            {
                Location = new Point(16, 128),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            _cmbSortOrder.Items.AddRange(new object[] { "A–Z", "Z–A", "Recently Added" });
            card.Controls.Add(_cmbSortOrder);

            _chkConfirmDelete = new CheckBox
            {
                Text = "Ask for confirmation before deleting a contact",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 172),
                FlatStyle = FlatStyle.Flat
            };
            card.Controls.Add(_chkConfirmDelete);

            var btnSave = new Button
            {
                Text = "Save Preferences",
                Size = new Size(160, 38),
                Location = new Point(16, 210),
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSaveGeneral_Click;
            card.Controls.Add(btnSave);

            _lblGeneralStatus = new Label
            {
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(16, 256),
                MaximumSize = new Size(420, 0)
            };
            card.Controls.Add(_lblGeneralStatus);

            panel.Controls.Add(card);
            return panel;
        }

        private void RefreshGeneralTab()
        {
            _cmbLandingPage.SelectedItem = _prefs.DefaultLandingPage == "AllContacts" ? "All Contacts" : "Dashboard";

            _cmbSortOrder.SelectedItem = _prefs.DefaultSortOrder switch
            {
                "Z-A" => "Z–A",
                "Recent" => "Recently Added",
                _ => "A–Z"
            };

            _chkConfirmDelete.Checked = _prefs.ConfirmBeforeDelete;
            _lblGeneralStatus.Text = "";
        }

        private void BtnSaveGeneral_Click(object sender, EventArgs e)
        {
            _prefs.DefaultLandingPage = _cmbLandingPage.SelectedItem?.ToString() == "All Contacts" ? "AllContacts" : "Dashboard";

            _prefs.DefaultSortOrder = _cmbSortOrder.SelectedItem?.ToString() switch
            {
                "Z–A" => "Z-A",
                "Recently Added" => "Recent",
                _ => "A-Z"
            };

            _prefs.ConfirmBeforeDelete = _chkConfirmDelete.Checked;

            try
            {
                PreferencesService.Save(_prefs);
                _lblGeneralStatus.Text = "Preferences saved.";
                _lblGeneralStatus.ForeColor = Color.FromArgb(80, 200, 120);
            }
            catch (Exception ex)
            {
                _lblGeneralStatus.Text = $"Failed to save preferences: {ex.Message}";
                _lblGeneralStatus.ForeColor = Color.FromArgb(220, 80, 80);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ACCOUNT TAB
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildAccountPanel()
        {
            var panel = new Panel
            {
                Location = new Point(40, 190),
                Size = new Size(900, 500),
                BackColor = AppColors.Background,
                Visible = false
            };

            var profileCard = MakeCard(new Point(0, 0), new Size(420, 110));

            var avatar = new Panel { Size = new Size(56, 56), Location = new Point(16, 27) };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var b = new SolidBrush(AppColors.Primary);
                e.Graphics.FillEllipse(b, 0, 0, 56, 56);
                string initial = string.IsNullOrEmpty(Session.UserName) ? "?" : Session.UserName[0].ToString().ToUpper();
                using var f = new Font("Segoe UI", 18f, FontStyle.Bold);
                SizeF sz = e.Graphics.MeasureString(initial, f);
                e.Graphics.DrawString(initial, f, Brushes.White, (56 - sz.Width) / 2, (56 - sz.Height) / 2);
            };
            profileCard.Controls.Add(avatar);

            _lblUsername = new Label
            {
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(86, 32)
            };
            profileCard.Controls.Add(_lblUsername);

            _badgeRole = new Panel { Size = new Size(70, 22), Location = new Point(86, 60) };
            _badgeRole.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color c = Session.IsAdmin ? Color.FromArgb(100, 60, 160) : Color.FromArgb(37, 99, 180);
                using var path = RoundedRect(new Rectangle(0, 0, _badgeRole.Width, _badgeRole.Height), 10);
                using var brush = new SolidBrush(c);
                e.Graphics.FillPath(brush, path);
                string text = Session.Role;
                using var f = new Font("Segoe UI", 8.5f);
                SizeF sz = e.Graphics.MeasureString(text, f);
                e.Graphics.DrawString(text, f, Brushes.White,
                    (_badgeRole.Width - sz.Width) / 2, (_badgeRole.Height - sz.Height) / 2);
            };
            profileCard.Controls.Add(_badgeRole);

            panel.Controls.Add(profileCard);

            var pwdCard = MakeCard(new Point(0, 130), new Size(420, 320));

            pwdCard.Controls.Add(new Label
            {
                Text = "Change Password",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16)
            });

            _txtCurrentPwd = MakePasswordField(pwdCard, "Current Password", 50);
            _txtNewPwd = MakePasswordField(pwdCard, "New Password", 124);
            _txtConfirmPwd = MakePasswordField(pwdCard, "Confirm New Password", 198);

            _lblPwdStatus = new Label
            {
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(16, 248),
                MaximumSize = new Size(388, 0)
            };
            pwdCard.Controls.Add(_lblPwdStatus);

            var btnSavePwd = new Button
            {
                Text = "Update Password",
                Size = new Size(160, 36),
                Location = new Point(16, 272),
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f)
            };
            btnSavePwd.FlatAppearance.BorderSize = 0;
            btnSavePwd.Click += BtnSavePassword_Click;
            pwdCard.Controls.Add(btnSavePwd);

            panel.Controls.Add(pwdCard);
            return panel;
        }

        private TextBox MakePasswordField(Panel parent, string label, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, y)
            });

            var txt = new TextBox
            {
                Location = new Point(16, y + 18),
                Width = 388,
                Height = 30,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 9.5f)
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private void RefreshAccountInfo()
        {
            _lblUsername.Text = Session.UserName;
            _badgeRole.Invalidate();

            _txtCurrentPwd.Text = "";
            _txtNewPwd.Text = "";
            _txtConfirmPwd.Text = "";
            _lblPwdStatus.Text = "";
        }

        private void BtnSavePassword_Click(object sender, EventArgs e)
        {
            string current = _txtCurrentPwd.Text;
            string newPwd = _txtNewPwd.Text;
            string confirm = _txtConfirmPwd.Text;

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirm))
            {
                ShowPwdStatus("All fields are required.", false);
                return;
            }

            if (newPwd.Length < 6)
            {
                ShowPwdStatus("New password must be at least 6 characters.", false);
                return;
            }

            if (newPwd != confirm)
            {
                ShowPwdStatus("New password and confirmation do not match.", false);
                return;
            }

            try
            {
                if (!UserService.VerifyPassword(Session.UserID, current))
                {
                    ShowPwdStatus("Current password is incorrect.", false);
                    return;
                }

                UserService.ResetPassword(Session.UserID, newPwd);
                ShowPwdStatus("Password updated successfully.", true);

                _txtCurrentPwd.Text = "";
                _txtNewPwd.Text = "";
                _txtConfirmPwd.Text = "";
            }
            catch (Exception ex)
            {
                ShowPwdStatus($"Failed to update password: {ex.Message}", false);
            }
        }

        private void ShowPwdStatus(string message, bool success)
        {
            _lblPwdStatus.Text = message;
            _lblPwdStatus.ForeColor = success ? Color.FromArgb(80, 200, 120) : Color.FromArgb(220, 80, 80);
        }

        // ════════════════════════════════════════════════════════════════════
        // DATABASE TAB
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildDatabasePanel()
        {
            var panel = new Panel { Location = new Point(40, 190), Size = new Size(900, 500), BackColor = AppColors.Background, Visible = false };

            var card = MakeCard(new Point(0, 0), new Size(420, 260));

            card.Controls.Add(new Label
            {
                Text = "Database Connection",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16)
            });

            AddInfoRow(card, "SERVER", AppConfig.DbServer, 50);
            AddInfoRow(card, "DATABASE", AppConfig.DbName, 100);

            bool windowsAuth = string.IsNullOrEmpty(AppConfig.DbUser);
            AddInfoRow(card, "AUTHENTICATION",
                windowsAuth ? "Windows Authentication" : $"SQL Server Authentication ({AppConfig.DbUser})", 150);

            _lblDbStatusDot = new Label
            {
                Text = "●",
                Font = new Font("Segoe UI", 12f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 204)
            };
            _lblDbStatusText = new Label
            {
                Text = "Not tested",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(34, 206)
            };
            card.Controls.Add(_lblDbStatusDot);
            card.Controls.Add(_lblDbStatusText);

            var btnTest = new Button
            {
                Text = "Test Connection",
                Size = new Size(150, 32),
                Location = new Point(254, 200),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f)
            };
            btnTest.FlatAppearance.BorderColor = AppColors.Border;
            btnTest.FlatAppearance.BorderSize = 1;
            btnTest.Click += (s, e) => RefreshDatabaseStatus();
            card.Controls.Add(btnTest);

            panel.Controls.Add(card);
            return panel;
        }

        private void AddInfoRow(Panel parent, string label, string value, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, y)
            });
            parent.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, y + 18)
            });
        }

        private void RefreshDatabaseStatus()
        {
            // Temporarily change text so the user knows it's thinking
            _lblDbStatusText.Text = "Testing connection...";
            _lblDbStatusText.ForeColor = AppColors.TextSecondary;
            _lblDbStatusDot.ForeColor = AppColors.TextSecondary;
            Application.DoEvents(); // Force UI to update immediately

            try
            {
                using var conn = DatabaseHelper.GetConnection();

                // Check the state before trying to open it!
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                // 1. Update the UI labels
                _lblDbStatusDot.ForeColor = Color.FromArgb(80, 200, 120);
                _lblDbStatusText.Text = "Connected";
                _lblDbStatusText.ForeColor = Color.FromArgb(80, 200, 120);

                // 2. Show the success popup
                MessageBox.Show("Database connection successfully established!",
                                "Connection Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // 1. Update the UI labels
                _lblDbStatusDot.ForeColor = Color.FromArgb(220, 80, 80);
                _lblDbStatusText.Text = "Connection failed."; // Shortened to fit the UI better
                _lblDbStatusText.ForeColor = Color.FromArgb(220, 80, 80);

                // 2. Show the detailed error popup
                MessageBox.Show($"Failed to connect to the database.\n\n{ex.Message}",
                                "Connection Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ABOUT TAB
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildAboutPanel()
        {
            var panel = new Panel { Location = new Point(40, 190), Size = new Size(900, 500), BackColor = AppColors.Background, Visible = false };

            var card = MakeCard(new Point(0, 0), new Size(500, 320));

            card.Controls.Add(new Label
            {
                Text = "ContactManager",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16)
            });
            card.Controls.Add(new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 46)
            });

            card.Controls.Add(new Panel { BackColor = AppColors.Border, Size = new Size(468, 1), Location = new Point(16, 78) });

            card.Controls.Add(new Label
            {
                Text = "Built with .NET 10, WinForms, and SQL Server.\nDeveloped as a VAP (Visual Application Programming) project.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 92)
            });

            card.Controls.Add(new Label
            {
                Text = "Quick Tips",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 150)
            });
            card.Controls.Add(new Label
            {
                Text =
                    "•  Use All Contacts to browse, search, and manage your contact list.\n" +
                    "•  Group related contacts together under Groups.\n" +
                    "•  Star a contact to add it to Favourites.\n" +
                    "•  Admins can Import/Export contacts and manage user accounts.\n" +
                    "•  Adjust default behavior under the General tab.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 176)
            });

            panel.Controls.Add(card);
            return panel;
        }

        // ── Shared helpers ────────────────────────────────────────────────────
        private Panel MakeCard(Point location, Size size)
        {
            var panel = new Panel { Location = location, Size = size, BackColor = AppColors.Surface };
            panel.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(AppColors.Border, 1), 0, 0, panel.Width - 1, panel.Height - 1);
            return panel;
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d = r * 2;
            var path = new GraphicsPath();
            path.AddArc(b.X, b.Y, d, d, 180, 90);
            path.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            path.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            path.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}