using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    public partial class AddContactForm : Form
    {
        private ComboBox cmbType;
        private TextBox txtFirstName, txtLastName;
        private TextBox txtPhone, txtEmail;
        private TextBox txtAddress, txtNotes;
        private Panel pnlSupplier;
        private TextBox txtCompany, txtCategory, txtPaymentTerms;
        private Button btnSave, btnCancel;
        private Panel pnlTitleBar;

        // Fixed layout constants 
        private const int FormX = 30;
        private const int FormW = 420;
        private const int TitleBarH = 36;
        private const int BtnH = 42;
        private const int BtnGapY = 16;
        private const int BottomPad = 24;

        // Computed once in BuildForm
        private int _btnYNormal;
        private int _btnYSupplier;
        private int _normalHeight;
        private int _supplierHeight;

        // Win32 – drag borderless form 
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public AddContactForm()
        {
            this.Text = "Add New Contact";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;  // borderless
            this.MaximizeBox = false;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildForm();
        }

        private void BuildForm()
        {
            int x = FormX, w = FormW;

            // Custom title bar 
            pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TitleBarH,
                BackColor = Color.FromArgb(18, 18, 28)
            };

            // App icon / name label on the left
            var lblAppName = new Label
            {
                Text = "Add New Contact",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(300, TitleBarH),
                Location = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Close button (×)
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

            // Drag-to-move
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

            // Content starts below title bar 
            int y = TitleBarH + 20;

            // Page title 
            this.Controls.Add(new Label
            {
                Text = "Add New Contact",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            y += 44;

            // Contact Type 
            AddFieldLabel("CONTACT TYPE", x, ref y);
            cmbType = new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = 32,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 24,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                Font = new Font("Segoe UI", 10f)
            };
            cmbType.Items.AddRange(new object[] { "Customer", "Supplier" });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += (s, e) => ToggleSupplierPanel();

            // Owner-draw: colours every row in the dropdown to match the dark theme
            cmbType.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                bool isHighlighted = (e.State & DrawItemState.Selected) != 0;

                Color bgColor = isHighlighted
                    ? Color.FromArgb(37, 99, 180)   // accent blue when hovered
                    : AppColors.SurfaceLight;        // normal dark background

                Color textColor = isHighlighted
                    ? Color.White
                    : AppColors.TextPrimary;

                using (var bgBrush = new SolidBrush(bgColor))
                using (var textBrush = new SolidBrush(textColor))
                {
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);
                    e.Graphics.DrawString(
                        cmbType.Items[e.Index].ToString(),
                        e.Font ?? cmbType.Font,
                        textBrush,
                        e.Bounds.X + 6,
                        e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2);
                }
            };

            this.Controls.Add(cmbType);
            y += 42;

            // First & Last name 
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

            // Phone & Email 
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

            // Address 
            AddFieldLabel("ADDRESS", x, ref y);
            txtAddress = AddTextBox(x, y, w);
            y += 40;

            // Notes 
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

            // Supplier panel 
            // 3 fields × (18 label + 28 box + 10 gap) + 10 top pad + 14 bottom pad = 190px
            const int supplierPanH = 190;
            pnlSupplier = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, supplierPanH),
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

            // Compute button Y positions & form heights 
            _btnYNormal = y + BtnGapY;                           // below notes
            _btnYSupplier = y + supplierPanH + BtnGapY;            // below supplier panel

            _normalHeight = _btnYNormal + BtnH + BottomPad;
            _supplierHeight = _btnYSupplier + BtnH + BottomPad;

            // Buttons 
            btnSave = new Button
            {
                Text = "Save Contact",
                Location = new Point(x, _btnYNormal),
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
                Location = new Point(x + 210, _btnYNormal),
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

            // Set initial form size
            this.Size = new Size(500, _normalHeight);

            // Reposition close button after width is set
            btnClose.Location = new Point(this.ClientSize.Width - btnClose.Width, 0);
        }

        // Toggle supplier panel 
        private void ToggleSupplierPanel()
        {
            bool isSupplier = cmbType.SelectedItem?.ToString() == "Supplier";
            pnlSupplier.Visible = isSupplier;

            int btnY = isSupplier ? _btnYSupplier : _btnYNormal;
            btnSave.Location = new Point(btnSave.Left, btnY);
            btnCancel.Location = new Point(btnCancel.Left, btnY);
            this.Height = isSupplier ? _supplierHeight : _normalHeight;
        }

        // Save 
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

                string type = cmbType.SelectedItem.ToString();

                Contact contact = type == "Supplier"
                    ? new Supplier
                    {
                        CompanyName = txtCompany.Text.Trim(),
                        ProductCategory = txtCategory.Text.Trim(),
                        PaymentTerms = txtPaymentTerms.Text.Trim()
                    }
                    : new Contact();

                contact.ContactType = type;
                contact.FirstName = txtFirstName.Text.Trim();
                contact.LastName = txtLastName.Text.Trim();
                contact.Phone = txtPhone.Text.Trim();
                contact.Email = txtEmail.Text.Trim();
                contact.Address = txtAddress.Text.Trim();
                contact.Notes = txtNotes.Text.Trim();

                ContactService.Add(contact);

                MessageBox.Show(
                    $"{contact.FullName} has been added successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save contact.\n\nDetails: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
                btnSave.Text = "Save Contact";
            }
        }

        private void ShowError(string message) =>
            MessageBox.Show(message, "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // UI helpers
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
            y += 40;
            return txt;
        }
    }
}