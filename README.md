# ContactManager

A desktop contact management system built with C# WinForms (.NET 10) and SQL Server, developed as a VAP (Visual Application Programming) coursework project.

## Features

- **Authentication** — login with role-based access (Admin / User), passwords hashed with SHA-256
- **Dashboard** — live counts for total contacts, customers, suppliers, contacts added this month, and loyalty tier breakdown
- **Contact Management** — full CRUD for Customers and Suppliers, with a master-detail dark-themed interface
- **Groups** — create, edit, and delete groups; add or remove contacts as members
- **Loyalty Program** — automatic point accrual and tier classification (Newbie / Regular / VIP) for customers
- **Import / Export** — bulk import contacts from CSV, export the full contact list to CSV, with a downloadable sample template
- **User Management** — admins can add accounts, deactivate/reactivate users, and change roles
- **Settings** — general preferences (default landing page, sort order, delete confirmation), account/password management, and a live database connection test

## Tech Stack

| Layer | Technology |
|---|---|
| UI | C# WinForms, .NET 10, custom dark theme (no Designer-based layout) |
| Data Access | Microsoft.Data.SqlClient |
| Database | Microsoft SQL Server |
| Architecture | Service layer (`Services/`) separating UI from data access; a custom `NavigationManager` caches and swaps views inside the main shell |

## Project Structure

```
ContactManagementSystem/
├── Forms/          Views, controls, and dialogs (MainForm, LoginForm, AllContactsView, GroupsView, SettingsView, ...)
├── Helpers/         Cross-cutting infrastructure (DatabaseHelper, Session, AppColors, AppConfig, PreferencesService, NavigationManager)
├── Models/          Plain data classes (Contact, Customer, Supplier, Group, AppPreferences)
├── Services/        Business logic and SQL access (ContactService, GroupService, UserService, LoyaltyService, ImportExportService)
└── Program.cs       Application entry point
```

## Setup Instructions

### Prerequisites

- Visual Studio 2022 or later, with the **.NET desktop development** workload
- .NET 10 SDK
- Microsoft SQL Server (Express edition is sufficient) running locally or accessible on the network
- NuGet package `Microsoft.Data.SqlClient` (restored automatically on build)

### 1. Configure the database connection

Open `Helpers/AppConfig.cs` and set your SQL Server details:

```csharp
internal static class AppConfig
{
    public static string DbServer = "YOUR_SERVER_NAME";   // e.g. "localhost" or ".\SQLEXPRESS"
    public static string DbName   = "ContactsDB";

    // Leave both empty to use Windows Authentication.
    // Fill both in to use SQL Server Authentication instead.
    public static string DbUser     = "";
    public static string DbPassword = "";
}
```

### 2. Create the database

You have two options:

- **Automatic (recommended):** just run the application. `DatabaseHelper.InitializeDatabase()` creates the `ContactsDB` database and all required tables on first launch if they don't already exist.
- **Manual:** run `ContactsDB_Schema.sql` (included in this submission) against your SQL Server instance using SQL Server Management Studio or `sqlcmd`.

### 3. Build and run

1. Open `ContactManagementSystem.sln` in Visual Studio.
2. Let NuGet restore packages (happens automatically on build).
3. Press **F5** to build and run.

### 4. Log in

Two accounts are seeded automatically:

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin — full access including Import/Export and User Management |
| `user` | `User@123` | User — standard access, restricted from delete and admin-only actions |

## Notes

- General application preferences (default landing page, sort order, delete confirmation) are stored locally in `%AppData%/ContactManagementSystem/preferences.json` and are machine-specific, not tied to a database account.
- Default login credentials above should be changed via **Settings → Account** after first login in any real deployment.