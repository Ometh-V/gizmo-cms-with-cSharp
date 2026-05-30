using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace ContactManagementSystem.Helpers
{
    internal static class AppSettings
    {
        private static readonly string SettingsPath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.dat");

        internal static void SaveUsername(string username)
        {
            try
            {
                File.WriteAllText(SettingsPath, username);
            }
            catch { }
        }


        internal static string LoadUsername()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    return File.ReadAllText(SettingsPath).Trim();
            }
            catch { }
            return "";
        }




        internal static void ClearUsername()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    File.Delete(SettingsPath);
            }
            catch { }
        }
    }
}
