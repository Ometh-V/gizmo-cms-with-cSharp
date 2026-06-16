#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Services;

namespace ContactManagementSystem.Forms
{
    // Determines which half of the form gets built.
    // Pass Import to show only the import section, Export for only export.
    public enum ImportExportMode
    {
        Import,
        Export
    }

    public partial class ImportExportForm : Form
    {
        private readonly ImportExportMode _mode;

        // ── Custom title bar ─────────────────────────────────────
        private Panel _pnlTitleBar;
        private Label _lblTitleBar;
        private Button _btnMinimize;
        private Button _btnClose;

        // Used for manual window dragging since FormBorderStyle.None
        // removes the native title bar (and its built-in drag handling).
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        // ── Import controls ────────────────────────────────────
        private TextBox txtImportPath;
        private Button btnBrowseImport;
        private Button btnImport;
        private Button btnDownloadSample;
        private Label lblImportStatus;

        // ── Export controls ────────────────────────────────────
        private TextBox txtExportPath;
        private Button btnBrowseExport;
        private Button btnExport;
        private Label lblExportStatus;

        // ── Log box ────────────────────────────────────────────
        private RichTextBox rtbLog;

        public ImportExportForm(ImportExportMode mode)
        {
            _mode = mode;

            // No native chrome — we draw our own dark title bar below,
            // matching AppColors instead of the white OS-themed bar.
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppColors.Background;
            this.ForeColor = AppColors.TextPrimary;
            this.Font = new Font("Segoe UI", 9.5f);

            // A thin border so the borderless form has a visible edge.
            this.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(AppColors.Border, 1),
                    0, 0, this.Width - 1, this.Height - 1);

            BuildTitleBar();
            BuildForm();
        }

