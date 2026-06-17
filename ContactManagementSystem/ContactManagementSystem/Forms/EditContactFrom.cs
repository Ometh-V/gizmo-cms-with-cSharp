using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class EditContactForm : Form
    {
        private readonly int _contactId;
        private Contact _contact;

        private TextBox txtFirstName, txtLastName;
        private TextBox txtPhone, txtEmail;
        private TextBox txtAddress, txtNotes;
        private Panel pnlSupplier;
        private TextBox txtCompany, txtCategory, txtPaymentTerms;
        private Label lblType, lblLoyalty;
        private Button btnSave, btnCancel;
        private Panel pnlTitleBar;

        // ── Customer / Loyalty panel (admin-only) ──────────────
        private Panel pnlCustomer;
        private TextBox txtTotalSpent;
        private DateTimePicker dtpLastPurchase;
        private CheckBox chkClearLastPurchase;
        private Label lblLoyaltyPoints;
        private const int CustomerPanH = 210;

        // ── Layout constants ───────────────────────────────────
        private const int FormX = 30;
        private const int FormW = 420;
        private const int TitleBarH = 36;
        private const int BtnH = 42;
        private const int BtnGapY = 16;
        private const int BottomPad = 24;
        private const int SupplierPanH = 190;

        // Computed in BuildForm, used in LoadContact
        private int _btnY;
        private int _baseHeight;   // height without supplier panel

        // ── Win32 drag ─────────────────────────────────────────
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public EditContactForm(int contactId)
        {
            _contactId = contactId;
            this.Text = "Edit Contact";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
            LoadContact();
        }

        private void BuildForm()
        {
            int x = FormX, w = FormW;

            // ── Custom title bar ───────────────────────────────
            pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TitleBarH,
                BackColor = Color.FromArgb(18, 18, 28)
            };

            var lblAppName = new Label
            {
                Text = "Edit Contact",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(300, TitleBarH),
                Location = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            var btnClose = new Button
            {
                Text = "×",
                Font = new Font("Segoe UI", 13f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(46, TitleBarH),
                Location = new Point(500 - 46, 0),
                TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 10, 20);
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            pnlTitleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); }
            };
            lblAppName.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); }
            };

            pnlTitleBar.Controls.Add(lblAppName);
            pnlTitleBar.Controls.Add(btnClose);
            this.Controls.Add(pnlTitleBar);

            // ── Content ────────────────────────────────────────
            int y = TitleBarH + 20;

            this.Controls.Add(new Label
            {
                Text = "Edit Contact",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 36;

            // Contact type badge (read-only)
            lblType = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 180),
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblType);

            // Loyalty tier (customers only)
            lblLoyalty = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(250, 200, 50),
                Location = new Point(x + 120, y),
                AutoSize = true,
                BackColor = Color.Transparent,
                Visible = false
            };
            this.Controls.Add(lblLoyalty);
            y += 30;

            // ── First & Last name ──────────────────────────────
            int halfW = (w / 2) - 6;
            int rightX = x + halfW + 12;

            AddFieldLabel("FIRST NAME *", x, ref y);
            txtFirstName = AddTextBox(x, y, halfW);
            AddFieldLabelAt("LAST NAME *", rightX, y - 18);
            txtLastName = new TextBox
            {
                Location = new Point(rightX, y),
                Width = halfW,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            this.Controls.Add(txtLastName);
            y += 40;

            // ── Phone & Email ──────────────────────────────────
            AddFieldLabel("PHONE", x, ref y);
            txtPhone = AddTextBox(x, y, halfW);
            AddFieldLabelAt("EMAIL", rightX, y - 18);
            txtEmail = new TextBox
            {
                Location = new Point(rightX, y),
                Width = halfW,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            this.Controls.Add(txtEmail);
            y += 40;

            // ── Address ────────────────────────────────────────
            AddFieldLabel("ADDRESS", x, ref y);
            txtAddress = AddTextBox(x, y, w);
            y += 40;

            // ── Notes ──────────────────────────────────────────
            AddFieldLabel("NOTES", x, ref y);
            txtNotes = new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 70,
                Multiline = true,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f),
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtNotes);
            y += 84;

            // ── Supplier panel (hidden until LoadContact) ──────
            pnlSupplier = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, SupplierPanH),
                BackColor = Color.FromArgb(42, 42, 52),
                Visible = false
            };
            pnlSupplier.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(AppColors.Border, 1),
                    0, 0, pnlSupplier.Width - 1, pnlSupplier.Height - 1);

            int sy = 10;
            AddFieldLabelToPanel("COMPANY NAME", pnlSupplier, 10, ref sy);
            txtCompany = AddTextBoxToPanel(pnlSupplier, 10, ref sy, w - 20);
            AddFieldLabelToPanel("PRODUCT CATEGORY", pnlSupplier, 10, ref sy);
            txtCategory = AddTextBoxToPanel(pnlSupplier, 10, ref sy, w - 20);
            AddFieldLabelToPanel("PAYMENT TERMS", pnlSupplier, 10, ref sy);
            txtPaymentTerms = AddTextBoxToPanel(pnlSupplier, 10, ref sy, w - 20);

            this.Controls.Add(pnlSupplier);

            // ── Customer / Loyalty panel (admin-only) ──────────
            pnlCustomer = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, CustomerPanH),
                BackColor = Color.FromArgb(30, 52, 42),
                Visible = false
            };
            pnlCustomer.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(Color.FromArgb(50, 180, 100), 1),
                    0, 0, pnlCustomer.Width - 1, pnlCustomer.Height - 1);

            int cy = 10;

            // Header label
            pnlCustomer.Controls.Add(new Label
            {
                Text = "🛒  LOYALTY / PURCHASE ADMIN",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 130),
                Location = new Point(10, cy),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            cy += 22;

            // Recalculated points preview label
            lblLoyaltyPoints = new Label
            {
                Text = "Points: —",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(250, 200, 50),
                Location = new Point(10, cy),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlCustomer.Controls.Add(lblLoyaltyPoints);
            cy += 22;

            // Total Spent field
            AddFieldLabelToPanel("TOTAL SPENT (Rs.)", pnlCustomer, 10, ref cy);
            txtTotalSpent = new TextBox
            {
                Location = new Point(10, cy),
                Width = w - 20,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            txtTotalSpent.TextChanged += TxtTotalSpent_TextChanged;
            pnlCustomer.Controls.Add(txtTotalSpent);
            cy += 40;

            // Last Purchase Date
            AddFieldLabelToPanel("LAST PURCHASE DATE", pnlCustomer, 10, ref cy);
            dtpLastPurchase = new DateTimePicker
            {
                Location = new Point(10, cy),
                Width = w - 20,
                Format = DateTimePickerFormat.Short,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                Font = new Font("Segoe UI", 10f),
                Value = DateTime.Today
            };
            pnlCustomer.Controls.Add(dtpLastPurchase);
            cy += 36;

            chkClearLastPurchase = new CheckBox
            {
                Text = "Clear date (no purchases yet)",
                Location = new Point(10, cy),
                AutoSize = true,
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f)
            };
            chkClearLastPurchase.CheckedChanged += (s, e) =>
                dtpLastPurchase.Enabled = !chkClearLastPurchase.Checked;
            pnlCustomer.Controls.Add(chkClearLastPurchase);

            this.Controls.Add(pnlCustomer);
            _btnY = y + BtnGapY;                            // below notes (supplier hidden)
            _baseHeight = _btnY + BtnH + BottomPad;

            // ── Buttons ────────────────────────────────────────
            btnSave = new Button
            {
                Text = "Save Changes",
                Location = new Point(x, _btnY),
                Size = new Size(200, BtnH),
                BackColor = Color.FromArgb(37, 99, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(47, 115, 200);
            btnSave.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 75, 150);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(x + 210, _btnY),
                Size = new Size(200, BtnH),
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnCancel.FlatAppearance.BorderColor = AppColors.Border;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            this.Size = new Size(500, _baseHeight);
            btnClose.Location = new Point(this.ClientSize.Width - btnClose.Width, 0);
        }

        // ── Load existing contact data ─────────────────────────
        private void LoadContact()
        {
            try
            {
                _contact = ContactService.GetById(_contactId);
                if (_contact == null)
                {
                    MessageBox.Show("Contact not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                txtFirstName.Text = _contact.FirstName;
                txtLastName.Text = _contact.LastName;
                txtPhone.Text = _contact.Phone;
                txtEmail.Text = _contact.Email;
                txtAddress.Text = _contact.Address;
                txtNotes.Text = _contact.Notes;

                lblType.Text = $"Type: {_contact.ContactType}";

                if (_contact.ContactType == "Customer")
                {
                    var cust = ContactService.GetCustomerDetails(_contactId);
                    if (cust != null)
                    {
                        lblLoyalty.Text = cust.LoyaltyTier;
                        lblLoyalty.Visible = true;

                        // Show admin loyalty panel only for admins
                        if (Session.IsAdmin)
                        {
                            txtTotalSpent.Text = cust.TotalPurchases.ToString("F2");
                            UpdatePointsPreview(cust.TotalPurchases);

                            if (cust.LastPurchaseDate.HasValue)
                            {
                                dtpLastPurchase.Value = cust.LastPurchaseDate.Value;
                                chkClearLastPurchase.Checked = false;
                            }
                            else
                            {
                                chkClearLastPurchase.Checked = true;
                                dtpLastPurchase.Enabled = false;
                            }

                            pnlCustomer.Visible = true;

                            int newBtnY = _btnY + CustomerPanH + BtnGapY;
                            btnSave.Location = new Point(btnSave.Left, newBtnY);
                            btnCancel.Location = new Point(btnCancel.Left, newBtnY);
                            this.Height = newBtnY + BtnH + BottomPad;
                        }
                    }
                }

                if (_contact.ContactType == "Supplier")
                {
                    var supplier = ContactService.GetSupplierDetails(_contactId);
                    if (supplier != null)
                    {
                        txtCompany.Text = supplier.CompanyName;
                        txtCategory.Text = supplier.ProductCategory;
                        txtPaymentTerms.Text = supplier.PaymentTerms;
                    }

                    pnlSupplier.Visible = true;

                    // Push buttons down and expand form to fit panel
                    int newBtnY = _btnY + SupplierPanH + BtnGapY;
                    btnSave.Location = new Point(btnSave.Left, newBtnY);
                    btnCancel.Location = new Point(btnCancel.Left, newBtnY);
                    this.Height = newBtnY + BtnH + BottomPad;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load contact.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Save changes ───────────────────────────────────────
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            { ShowError("First name is required."); txtFirstName.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            { ShowError("Last name is required."); txtLastName.Focus(); return; }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !txtEmail.Text.Contains("@"))
            { ShowError("Please enter a valid email address."); txtEmail.Focus(); return; }

            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                _contact.FirstName = txtFirstName.Text.Trim();
                _contact.LastName = txtLastName.Text.Trim();
                _contact.Phone = txtPhone.Text.Trim();
                _contact.Email = txtEmail.Text.Trim();
                _contact.Address = txtAddress.Text.Trim();
                _contact.Notes = txtNotes.Text.Trim();

                if (_contact.ContactType == "Supplier" && _contact is Supplier s)
                {
                    s.CompanyName = txtCompany.Text.Trim();
                    s.ProductCategory = txtCategory.Text.Trim();
                    s.PaymentTerms = txtPaymentTerms.Text.Trim();
                }

                ContactService.Update(_contact);

                // ── Save loyalty changes if admin + customer ───
                if (_contact.ContactType == "Customer" && Session.IsAdmin && pnlCustomer.Visible)
                {
                    if (!decimal.TryParse(txtTotalSpent.Text.Trim(), out decimal newTotal) || newTotal < 0)
                    {
                        ShowError("Total Spent must be a valid non-negative number.");
                        btnSave.Enabled = true;
                        btnSave.Text = "Save Changes";
                        return;
                    }

                    var cust = ContactService.GetCustomerDetails(_contactId);
                    if (cust != null)
                    {
                        DateTime? lastDate = chkClearLastPurchase.Checked
                            ? (DateTime?)null
                            : dtpLastPurchase.Value.Date;

                        LoyaltyService.AdminUpdateTotalSpent(cust.CustomerID, newTotal, lastDate);

                        // Refresh tier badge
                        int newPoints = Customer.CalculatePoints(newTotal);
                        lblLoyalty.Text = LoyaltyService.GetTier(newPoints);
                    }
                }

                MessageBox.Show("Contact updated successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update contact.\n\nDetails: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
                btnSave.Text = "Save Changes";
            }
        }

        // ── Live points preview ────────────────────────────────
        private void TxtTotalSpent_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTotalSpent.Text.Trim(), out decimal val) && val >= 0)
                UpdatePointsPreview(val);
            else
                lblLoyaltyPoints.Text = "Points: —";
        }

        private void UpdatePointsPreview(decimal total)
        {
            int pts = Customer.CalculatePoints(total);
            string tier = LoyaltyService.GetTier(pts);
            lblLoyaltyPoints.Text = $"Points: {pts:N0}  →  {tier}";
            lblLoyaltyPoints.ForeColor = LoyaltyService.GetTierColor(pts);
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // ── UI helpers ─────────────────────────────────────────
        private void AddFieldLabel(string text, int x, ref int y)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 18;
        }

        private void AddFieldLabelAt(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
        }

        private TextBox AddTextBox(int x, int y, int w)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            this.Controls.Add(txt);
            return txt;
        }

        private void AddFieldLabelToPanel(string text, Panel panel, int x, ref int y)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 18;
        }

        private TextBox AddTextBoxToPanel(Panel panel, int x, ref int y, int w)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 28,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            panel.Controls.Add(txt);
            y += 40;   // 28 box + 12 gap — prevents last field clipping
            return txt;
        }
    }
}