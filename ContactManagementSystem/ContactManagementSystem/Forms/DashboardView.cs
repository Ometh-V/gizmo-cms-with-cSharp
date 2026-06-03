using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Forms
{
    public partial class DashboardView : UserControl, INavigationAware
    {
        public DashboardView()
        {
            
            this.BackColor = AppColors.Background;
            this.DoubleBuffered = true;
            BuildUI();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(40, 36)
            };

            var sub = new Label
            {
                Text = "Welcome back to ContactManager",
                Font = new Font("Segoe UI", 11f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(40, 84)
            };

            this.Controls.AddRange(new Control[] { title, sub });

            
            AddStatCard("Total Contacts", "0", AppColors.Primary, new Point(40, 120));
            AddStatCard("Groups", "0", AppColors.AvatarColors[1], new Point(240, 120));
            AddStatCard("Favourites", "0", AppColors.AvatarColors[2], new Point(440, 120));
        }

        private void AddStatCard(string label, string value, Color accent, Point location)
        {
            var card = new Panel
            {
                Size = new Size(170, 90),
                Location = location,
                BackColor = AppColors.Surface
            };

            // Left accent bar drawn via Paint event
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 4, card.Height);
            };

            var valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(18, 14)
            };

            var nameLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(18, 58)
            };

            card.Controls.Add(valLabel);
            card.Controls.Add(nameLabel);
            this.Controls.Add(card);
        }


        public void OnNavigatedTo()
        {
            
        }
    }
}