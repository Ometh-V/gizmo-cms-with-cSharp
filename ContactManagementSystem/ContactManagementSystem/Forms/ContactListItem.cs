using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Forms
{
    public partial class ContactListItem : UserControl
    {
        private Panel _pnlAvatar = null!;
        private Label _lblName = null!;
        private Label _lblEmail = null!;
        private CheckBox _chkSelect = null!; // Grouped up here for consistency
        private string _initials = "??";

        public int ContactId { get; private set; }
        public Color AvatarColor { get; private set; } = Color.SteelBlue;

        // REQUIRED FOR BULK DELETE: Allows the main form to check if this item is ticked
        public bool IsSelected => _chkSelect.Checked;

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

            _chkSelect = new CheckBox
            {
                Size = new Size(20, 20),
                Location = new Point(10, (this.Height - 20) / 2),
                Visible = false,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            this.Controls.Add(_chkSelect);
            this.Controls.Add(_pnlAvatar);
            this.Controls.Add(_lblName);
            this.Controls.Add(_lblEmail);

            _lblName.Click += (s, e) => this.OnClick(e);
            _lblEmail.Click += (s, e) => this.OnClick(e);
            _pnlAvatar.Click += (s, e) => this.OnClick(e);
        }

        // REQUIRED FOR BULK DELETE: Toggles visibility and safely shifts the layout
        public void ToggleSelectionMode(bool isSelecting)
        {
            _chkSelect.Visible = isSelecting;
            _chkSelect.Checked = false; // Always uncheck when turning mode on/off

            // Dynamically shift the UI to the right so it doesn't overlap the CheckBox
            int shift = isSelecting ? 30 : 0;
            _pnlAvatar.Location = new Point(15 + shift, 15);
            _lblName.Location = new Point(70 + shift, 12);
            _lblEmail.Location = new Point(70 + shift, 35);
        }

        public void SetData(int id, string name, string email, Color avatarColor)
        {
            ContactId = id;
            _lblName.Text = name;
            _lblEmail.Text = email;
            AvatarColor = avatarColor;

            // FIXED: Grabbing the actual first letters instead of calling .ToString() on an array
            string[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                _initials = $"{parts}{parts}".ToUpper();
            }
            else if (name.Length > 0)
            {
                _initials = name.ToString().ToUpper();
            }
            else
            {
                _initials = "??";
            }

            _pnlAvatar.Invalidate();
        }

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