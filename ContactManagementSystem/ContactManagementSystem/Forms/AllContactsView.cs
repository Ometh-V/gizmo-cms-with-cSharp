using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
#nullable disable

namespace ContactManagementSystem.Forms
{
    public partial class AllContactsView : UserControl, INavigationAware
    {
        // ── Layout controls ───────────────────────────────────────────────────
        private SplitContainer _split;
        private FlowLayoutPanel _listFlow;
        private Panel _detailPanel;
        private Label _lblCount;

        // ── Selection state ───────────────────────────────────────────────────
        private ContactListItem _selectedItem;

        // ── Contact data ──────────────────────────────────────────────────────
        private ContactModel[] _contacts;

        public AllContactsView()
        {
            this.BackColor = AppColors.Background;
            this.DoubleBuffered = true;

            // Sample contact data (replace with database call later)
            _contacts = new ContactModel[]
            {
                new ContactModel { Id=1, Name="Ashan Kumara",   Email="ashan@email.com",
                                   Phone="+94 77 123 4567", Address="Colombo 03, Sri Lanka",
                                   DateAdded="29 May 2026", Group="Friends",
                                   AvatarColor=AppColors.AvatarColors[0] },
                new ContactModel { Id=2, Name="Nimal Perera",   Email="nimal@email.com",
                                   Phone="+94 71 234 5678", Address="Kandy, Sri Lanka",
                                   DateAdded="28 May 2026", Group="Work",
                                   AvatarColor=AppColors.AvatarColors[1] },
                new ContactModel { Id=3, Name="Saman Fernando", Email="saman@email.com",
                                   Phone="+94 72 345 6789", Address="Galle, Sri Lanka",
                                   DateAdded="27 May 2026", Group="Family",
                                   AvatarColor=AppColors.AvatarColors[2] },
                new ContactModel { Id=4, Name="Kasun Silva",    Email="kasun@email.com",
                                   Phone="+94 76 456 7890", Address="Negombo, Sri Lanka",
                                   DateAdded="26 May 2026", Group="Friends",
                                   AvatarColor=AppColors.AvatarColors[3] }
            };

            BuildLayout();
        }

        // ════════════════════════════════════════════════════════════════════
        // A. LAYOUT — SplitContainer → Master (left) + Detail (right)
        // ════════════════════════════════════════════════════════════════════
        private void BuildLayout()
        {
            // 1. Pre-size the base UserControl
            this.Size = new Size(1000, 600);

            // 2. Create the SplitContainer and FORCE its size immediately
            _split = new SplitContainer();
            _split.Size = new Size(1000, 600); // This prevents the crash!

            // 3. Now that it is massive, we can safely apply constraints
            _split.Dock = DockStyle.Fill;
            _split.Panel1MinSize = 300;
            _split.Panel2MinSize = 260;
            _split.SplitterDistance = 360; // Safely set this right here
            _split.BackColor = AppColors.Background;
            _split.SplitterWidth = 1;

            _split.Panel1.BackColor = AppColors.Surface;
            _split.Panel2.BackColor = AppColors.Background;

            _split.SplitterMoved += (s, e) => ResizeListItems();

            // 4. Build children and add to the form
            BuildMasterPanel(_split.Panel1);
            BuildDetailPanel(_split.Panel2);

            this.Controls.Add(_split);
        }



