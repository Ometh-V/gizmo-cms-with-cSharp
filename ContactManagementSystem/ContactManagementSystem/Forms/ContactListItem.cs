using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using ContactManagementSystem.Helpers; // Required for AppColors

namespace ContactManagementSystem.Forms
{
    public partial class ContactListItem : UserControl
    {
        private Panel _pnlAvatar = null!;
        private Label _lblName = null!;
        private Label _lblEmail = null!;
        private string _initials = "??";

        // Properties expected by AllContactsView
        public int ContactId { get; private set; }
        public Color AvatarColor { get; private set; } = Color.SteelBlue;

        public ContactListItem()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Size = new Size(350, 70);
            this.BackColor = AppColors.Surface;
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0, 0, 0, 1);

            _pnlAvatar = new Panel { Size = new Size(40, 40), Location = new Point(15, 15), BackColor = Color.Transparent };
            _pnlAvatar.Paint += DrawCircularAvatar;

            _lblName = new Label { Text = "Name", ForeColor = AppColors.TextPrimary, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(70, 12), AutoSize = true };
            _lblEmail = new Label { Text = "email", ForeColor = AppColors.TextSecondary, Font = new Font("Segoe UI", 9, FontStyle.Regular), Location = new Point(70, 35), AutoSize = true };

            this.Controls.Add(_pnlAvatar);
            this.Controls.Add(_lblName);
            this.Controls.Add(_lblEmail);

            // Pass clicks through to the main control
            _lblName.Click += (s, e) => this.OnClick(e);
            _lblEmail.Click += (s, e) => this.OnClick(e);
            _pnlAvatar.Click += (s, e) => this.OnClick(e);
        }

        /// <summary>
        /// Feeds database information into the visual labels
        /// </summary>
        public void SetData(int id, string name, string email, Color avatarColor)
        {
            ContactId = id;
            _lblName.Text = name;
            _lblEmail.Text = email;
            AvatarColor = avatarColor;

            // Extract the first letter of the first and last name for the avatar
            string[] parts = name.Trim().Split(' ');
            _initials = parts.Length >= 2
                ? $"{parts}{parts}".ToUpper()
                : $"{name}".ToUpper();

            _pnlAvatar.Invalidate(); // Forces the circle to redraw
        }

        /// <summary>
        /// Visually highlights the row when clicked
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            this.BackColor = isSelected ? AppColors.NavActive : AppColors.Surface;
        }

        private void DrawCircularAvatar(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, _pnlAvatar.Width - 1, _pnlAvatar.Height - 1);
            using (SolidBrush brush = new SolidBrush(this.AvatarColor))
            {
                g.FillEllipse(brush, rect);
            }

            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(_initials, font, textBrush, rect, sf);
            }
        }
    }
}