using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Forms
{
    public partial class ContactListItem : UserControl
    {
        // ── Private state ─────────────────────────────────────────────────────
        private string _name = "";
        private string _email = "";
        private Color _avatarColor = AppColors.Primary;

        // ── Public properties ─────────────────────────────────────────────────
        public int ContactId { get; private set; }
        public bool IsSelected { get; private set; }

        public ContactListItem()
        {
            InitializeComponent();
            this.Height = 65;
            this.Dock = DockStyle.Top;
            this.BackColor = AppColors.Surface;
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;   // Prevents flicker when the control repaints

            // Hover highlight — only when not already selected
            this.MouseEnter += (s, e) => { if (!IsSelected) { BackColor = AppColors.ListHover; Invalidate(); } };
            this.MouseLeave += (s, e) => { if (!IsSelected) { BackColor = AppColors.Surface; Invalidate(); } };

            // All visual output is produced in one place: OnPaint
            this.Paint += OnPaint;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Fills the control with contact data and requests a repaint.</summary>
        public void SetData(int id, string name, string email, Color avatarColor)
        {
            ContactId = id;
            _name = name;
            _email = email;
            _avatarColor = avatarColor;
            this.Invalidate();   // Tells WinForms to call OnPaint with the new data
        }

        /// <summary>Switches the item between its selected and normal visual state.</summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            this.BackColor = selected ? AppColors.ListSelected : AppColors.Surface;
            this.Invalidate();
        }

        // ── Rendering ─────────────────────────────────────────────────────────
        // Everything drawn here — no child Label/PictureBox controls needed.
        // GDI+ gives pixel-perfect control over every element.

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // ── Avatar circle ─────────────────────────────────────────────────
            const int cSize = 40;
            const int cX = 14;
            int cY = (this.Height - cSize) / 2;

            using (var brush = new SolidBrush(_avatarColor))
                g.FillEllipse(brush, cX, cY, cSize, cSize);

            // ── Initials centred inside circle ────────────────────────────────
            string initials = GetInitials(_name);
            using (var font = new Font("Segoe UI", 12f, FontStyle.Bold))
            {
                SizeF sz = g.MeasureString(initials, font);
                g.DrawString(initials, font, Brushes.White,
                    cX + (cSize - sz.Width) / 2,
                    cY + (cSize - sz.Height) / 2);
            }

            // ── Name — upper text line ────────────────────────────────────────
            int textX = cX + cSize + 14;
            using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var brush = new SolidBrush(AppColors.TextPrimary))
                g.DrawString(_name, font, brush, textX, this.Height / 2 - 17);

            // ── Email — lower text line ───────────────────────────────────────
            using (var font = new Font("Segoe UI", 9f))
            using (var brush = new SolidBrush(AppColors.TextSecondary))
                g.DrawString(_email, font, brush, textX, this.Height / 2 + 1);

            // ── Bottom separator ──────────────────────────────────────────────
            using (var pen = new Pen(AppColors.Border, 1))
                g.DrawLine(pen, 0, this.Height - 1, this.Width, this.Height - 1);
        }

        // "Ashan Kumara" → "AK", "Nimal" → "N"
        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] parts = name.Trim().Split(' ');
            return parts.Length >= 2
                ? string.Format("{0}{1}", parts[0][0], parts[1][0]).ToUpper()
                : name[0].ToString().ToUpper();
        }
    }
}