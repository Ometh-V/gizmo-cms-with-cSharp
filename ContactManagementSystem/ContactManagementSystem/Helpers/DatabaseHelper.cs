using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ContactManagementSystem.Helpers
{
    internal static class DatabaseHelper
    {
        private static readonly string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=ContactsDB;Integrated Security=true;TrustServerCertificate=true;";
    }
}
