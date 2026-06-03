namespace ContactManagementSystem.Helpers
{
    /// <summary>
    /// Implement this on any UserControl view that needs to
    /// refresh its data every time the user navigates to it.
    /// The NavigationManager calls OnNavigatedTo() automatically.
    /// </summary>
    public interface INavigationAware
    {
        void OnNavigatedTo();
    }
}