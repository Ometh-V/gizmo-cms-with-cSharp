using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Forms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to the database.\n\nDetails: {ex.Message}\n\nMake sure SQL Server is running.",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            
            while (true)
            {
                var login = new LoginForm();
                if (login.ShowDialog() != DialogResult.OK)
                    break;  

                var main = new MainForm();
                Application.Run(main);

                
                if (!Session.LoggedOut)
                    break;

                
                Session.LoggedOut = false;
            }
        }
    }
}