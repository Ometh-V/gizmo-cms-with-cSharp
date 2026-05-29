using ContactManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ContactManagementSystem.Forms
{
    public partial class LoginForm : Form
    {
        private bool _passowrdVisible = false;
        private int _loginAttempts = 0;
        private const int MaxAttempts = 5;


        public LoginForm()
        {
            InitializeComponent();

            this.Text = "Login";
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
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
            _passowrdVisible = !_passowrdVisible;
            txtPassword.PasswordChar = _passowrdVisible ? '\0' : '●';
            btnShowPass.Text = _passowrdVisible ? "🙈" : "👁";
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
