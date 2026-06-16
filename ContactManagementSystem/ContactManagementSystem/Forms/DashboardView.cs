#nullable disable
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class DashboardView : UserControl, INavigationAware
    {
        // Order: 0=Total, 1=Customers, 2=Suppliers, 3=Recent,
        //        4=Newbie, 5=Regular, 6=VIP
        private readonly List<Label> _statValues = new List<Label>();
        private Label _lblWelcome;

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

            // ── Row 2: loyalty tier breakdown ────────────────────────────────
            AddStatCard("Newbie", Color.FromArgb(100, 180, 100), new Point(40, 240));
            AddStatCard("Regular", Color.FromArgb(250, 200, 50), new Point(230, 240));
            AddStatCard("VIP", Color.FromArgb(100, 160, 255), new Point(420, 240));
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

                _lblWelcome.Text = $"Welcome back, {Session.UserName}!";
            }
            catch
            {
                foreach (var lbl in _statValues)
                    lbl.Text = "—";
            }
        }
    }
}