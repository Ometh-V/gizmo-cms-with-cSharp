using ContactManagementSystem.Services;
using ContactManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ContactManagementSystem.Forms
{
    public partial class LoginForm : Form
    {
        private bool _passwordVisible = false;
        private int _loginAttempts = 0;
        private const int MaxAttempts = 5;

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int Msg, int wParam, int lParam);


        public LoginForm()
        {
            InitializeComponent();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedCorners();
            CenterCard();
            btnLogin.Refresh();
            LoadRememberedUser();
        }


        private void LoadRememberedUser()
        {
            var saved = AppSettings.LoadUsername();
            if (!string.IsNullOrEmpty(saved))
            {
                txtUsername.Text = saved;
                chkRemember.Checked = true;
                txtPassword.Focus();
            }
        }

       
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedCorners();
            CenterCard();
        }

        
        private void ApplyRoundedCorners()
        {
            this.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));
        }

        
        private void CenterCard()
        {
            pnlMain.Location = new Point(
                (this.ClientSize.Width - pnlMain.Width) / 2,
                (this.ClientSize.Height - pnlMain.Height) / 2);
        }

        
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }



        private void btnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                ShowError("Please Enter Both Username and Password.");
                return;
            }

            if (_loginAttempts >= MaxAttempts)
            {
                ShowError("Too Many Failed Attempts. Please Restart The Application!");
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Signing in...";

                if (UserService.Login(username,password))
                {
                    if (chkRemember.Checked)
                        AppSettings.SaveUsername(username);
                    else
                        AppSettings.ClearUsername();


                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _loginAttempts++;
                    int remaining = MaxAttempts - _loginAttempts;
                    ShowError($"Invalid UserName or Password. {remaining} attempt(s) remaining.");
                    txtPassword.Clear();
                    txtPassword.Focus();

                    btnLogin.Enabled = true;
                    btnLogin.Text = "Sign in";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An Error Occured During Logn. \n\nDetails: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "Sign in";
            }
        }


        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnLogin_Click(sender, e);
            }
        }


        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            txtPassword.PasswordChar = _passwordVisible ? '\0' : '●';
            btnShowPass.Text = _passwordVisible ? "🙈" : "👁";
        }


        private void ShowError(string message)
        {
            MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                var confirm = MessageBox.Show(
                    "Are You Sure You Want To Exit?",
                    "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.No)
                    e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
