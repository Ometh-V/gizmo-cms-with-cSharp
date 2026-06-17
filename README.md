# Gizmo Contact Management System

A lightweight, high-performance desktop Contact Management System (CMS) tailored for Gizmo Electronics. This project was developed as a coursework submission for the **Visual Application Programming (VAP)** module.

## 📋 Project Overview
Gizmo Electronics faced significant operational friction due to scattered contact data for customers and suppliers. This application solves that by providing a secure, role-based desktop environment to organize business relationships, track customer loyalty, and streamline daily operations.

## ✨ Key Features
* **Role-Based Access Control:** Admin and User tiers with hashed password security (SHA-256).
* **Smart Dashboard:** Real-time metrics on contact growth and loyalty performance.
* **CRM Capabilities:** Full CRUD functionality for Customers and Suppliers.
* **Loyalty Engine:** Automated point accrual and tier classification (Newbie, Regular, VIP).
* **Enterprise Utilities:** Bulk CSV Import/Export tools with template support.
* **Modern UI:** Bespoke dark-themed interface built using custom GDI+ rendering.

## 🛠 Tech Stack
| Layer | Technology |
| :--- | :--- |
| **UI** | C# WinForms (.NET 10) |
| **Data Access** | ADO.NET (`Microsoft.Data.SqlClient`) |
| **Database** | Microsoft SQL Server |
| **Architecture** | Service Layer pattern with custom `NavigationManager` |

## 🚀 Setup Instructions

### Prerequisites
* Visual Studio 2022 or later.
* .NET 10 SDK installed.
* SQL Server (Local or Network instance).

### 1. Database Configuration
Update your database connection settings before running the application:
1. Open `Helpers/AppConfig.cs` in Visual Studio.
2. Update the `DbServer` field with your SQL instance name (e.g., `localhost` or `.\SQLEXPRESS`).
3. If your SQL server requires a username/password, fill in the `DbUser` and `DbPassword` fields; otherwise, leave them empty to use Windows Authentication.

### 2. Database Initialization
* **Automatic:** Simply run the application. The system includes a `DatabaseHelper` class that creates the `ContactsDB` and all required tables automatically on the first launch.

### 3. Build & Run
1. Open `ContactManagementSystem.sln`.
2. Allow Visual Studio to restore NuGet packages.
3. Press `F5` to build and launch.

## 🔑 Default Credentials
Use these accounts to explore the system:

| Username | Password | Role | Access Level |
| :--- | :--- | :--- | :--- |
| **admin** | Admin@123 | Admin | Full (Import/Export, User Mgmt, Delete) |
| **user** | User@123 | User | Standard (Restricted) |

## 📂 Project Structure
```text
ContactManagementSystem/
├── Forms/          # UI Views and custom controls
├── Helpers/        # Infrastructure (Database, Session, Navigation)
├── Models/         # Data structures
├── Services/       # Business logic and SQL access
└── Program.cs      # App entry point   