using System;
using System.IO;
using System.Text.Json;
using ContactManagementSystem.Models;

namespace ContactManagementSystem.Helpers
{
    // Reads/writes a small JSON file in the user's AppData folder.
    // No database table needed — this is per-machine, not per-account.
    internal static class PreferencesService
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ContactManagementSystem");

        private static readonly string FilePath = Path.Combine(FolderPath, "preferences.json");

        internal static AppPreferences Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new AppPreferences();   // first run — use defaults

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppPreferences>(json) ?? new AppPreferences();
            }
            catch
            {
                // Corrupted or unreadable file — fall back to defaults
                // rather than crashing the app on startup.
                return new AppPreferences();
            }
        }

        internal static void Save(AppPreferences prefs)
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                string json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save preferences: {ex.Message}", ex);
            }
        }
    }
}