#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class DashboardView : UserControl, INavigationAware
    {
        // Order: 0=Total, 1=Customers, 2=Suppliers, 3=Recent,
        //        4=Newbie, 5=Regular, 6=VIP, 7=Groups 
        private readonly List<Label> _statValues = new List<Label>();
        private Label _lblWelcome;

        // ──  recently added panel ───────────────────────────
        private Panel _recentPanel;
        private FlowLayoutPanel _recentFlow;

        public DashboardView()
        {
            this.BackColor = AppColors.Background;
            this.DoubleBuffered = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Add(new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(40, 36)
            });

            _lblWelcome = new Label
            {
                Text = "Welcome back to ContactManager",
                Font = new Font("Segoe UI", 11f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(40, 82)
            };
            this.Controls.Add(_lblWelcome);

            // ── Row 1: contact counts ────────────────────────────────────────
            AddStatCard("Total Contacts", AppColors.Primary, new Point(40, 130));
            AddStatCard("Customers", AppColors.AvatarColors[1], new Point(230, 130));
            AddStatCard("Suppliers", AppColors.AvatarColors[2], new Point(420, 130));
            AddStatCard("Added This Month", AppColors.AvatarColors[4], new Point(610, 130));

            // ── Row 2: loyalty tier breakdown + groups  ──────────
            AddStatCard("Newbie", Color.FromArgb(100, 180, 100), new Point(40, 240));
            AddStatCard("Regular", Color.FromArgb(250, 200, 50), new Point(230, 240));
            AddStatCard("VIP", Color.FromArgb(100, 160, 255), new Point(420, 240));
            AddStatCard("Groups", Color.FromArgb(120, 100, 220), new Point(610, 240));

            // ── Row 3: recently added contacts panel ─────────────────
            BuildRecentPanel();
        }

        private void AddStatCard(string label, Color accent, Point location)
        {
            var card = new Panel
            {
                Size = new Size(170, 90),
                Location = location,
                BackColor = AppColors.Surface
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 4, card.Height);
            };

            var valLabel = new Label
            {
                Text = "…",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(18, 14)
            };

            _statValues.Add(valLabel);

            card.Controls.Add(valLabel);
            card.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(18, 58)
            });

            this.Controls.Add(card);
        }

        // ──  builds the "Recently added" card with its own list ────────
        private void BuildRecentPanel()
        {
            _recentPanel = new Panel
            {
                Location = new Point(40, 350),
                Size = new Size(740, 230),
                BackColor = AppColors.Surface
            };
            _recentPanel.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(AppColors.Border, 1),
                    0, 0, _recentPanel.Width - 1, _recentPanel.Height - 1);

            _recentPanel.Controls.Add(new Label
            {
                Text = "Recently added",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(18, 14),
                BackColor = Color.Transparent
            });

            _recentFlow = new FlowLayoutPanel
            {
                Location = new Point(0, 46),
                Size = new Size(740, 180),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = AppColors.Surface
            };
            _recentPanel.Controls.Add(_recentFlow);

            this.Controls.Add(_recentPanel);
        }

        // ──  builds one row inside the recently added list ─────────────
        private void AddRecentRow(Contact c)
        {
            var row = new Panel
            {
                Width = 720,
                Height = 44,
                BackColor = AppColors.Surface
            };

            Color avColor = AppColors.AvatarColors[c.ContactID % AppColors.AvatarColors.Length];
            var avatar = new Panel
            {
                Size = new Size(32, 32),
                Location = new Point(18, 6),
                BackColor = Color.Transparent
            };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(avColor))
                    e.Graphics.FillEllipse(b, 0, 0, 32, 32);
                using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    var sz = e.Graphics.MeasureString(c.Initials, f);
                    e.Graphics.DrawString(c.Initials, f, Brushes.White,
                        (32 - sz.Width) / 2, (32 - sz.Height) / 2);
                }
            };
            row.Controls.Add(avatar);

            row.Controls.Add(new Label
            {
                Text = c.FullName,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(62, 8),
                BackColor = Color.Transparent
            });

            row.Controls.Add(new Label
            {
                Text = c.ContactType,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(62, 24),
                BackColor = Color.Transparent
            });

            row.Controls.Add(new Label
            {
                Text = TimeAgo(c.CreatedAt),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(610, 14),
                BackColor = Color.Transparent
            });

            _recentFlow.Controls.Add(row);
        }

        // ──  formats a date as "2 days ago", "Just now" etc. ───────────
        private static string TimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} hr ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} day(s) ago";
            return date.ToString("dd MMM yyyy");
        }

        // Called every time the user navigates here — pulls live DB counts
        public void OnNavigatedTo()
        {
            try
            {
                _statValues[0].Text = ContactService.GetTotalCount().ToString();
                _statValues[1].Text = ContactService.GetCustomerCount().ToString();
                _statValues[2].Text = ContactService.GetSupplierCount().ToString();
                _statValues[3].Text = ContactService.GetRecentCount().ToString();

                var (newbie, regular, vip) = LoyaltyService.GetTierCounts();
                _statValues[4].Text = newbie.ToString();
                _statValues[5].Text = regular.ToString();
                _statValues[6].Text = vip.ToString();

                // ──  groups count ───────────────────────────
                _statValues[7].Text = GroupService.GetCount().ToString();

                _lblWelcome.Text = $"Welcome back, {Session.UserName}!";

                // ──  load recently added contacts ───────────
                _recentFlow.Controls.Clear();
                var recent = ContactService.GetRecentlyAdded(5);

                if (recent.Count == 0)
                {
                    _recentFlow.Controls.Add(new Label
                    {
                        Text = "No contacts added yet.",
                        Font = new Font("Segoe UI", 9.5f),
                        ForeColor = AppColors.TextSecondary,
                        AutoSize = true,
                        Location = new Point(18, 10)
                    });
                }
                else
                {
                    foreach (var c in recent)
                        AddRecentRow(c);
                }
            }
            catch (Exception ex)
            {
                foreach (var lbl in _statValues)
                    lbl.Text = "—";

                // ── surface the actual error instead of
                // silently failing, so connection issues are visible
                // during testing instead of just showing dashes.
                MessageBox.Show(
                    $"Failed to load dashboard data.\n\n{ex.Message}",
                    "Dashboard Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}