        private void BuildMasterPanel(SplitterPanel panel)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = AppColors.Surface
            };
            header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.Border, 1),
                    0, header.Height - 1, header.Width, header.Height - 1);

            _lblCount = new Label
            {
                Text = "All contacts",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 15)
            };
            header.Controls.Add(_lblCount);

            _listFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                BackColor = AppColors.Surface,
                Padding = new Padding(0)
            };
            _listFlow.Resize += (s, e) => ResizeListItems();

            panel.Controls.Add(_listFlow);
            panel.Controls.Add(header);
        }

        private void BuildDetailPanel(SplitterPanel panel)
        {
            _detailPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                AutoScroll = true
            };
            ShowDetailPlaceholder();
            panel.Controls.Add(_detailPanel);
        }

        // ════════════════════════════════════════════════════════════════════
        // B. DATA LOADING  (INavigationAware — called on every navigation)
        // ════════════════════════════════════════════════════════════════════
        public void OnNavigatedTo()
        {
            LoadContacts();
        }

        private void LoadContacts()
        {
            _listFlow.Controls.Clear();
            _selectedItem = null;
            ShowDetailPlaceholder();

            foreach (ContactModel c in _contacts)
            {
                var item = new ContactListItem();
                item.Width = _listFlow.ClientSize.Width;
                item.SetData(c.Id, c.Name, c.Email, c.AvatarColor);
                item.Click += OnContactItemClicked;
                _listFlow.Controls.Add(item);
            }

            _lblCount.Text = string.Format("All contacts — {0} contacts", _contacts.Length);
        }

        private void ResizeListItems()
        {
            int w = _listFlow.ClientSize.Width;
            foreach (ContactListItem item in _listFlow.Controls)
                item.Width = w;
        }

        // ════════════════════════════════════════════════════════════════════
        // C. SELECTION — highlight item, load its detail on the right
        // ════════════════════════════════════════════════════════════════════
        private void OnContactItemClicked(object sender, EventArgs e)
        {
            var item = sender as ContactListItem;
            if (item == null || item == _selectedItem) return;

            _selectedItem?.SetSelected(false);
            _selectedItem = item;
            _selectedItem.SetSelected(true);

            ContactModel contact = FindById(item.ContactId);
            if (contact != null) ShowContactDetail(contact);
        }

        private ContactModel FindById(int id)
        {
            foreach (ContactModel c in _contacts)
                if (c.Id == id) return c;
            return null;
        }

        // ════════════════════════════════════════════════════════════════════
        // D. DETAIL PANEL — rebuilt each time a new contact is selected
        // ════════════════════════════════════════════════════════════════════
        private void ShowDetailPlaceholder()
        {
            _detailPanel.Controls.Clear();
            _detailPanel.Controls.Add(new Label
            {
                Text = "Select a contact\nto view details",
                Font = new Font("Segoe UI", 11f),
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            });
        }

        private void ShowContactDetail(ContactModel c)
        {
            _detailPanel.Controls.Clear();

            int pW = _detailPanel.ClientSize.Width;
            int lX = 24;
            int lW = pW - 48;

            // Avatar circle
            int avSize = 80;
            Color avColor = c.AvatarColor;
            string avInitials = GetInitials(c.Name);

            var avatar = new Panel
            {
                Size = new Size(avSize, avSize),
                Location = new Point((pW - avSize) / 2, 28),
                BackColor = AppColors.Background
            };
            avatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(avColor))
                    g.FillEllipse(b, 0, 0, avSize, avSize);
                using (var f = new Font("Segoe UI", 18f, FontStyle.Bold))
                {
                    SizeF sz = g.MeasureString(avInitials, f);
                    g.DrawString(avInitials, f, Brushes.White,
                        (avSize - sz.Width) / 2, (avSize - sz.Height) / 2);
                }
            };
            _detailPanel.Controls.Add(avatar);

            // Name
            _detailPanel.Controls.Add(new Label
            {
                Text = c.Name,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(lW, 28),
                Location = new Point(lX, 120)
            });

            // Group badge
            var badge = new Panel
            {
                Size = new Size(80, 24),
                Location = new Point((pW - 80) / 2, 156),
                BackColor = AppColors.Background
            };
            string badgeText = c.Group;
            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, badge.Width, badge.Height), 10))
                using (var brush = new SolidBrush(AppColors.NavActive))
                    e.Graphics.FillPath(brush, path);
                using (var f = new Font("Segoe UI", 8.5f))
                {
                    SizeF sz = e.Graphics.MeasureString(badgeText, f);
                    e.Graphics.DrawString(badgeText, f, Brushes.White,
                        (badge.Width - sz.Width) / 2,
                        (badge.Height - sz.Height) / 2);
                }
            };
            _detailPanel.Controls.Add(badge);

            // Divider
            _detailPanel.Controls.Add(new Panel
            {
                BackColor = AppColors.Border,
                Size = new Size(lW, 1),
                Location = new Point(lX, 194)
            });

            // Field rows
            int y = 208;
            AddFieldRow("EMAIL", c.Email, lX, lW, ref y);
            AddFieldRow("PHONE", c.Phone, lX, lW, ref y);
            AddFieldRow("ADDRESS", c.Address, lX, lW, ref y);
            AddFieldRow("ADDED", c.DateAdded, lX, lW, ref y);

            // Edit and Delete buttons
            y += 10;
            int btnW = (pW - 60) / 2;

            var btnEdit = new Button
            {
                Text = "  Edit",
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppColors.TextPrimary,
                BackColor = AppColors.SurfaceLight,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnW, 38),
                Location = new Point(20, y),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderColor = AppColors.Border;
            btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.FlatAppearance.MouseOverBackColor = AppColors.NavHover;

            var btnDelete = new Button
            {
                Text = "  Delete",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(240, 100, 100),
                BackColor = AppColors.Background,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnW, 38),
                Location = new Point(28 + btnW, y),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderColor = AppColors.Danger;
            btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 30, 30);

            _detailPanel.Controls.Add(btnEdit);
            _detailPanel.Controls.Add(btnDelete);
        }

        private void AddFieldRow(string fieldLabel, string value, int x, int width, ref int y)
        {
            _detailPanel.Controls.Add(new Label
            {
                Text = fieldLabel,
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppColors.TextSecondary,
                Size = new Size(width, 18),
                Location = new Point(x, y)
            });
            y += 20;

            _detailPanel.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppColors.TextPrimary,
                Size = new Size(width, 22),
                Location = new Point(x, y)
            });
            y += 34;
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

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] parts = name.Trim().Split(' ');
            return parts.Length >= 2
                ? string.Format("{0}{1}", parts[0][0], parts[1][0]).ToUpper()
                : name[0].ToString().ToUpper();
        }

        // ════════════════════════════════════════════════════════════════════
        // E. CONTACT DATA MODEL (local to this view)
        // ════════════════════════════════════════════════════════════════════
        private class ContactModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string DateAdded { get; set; }
            public string Group { get; set; }
            public Color AvatarColor { get; set; }
        }
    }
}