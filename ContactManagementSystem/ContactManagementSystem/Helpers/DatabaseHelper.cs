using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ContactManagementSystem.Helpers
{
    internal static class DatabaseHelper
    {
        // Connecting to the master database first before creating the contacts DB
        /* Make a AppConfig.cs and in it create
            internal static string DbServer = "yourSQLServer";        or .\SQLEXPRESS
            internal static string DbName = "ContactsDB";
            internal static string DbUser = "yourUser";
            internal static string DbPassword = "yourPassword"; 
         
         */

        private static readonly string MasterConnection =
            $"Server={AppConfig.DbServer};Database=master;User Id={AppConfig.DbUser};Password={AppConfig.DbPassword};TrustServerCertificate=true;";



        private static readonly string ConnectionString =
            $"Server={AppConfig.DbServer};Database={AppConfig.DbName};User Id={AppConfig.DbUser};Password={AppConfig.DbPassword};TrustServerCertificate=true;";

        internal static SqlConnection GetConnection()
        {
            try
            {
                var conn = new SqlConnection(ConnectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Connection Failed: {ex.Message}", ex);
            }
        }

        internal static void InitializeDatabase()
        {

            try
            {
                // Creating the Database if the Database doesnt exists by connecting to the master database
                using (var masterConn = new SqlConnection(MasterConnection))
                {
                    masterConn.Open();
                    var createDb = masterConn.CreateCommand();
                    createDb.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ContactsDB')
                    CREATE DATABASE ContactsDB;
                ";
                    createDb.ExecuteNonQuery();
                }

                    // After creating the Database creates the tables if the Tables doesnt exists
                    using var conn = GetConnection();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                
                    -- Creating the Contacts Table
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Contacts' AND xtype='U')
                    CREATE TABLE Contacts (
                        ContactID   INT IDENTITY(1,1) PRIMARY KEY,
                        ContactType NVARCHAR(20)  NOT NULL CHECK (ContactType IN ('Customer','Supplier')),
                        FirstName   NVARCHAR(100) NOT NULL,
                        LastName    NVARCHAR(100) NOT NULL,
                        Phone       NVARCHAR(50),
                        Email       NVARCHAR(200),
                        Address     NVARCHAR(300),
                        Notes       NVARCHAR(MAX),
                        CreatedAt   DATETIME DEFAULT GETDATE(),
                        UpdatedAt   DATETIME DEFAULT GETDATE()
                    );


                    -- Creating the Customer Details
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CustomerDetails' AND xtype='U')
                    CREATE TABLE CustomerDetails (
                        CustomerID       INT IDENTITY(1,1) PRIMARY KEY,
                        ContactID        INT NOT NULL,
                        LoyaltyPoints    INT DEFAULT 0,
                        TotalPurchases   DECIMAL(10,2) DEFAULT 0,
                        LastPurchaseDate DATETIME,
                        MemberSince      DATETIME DEFAULT GETDATE(),
                        CONSTRAINT FK_CustomerDetails_Contact
                            FOREIGN KEY (ContactID) REFERENCES Contacts(ContactID)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE
                    );


                    -- Creating the Supplier Details
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SupplierDetails' AND xtype='U')
                    CREATE TABLE SupplierDetails (
                        SupplierID      INT IDENTITY(1,1) PRIMARY KEY,
                        ContactID       INT NOT NULL,
                        CompanyName     NVARCHAR(200),
                        ProductCategory NVARCHAR(100),
                        PaymentTerms    NVARCHAR(100),
                        IsActive        BIT DEFAULT 1,
                        CONSTRAINT FK_SupplierDetails_Contact
                            FOREIGN KEY (ContactID) REFERENCES Contacts(ContactID)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE
                    );


                    -- Creating Loyalty transactions
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LoyaltyTransactions' AND xtype='U')
                    CREATE TABLE LoyaltyTransactions (
                        TransactionID   INT IDENTITY(1,1) PRIMARY KEY,
                        CustomerID      INT NOT NULL,
                        PointsEarned    INT NOT NULL,
                        PurchaseAmount  DECIMAL(10,2),
                        Description     NVARCHAR(200),
                        TransactionDate DATETIME DEFAULT GETDATE(),
                        CONSTRAINT FK_LoyaltyTransactions_Customer
                            FOREIGN KEY (CustomerID) REFERENCES CustomerDetails(CustomerID)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE
                    );


                    -- Creating Groups
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Groups' AND xtype='U')
                    CREATE TABLE Groups (
                        GroupID     INT IDENTITY(1,1) PRIMARY KEY,
                        GroupName   NVARCHAR(100) NOT NULL UNIQUE,
                        Description NVARCHAR(300)
                    );


                    -- Creating Contact-Group link table
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ContactGroups' AND xtype='U')
                    CREATE TABLE ContactGroups (
                        ContactID INT NOT NULL,
                        GroupID   INT NOT NULL,
                        PRIMARY KEY (ContactID, GroupID),
                        CONSTRAINT FK_ContactGroups_Contact
                            FOREIGN KEY (ContactID) REFERENCES Contacts(ContactID)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE,
                        CONSTRAINT FK_ContactGroups_Group
                            FOREIGN KEY (GroupID) REFERENCES Groups(GroupID)
                            ON DELETE CASCADE
                            ON UPDATE CASCADE
                    );

                    
                    -- Creating Users table
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                    CREATE TABLE Users (
                        UserID       INT IDENTITY(1,1) PRIMARY KEY,
                        Username     NVARCHAR(100) NOT NULL UNIQUE,
                        PasswordHash NVARCHAR(256) NOT NULL,
                        Role         NVARCHAR(20)  NOT NULL CHECK (Role IN ('Admin','User')),
                        IsActive     BIT DEFAULT 1,
                        CreatedAt    DATETIME DEFAULT GETDATE()
                    );


                    -- Insert default admin account (Password: Admin@123)
                    IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
                    INSERT INTO Users (Username, PasswordHash, Role)
                    VALUES ('admin', 'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', 'Admin');


                    -- Insert default user account (Password: User@123)
                    IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'user')
                    INSERT INTO Users (Username, PasswordHash, Role)
                    VALUES ('user', '3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 'User');
                    ";
                    cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to Initialize Database Tables: {ex.Message}", ex);
            }
            
        }
    }
}
