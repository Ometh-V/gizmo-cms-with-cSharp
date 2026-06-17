using System;
using System.Collections.Generic;
using System.Text;

namespace ContactManagementSystem.Helpers
{
    internal static class Session
    {
        internal static int UserID { get; set; }
        internal static string UserName { get; set; } = "";
        internal static string Role { get; set; } = "";

        internal static bool LoggedOut { get; set; } = false;

        internal static bool IsAdmin => Role == "Admin";

        internal static void Clear()
        {
            UserID = 0;
            UserName = "";
            Role = "";
        }
    }
}