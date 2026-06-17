using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
        private ComboBox _cmbSort; // ← NEW: sort dropdown

        // ── Selection state ───────────────────────────────────────────────────
        private ContactListItem _selectedItem;
        private int _selectedContactId;

        public AllContactsView()
        {
            this.BackColor = AppColors.Background;
            this.DoubleBuffered = true;

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
            _split.Size = new Size(1000, 600);


            _split.Dock = DockStyle.Fill;
            _split.Panel1MinSize = 300;
            _split.Panel2MinSize = 260;
            _split.SplitterDistance = 360;
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

            // ── Sort dropdown (owner-drawn to match dark theme) ─
            _cmbSort = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                Width = 110,
                ItemHeight = 22,
                Font = new Font("Segoe UI", 8.5f),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _cmbSort.Items.AddRange(new object[] { "A-Z", "Z-A", "Newest", "Oldest" });
            _cmbSort.SelectedIndex = 0;
            // Position on the right side of the header, vertically centred
            _cmbSort.Location = new Point(header.Width - _cmbSort.Width - 16,
                (header.Height - _cmbSort.Height) / 2);
            _cmbSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmbSort.SelectedIndexChanged += CmbSort_SelectedIndexChanged;

            // Owner-draw: paint each item with the app dark theme
            _cmbSort.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool isSelected = (e.State & DrawItemState.Selected) != 0;
                Color bg = isSelected ? AppColors.NavActive : AppColors.SurfaceLight;
                Color fg = AppColors.TextPrimary;

                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                string itemText = _cmbSort.Items[e.Index].ToString();
                TextRenderer.DrawText(e.Graphics, itemText, e.Font,
                    new Point(e.Bounds.X + 6, e.Bounds.Y + 3), fg);
            };

            header.Controls.Add(_cmbSort);

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

        // Loads ALL contacts fresh from the database (default view)
        private void LoadContacts()
        {
            try
            {
                List<Contact> contacts = ContactService.GetAll();
                RenderContacts(contacts, $"All contacts — {contacts.Count} contacts");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load contacts.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        internal void LoadContacts(List<Contact> contacts)
        {
            RenderContacts(contacts, $"Results — {contacts.Count} contact(s)");
        }


        private void RenderContacts(List<Contact> contacts, string headerText)
        {
            _listFlow.Controls.Clear();
            _selectedItem = null;
            _selectedContactId = 0;
            ShowDetailPlaceholder();

            int colorIndex = 0;
            foreach (var c in contacts)
            {
                var item = new ContactListItem();
                item.Width = _listFlow.ClientSize.Width;
                item.SetData(
                    c.ContactID,
                    c.FullName,
                    c.Email,
                    AppColors.AvatarColors[colorIndex % AppColors.AvatarColors.Length]);
                item.Tag = c;
                item.Click += OnContactItemClicked;
                _listFlow.Controls.Add(item);
                colorIndex++;
            }

            _lblCount.Text = headerText;
        }


        private void CmbSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var sorted = SearchService.GetSorted(_cmbSort.SelectedItem.ToString());
                RenderContacts(sorted, $"All contacts — {sorted.Count} contacts");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to sort contacts.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResizeListItems()
        {
            int w = _listFlow.ClientSize.Width;

            // Safety check: Do not resize if the FlowPanel hasn't rendered its width yet
            if (w < 50) return;

            foreach (ContactListItem item in _listFlow.Controls)
            {
                // Subtract 5px to prevent the horizontal scrollbar from triggering
                item.Width = w - 5;
            }
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
            _selectedContactId = item.ContactId; // ← store selected ID
            _selectedItem.SetSelected(true);


            var contact = item.Tag as Contact;
            if (contact != null)
                ShowContactDetail(contact);
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

        private void ShowContactDetail(Contact c)
        {
            _detailPanel.Controls.Clear();

            int pW = _detailPanel.ClientSize.Width;
            int lX = 24;
            int lW = pW - 48;
            int avSize = 80;


            Color avColor = AppColors.AvatarColors[c.ContactID % AppColors.AvatarColors.Length];
            string avInitials = c.Initials; // ← was GetInitials(c.Name)

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
                Text = c.FullName, // ← was c.Name
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(lW, 28),
                Location = new Point(lX, 120)
            });


            var badge = new Panel
            {
                Size = new Size(90, 24),
                Location = new Point((pW - 90) / 2, 156),
                BackColor = AppColors.Background
            };
            string badgeText = c.ContactType; // ← was c.Group
            Color badgeColor = c.ContactType == "Customer"
                ? Color.FromArgb(37, 99, 180)
                : Color.FromArgb(100, 60, 160);
            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(
                    new Rectangle(0, 0, badge.Width, badge.Height), 10))
                using (var brush = new SolidBrush(badgeColor))
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


            int y = 208;
            AddFieldRow("EMAIL", c.Email, lX, lW, ref y);
            AddFieldRow("PHONE", c.Phone, lX, lW, ref y);
            AddFieldRow("ADDRESS", c.Address, lX, lW, ref y);
            AddFieldRow("ADDED", c.CreatedAt.ToString("dd MMM yyyy"), lX, lW, ref y); // ← was c.DateAdded
            if (!string.IsNullOrWhiteSpace(c.Notes))
                AddFieldRow("NOTES", c.Notes, lX, lW, ref y);

            // Loyalty summary — Customers only
            if (c.ContactType == "Customer")
            {
                try
                {
                    var cust = ContactService.GetCustomerDetails(c.ContactID);
                    if (cust != null)
                    {
                        var summary = LoyaltyService.GetSummary(cust.CustomerID);
                        if (summary != null)
                        {
                            AddFieldRow("LOYALTY TIER", summary.Tier, summary.TierColor, lX, lW, ref y);
                            AddFieldRow("POINTS", $"{summary.LoyaltyPoints} pts", lX, lW, ref y);
                            AddFieldRow("TOTAL SPENT", $"Rs. {summary.TotalPurchases:N2}", lX, lW, ref y);
                            AddFieldRow("LAST PURCHASE", summary.LastPurchaseFormatted, lX, lW, ref y);

                            if (summary.PointsToNextTier > 0)
                                AddFieldRow("NEXT TIER", $"{summary.PointsToNextTier} pts to go", lX, lW, ref y);
                        }
                    }
                }
                catch
                {
                    // Loyalty data unavailable — silently skip
                }
            }

            y += 10;
            int btnW = (pW - 60) / 2;

            // Edit button
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


            btnEdit.Click += (s, e) =>
            {
                var form = new EditContactForm(_selectedContactId);
                if (form.ShowDialog() == DialogResult.OK)
                    LoadContacts();
            };

            // Delete button
            var btnDelete = new Button
            {
                Text = "  Delete",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(240, 100, 100),
                BackColor = AppColors.Background,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnW, 38),
                Location = new Point(28 + btnW, y),
                Cursor = Cursors.Hand,
                // ← NEW: hide delete button for non-admins
                Visible = Session.IsAdmin
            };
            btnDelete.FlatAppearance.BorderColor = AppColors.Danger;
            btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 30, 30);


            btnDelete.Click += (s, e) =>
            {
                if (!Session.IsAdmin)
                {
                    MessageBox.Show("You don't have permission to delete contacts.",
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var prefs = PreferencesService.Load();
                bool proceed = true;

                if (prefs.ConfirmBeforeDelete)
                {
                    var confirm = MessageBox.Show(
                        $"Delete {c.FullName}?\nThis cannot be undone.",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    proceed = confirm == DialogResult.Yes;
                }

                if (proceed)
                {
                    try
                    {
                        ContactService.Delete(_selectedContactId);
                        LoadContacts();
                        ShowDetailPlaceholder();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete contact.\n\n{ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

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


        private void AddFieldRow(string fieldLabel, string value, Color valueColor, int x, int width, ref int y)
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
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = valueColor,
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


    }
}