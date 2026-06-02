using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ContactManagementSystem.Helpers
{
    public class NavigationManager
    {
        private readonly Panel _contentPanel;

        // Key   = the Type of the view (e.g. typeof(AllContactsView))
        // Value = the single cached instance of that view
        private readonly Dictionary<Type, UserControl> _viewCache;

        public NavigationManager(Panel contentPanel)
        {
            _contentPanel = contentPanel;
            _viewCache = new Dictionary<Type, UserControl>();
        }

        public void NavigateTo<T>() where T : UserControl, new()
        {
            Type viewType = typeof(T);

           
            if (!_viewCache.ContainsKey(viewType))
            {
                var newView = new T { Dock = DockStyle.Fill };
                _contentPanel.Controls.Add(newView);
                _viewCache[viewType] = newView;
            }

            // ── 2. Refresh data (fixes stale-cache problem)
            // Only called if the view opted in by implementing INavigationAware
            if (_viewCache[viewType] is INavigationAware aware)
                aware.OnNavigatedTo();

            // ── 3. Show this view, push all others behind 
            _viewCache[viewType].BringToFront();
        }
    }
}