        // ════════════════════════════════════════════════════════
        // CUSTOM TITLE BAR — replaces the native white bar
        // ════════════════════════════════════════════════════════
        private void BuildTitleBar()
        {
            _pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = AppColors.Header
            };
            _pnlTitleBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.Border, 1),
                    0, _pnlTitleBar.Height - 1, _pnlTitleBar.Width, _pnlTitleBar.Height - 1);

            _lblTitleBar = new Label
            {
                Text = _mode == ImportExportMode.Import ? "Import Contacts" : "Export Contacts",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 13),
                BackColor = Color.Transparent
            };

            _btnClose = new Button
            {
                Text = "✕",
                Size = new Size(40, 44),
                BackColor = AppColors.Header,
                ForeColor = AppColors.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatAppearance.MouseOverBackColor = AppColors.Danger;
            _btnClose.Click += (s, e) => this.Close();

            _btnMinimize = new Button
            {
                Text = "—",
                Size = new Size(40, 44),
                BackColor = AppColors.Header,
                ForeColor = AppColors.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            _btnMinimize.FlatAppearance.BorderSize = 0;
            _btnMinimize.FlatAppearance.MouseOverBackColor = AppColors.NavHover;
            _btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Keep buttons pinned to the right edge if the form is ever resized
            _pnlTitleBar.Resize += (s, e) =>
            {
                _btnClose.Location = new Point(_pnlTitleBar.Width - 40, 0);
                _btnMinimize.Location = new Point(_pnlTitleBar.Width - 80, 0);
            };

            // Drag-to-move — required since we removed the native title bar
            _pnlTitleBar.MouseDown += TitleBar_MouseDown;
            _pnlTitleBar.MouseMove += TitleBar_MouseMove;
            _lblTitleBar.MouseDown += TitleBar_MouseDown;
            _lblTitleBar.MouseMove += TitleBar_MouseMove;

            _pnlTitleBar.Controls.Add(_lblTitleBar);
            _pnlTitleBar.Controls.Add(_btnMinimize);
            _pnlTitleBar.Controls.Add(_btnClose);
            this.Controls.Add(_pnlTitleBar);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragCursorPoint = Cursor.Position;
            _dragFormPoint = this.Location;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Point diff = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
            this.Location = Point.Add(_dragFormPoint, new Size(diff));
        }

        // ════════════════════════════════════════════════════════
        // BODY — only the section matching _mode is built
        // ════════════════════════════════════════════════════════
        private void BuildForm()
        {
            int x = 24, w = 520;
            int titleBarHeight = 44;

            if (_mode == ImportExportMode.Import)
                BuildImportOnly(x, w, titleBarHeight);
            else
                BuildExportOnly(x, w, titleBarHeight);
        }

        private void BuildImportOnly(int x, int w, int top)
        {
            var pnlImport = MakeSection("⬆  Import Contacts from CSV", x, top + 20, w, 200);

            AddLabelToPanel("SELECT CSV FILE", pnlImport, 14, 38);

            txtImportPath = new TextBox
            {
                Location = new Point(14, 56),
                Width = 360,
                Height = 30,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                ReadOnly = true,
                PlaceholderText = "Click Browse to select a CSV file..."
            };
            pnlImport.Controls.Add(txtImportPath);

            btnBrowseImport = MakeButton("Browse", AppColors.SurfaceLight,
                AppColors.TextPrimary, new Point(384, 56), new Size(120, 30));
            btnBrowseImport.Click += BtnBrowseImport_Click;
            pnlImport.Controls.Add(btnBrowseImport);

            lblImportStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(14, 96),
                Size = new Size(490, 18),
                BackColor = Color.Transparent
            };
            pnlImport.Controls.Add(lblImportStatus);

            btnImport = MakeButton("Import Contacts",
                Color.FromArgb(37, 99, 180), Color.White,
                new Point(14, 122), new Size(180, 40));
            btnImport.Enabled = false;
            btnImport.Click += BtnImport_Click;
            pnlImport.Controls.Add(btnImport);

            btnDownloadSample = MakeButton("Download Sample CSV",
                AppColors.SurfaceLight, AppColors.TextPrimary,
                new Point(206, 122), new Size(180, 40));
            btnDownloadSample.Click += BtnDownloadSample_Click;
            pnlImport.Controls.Add(btnDownloadSample);

            this.Controls.Add(pnlImport);

            int logY = pnlImport.Bottom + 20;
            BuildLogBox(x, w, logY);

            this.ClientSize = new Size(580, logY + 18 + 90 + 20);
        }

        private void BuildExportOnly(int x, int w, int top)
        {
            var pnlExport = MakeSection("⬇  Export Contacts to CSV", x, top + 20, w, 170);

            AddLabelToPanel("SAVE LOCATION", pnlExport, 14, 38);

            txtExportPath = new TextBox
            {
                Location = new Point(14, 56),
                Width = 360,
                Height = 30,
                BackColor = AppColors.SurfaceLight,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                ReadOnly = true,
                PlaceholderText = "Click Browse to choose save location..."
            };
            pnlExport.Controls.Add(txtExportPath);

            btnBrowseExport = MakeButton("Browse", AppColors.SurfaceLight,
                AppColors.TextPrimary, new Point(384, 56), new Size(120, 30));
            btnBrowseExport.Click += BtnBrowseExport_Click;
            pnlExport.Controls.Add(btnBrowseExport);

            lblExportStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(14, 96),
                Size = new Size(490, 18),
                BackColor = Color.Transparent
            };
            pnlExport.Controls.Add(lblExportStatus);

            btnExport = MakeButton("Export All Contacts",
                Color.FromArgb(37, 140, 80), Color.White,
                new Point(14, 118), new Size(180, 40));
            btnExport.Enabled = false;
            btnExport.Click += BtnExport_Click;
            pnlExport.Controls.Add(btnExport);

            this.Controls.Add(pnlExport);

            int logY = pnlExport.Bottom + 20;
            BuildLogBox(x, w, logY);

            this.ClientSize = new Size(580, logY + 18 + 90 + 20);
        }

        private void BuildLogBox(int x, int w, int y)
        {
            this.Controls.Add(new Label
            {
                Text = "LOG",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            rtbLog = new RichTextBox
            {
                Location = new Point(x, y + 18),
                Size = new Size(w, 90),
                BackColor = AppColors.Surface,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            this.Controls.Add(rtbLog);
        }

        // ── Browse for import file ─────────────────────────────
        private void BtnBrowseImport_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Select CSV File to Import",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtImportPath.Text = dialog.FileName;
                btnImport.Enabled = true;
                lblImportStatus.Text = "File selected — ready to import.";
                lblImportStatus.ForeColor = AppColors.TextSecondary;
            }
        }

        // ── Browse for export location ─────────────────────────
        private void BtnBrowseExport_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Save Contacts As CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"contacts_export_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtExportPath.Text = dialog.FileName;
                btnExport.Enabled = true;
                lblExportStatus.Text = "Save location selected — ready to export.";
                lblExportStatus.ForeColor = AppColors.TextSecondary;
            }
        }

        // ── Import ─────────────────────────────────────────────
        private void BtnImport_Click(object sender, EventArgs e)
        {
            string path = txtImportPath.Text.Trim();
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                btnImport.Enabled = false;
                btnImport.Text = "Importing...";
                Log("Starting import from: " + path);

                var result = ImportExportService.ImportFromCSV(path);

                Log($"✅ Imported:  {result.Succeeded}");
                Log($"❌ Failed:    {result.Failed}");

                foreach (var err in result.Errors)
                    Log($"   ⚠ {err}");

                lblImportStatus.Text = result.Succeeded > 0
                    ? $"✅ {result.Succeeded} contact(s) imported successfully."
                    : "❌ Import failed — check the log below.";

                lblImportStatus.ForeColor = result.Succeeded > 0
                    ? Color.FromArgb(80, 200, 120)
                    : Color.FromArgb(220, 80, 80);

                if (result.Succeeded > 0)
                    MessageBox.Show(result.Summary, "Import Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"❌ Error: {ex.Message}");
                lblImportStatus.Text = "❌ Import failed.";
                lblImportStatus.ForeColor = Color.FromArgb(220, 80, 80);

                MessageBox.Show($"Import failed.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnImport.Enabled = true;
                btnImport.Text = "Import Contacts";
            }
        }

        // ── Export ─────────────────────────────────────────────
        private void BtnExport_Click(object sender, EventArgs e)
        {
            string path = txtExportPath.Text.Trim();
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                btnExport.Enabled = false;
                btnExport.Text = "Exporting...";
                Log("Starting export to: " + path);

                int count = ImportExportService.ExportToCSV(path);

                Log($"✅ Exported {count} contact(s) to CSV.");

                lblExportStatus.Text = $"✅ {count} contact(s) exported successfully.";
                lblExportStatus.ForeColor = Color.FromArgb(80, 200, 120);

                MessageBox.Show(
                    $"{count} contact(s) exported successfully!\n\nSaved to:\n{path}",
                    "Export Complete", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"❌ Error: {ex.Message}");
                lblExportStatus.Text = "❌ Export failed.";
                lblExportStatus.ForeColor = Color.FromArgb(220, 80, 80);

                MessageBox.Show($"Export failed.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExport.Enabled = true;
                btnExport.Text = "Export All Contacts";
            }
        }

        // ── Download sample CSV ────────────────────────────────
        private void BtnDownloadSample_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Save Sample CSV Template",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "contacts_sample_template.csv"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                ImportExportService.GenerateSampleCSV(dialog.FileName);
                Log($"✅ Sample template saved to: {dialog.FileName}");
                MessageBox.Show(
                    "Sample CSV template saved!\n\n" +
                    "Fill in your contacts using this format,\n" +
                    "then use Import to load them.",
                    "Template Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save template.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Append to log box ──────────────────────────────────
        private void Log(string message)
        {
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}]  {message}\n");
            rtbLog.ScrollToCaret();
        }

        // ── UI helpers ─────────────────────────────────────────
        private Panel MakeSection(string title, int x, int y, int w, int h)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = AppColors.Surface
            };
            panel.Paint += (s, e) =>
                e.Graphics.DrawRectangle(
                    new Pen(AppColors.Border, 1),
                    0, 0, panel.Width - 1, panel.Height - 1);

            panel.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(14, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            return panel;
        }

        private void AddLabelToPanel(string text, Panel panel, int x, int y)
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
        }

        private Button MakeButton(string text, Color bg, Color fg, Point loc, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}