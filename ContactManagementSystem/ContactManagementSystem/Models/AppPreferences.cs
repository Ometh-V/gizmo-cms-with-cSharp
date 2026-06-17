namespace ContactManagementSystem.Models
{
    // Plain data container serialized to/from a local JSON file by
    // PreferencesService. Defaults here are what a brand-new install gets.
    internal class AppPreferences
    {
        // "Dashboard" or "AllContacts"
        public string DefaultLandingPage { get; set; } = "Dashboard";

        public bool ConfirmBeforeDelete { get; set; } = true;

        // "A-Z", "Z-A", or "Recent"
        public string DefaultSortOrder { get; set; } = "A-Z";
    }
}