using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContactManagementSystem.Services
{
    internal class ContactService
    {
        // to get all
        internal static List<Contact> GetAll()
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM Contacts
                    ORDER BY FirstName, LastName", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load contacts: {ex.Message}", ex);
            }
            return list;
        }


        // to get by ID
        internal static Contact GetById(int contactId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT ContactID, ContactType, FirstName, LastName, 
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM Contacts
                    WHERE ContactID = @id", conn);
                cmd.Parameters.AddWithValue("@id", contactId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read()) return MapContact(reader);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get contact: {ex.Message}", ex);
            }
            return null;
        }


        // to get customer details
        internal static Customer GetCustomerDetails(int contactId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT c.ContactID, c.ContactType, c.FirstName, c.LastName,
                           c.Phone, c.Email, c.Address, c.Notes,
                           c.CreatedAt, c.updatedAt,
                           cd.CustomerID, cd.loyaltyPoints, cd.TotalPurchases,
                           cd.LastPurchaseDate, cd.MemberSince
                    FROM Contacts c
                    JOIN CustomerDetails cd ON c.ContactID = cd.ContactID
                    WHERE c.ContactID = @id", conn);
                cmd.Parameters.AddWithValue("@id", contactId);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null; //Removed second reader.Read


                return new Customer
                {
                    ContactID = reader.GetInt32(0),
                    ContactType = reader.GetString(1),
                    FirstName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    LastName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Notes = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9),
                    CustomerID = reader.GetInt32(10),
                    LoyaltyPoints = reader.GetInt32(11),
                    TotalPurchases = reader.GetDecimal(12),
                    LastPurchaseDate = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                    MemberSince = reader.GetDateTime(14)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get customer details: {ex.Message}", ex);
            }

        }


        // to get supplier details
        internal static Supplier GetSupplierDetails(int contactId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT c.ContactID, c.ContactType, c.FirstName, c.LastName,
                           c.Phone, c.Email, c.Address, c.Notes,
                           c.CreatedAt, c.UpdatedAt,
                           sd.SupplierID, sd.CompanyName,
                           sd.ProductCategory, sd.PaymentTerms, sd.IsActive
                    FROM   Contacts c
                    JOIN   SupplierDetails sd ON c.ContactID = sd.ContactID
                    WHERE  c.ContactID = @id", conn);
                cmd.Parameters.AddWithValue("@id", contactId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Supplier
                    {
                        ContactID = reader.GetInt32(0),
                        ContactType = reader.GetString(1),
                        FirstName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        LastName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        Notes = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        CreatedAt = reader.GetDateTime(8),
                        UpdatedAt = reader.GetDateTime(9),
                        SupplierID = reader.GetInt32(10),
                        CompanyName = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        ProductCategory = reader.IsDBNull(12) ? "" : reader.GetString(12),
                        PaymentTerms = reader.IsDBNull(13) ? "" : reader.GetString(13),
                        IsActive = reader.GetBoolean(14)
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get supplier details: {ex.Message}", ex);
            }
            return null;
        }


        // add contacts into the system
        internal static int Add(Contact c)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    INSERT INTO Contacts
                        (ContactType, FirstName, LastName,
                         Phone, Email, Address, Notes)
                    OUTPUT INSERTED.ContactID
                    VALUES
                        (@type, @first, @last,
                         @phone, @email, @address, @notes)", conn);

                cmd.Parameters.AddWithValue("@type", c.ContactType);
                cmd.Parameters.AddWithValue("@first", c.FirstName);
                cmd.Parameters.AddWithValue("@last", c.LastName);
                cmd.Parameters.AddWithValue("@phone", (object)c.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@email", (object)c.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@address", (object)c.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", (object)c.Notes ?? DBNull.Value);

                int newId = (int)cmd.ExecuteScalar();

                if (c.ContactType == "Customer")
                    AddCustomerDetails(newId);

                if (c.ContactType == "Supplier")
                    AddSupplierDetails(newId, c as Supplier);

                return newId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add contact: {ex.Message}", ex);
            }
        }


        // update contacts
        internal static void Update(Contact c)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    UPDATE Contacts SET
                        FirstName = @first,
                        LastName  = @last,
                        Phone     = @phone,
                        Email     = @email,
                        Address   = @address,
                        Notes     = @notes,
                        UpdatedAt = GETDATE()
                    WHERE ContactID = @id", conn);

                cmd.Parameters.AddWithValue("@first", c.FirstName);
                cmd.Parameters.AddWithValue("@last", c.LastName);
                cmd.Parameters.AddWithValue("@phone", (object)c.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@email", (object)c.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@address", (object)c.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", (object)c.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", c.ContactID);
                cmd.ExecuteNonQuery();

                if (c.ContactType == "Supplier" && c is Supplier s)
                    UpdateSupplierDetails(s);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update contact: {ex.Message}", ex);
            }
        }


        // delete contacts
        internal static void Delete(int contactId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Contacts WHERE ContactID = @id";
                cmd.Parameters.AddWithValue("@id", contactId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete contact: {ex.Message}", ex);
            }
        }


        // counters needed for the dashboard
        internal static int GetTotalCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            return (int)new SqlCommand(
                "SELECT COUNT(*) FROM Contacts", conn).ExecuteScalar();
        }

        internal static int GetCustomerCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            return (int)new SqlCommand(
                "SELECT COUNT(*) FROM Contacts WHERE ContactType='Customer'",
                conn).ExecuteScalar();
        }


        internal static int GetSupplierCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            return (int)new SqlCommand(
                "SELECT COUNT(*) FROM Contacts WHERE ContactType='Supplier'",
                conn).ExecuteScalar();
        }


        internal static int GetRecentCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            return (int)new SqlCommand(@"
                SELECT COUNT(*) FROM Contacts
                WHERE  CreatedAt >= DATEADD(day, -30, GETDATE())",
                conn).ExecuteScalar();
        }


        internal static List<Contact> GetRecentlyAdded(int count = 5)
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT TOP (@count)
                           ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM Contacts
                    ORDER BY CreatedAt DESC", conn);
                cmd.Parameters.AddWithValue("@count", count);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load recently added contacts: {ex.Message}", ex);
            }
            return list;
        }


        // private helpers
        private static Contact MapContact(SqlDataReader r)
        {
            return new Contact
            {
                ContactID = r.GetInt32(0),
                ContactType = r.GetString(1),
                FirstName = r.IsDBNull(2) ? "" : r.GetString(2),
                LastName = r.IsDBNull(3) ? "" : r.GetString(3),
                Phone = r.IsDBNull(4) ? "" : r.GetString(4),
                Email = r.IsDBNull(5) ? "" : r.GetString(5),
                Address = r.IsDBNull(6) ? "" : r.GetString(6),
                Notes = r.IsDBNull(7) ? "" : r.GetString(7),
                CreatedAt = r.GetDateTime(8),
                UpdatedAt = r.GetDateTime(9)
            };
        }


        private static void AddCustomerDetails(int contactId)
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO CustomerDetails
                    (ContactID, LoyaltyPoints, TotalPurchases)
                VALUES (@id, 0, 0)", conn);
            cmd.Parameters.AddWithValue("@id", contactId);
            cmd.ExecuteNonQuery();
        }


        private static void AddSupplierDetails(
            int contactId, Supplier s)
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO SupplierDetails
                    (ContactID, CompanyName, ProductCategory, PaymentTerms, IsActive)
                VALUES (@id, @company, @category, @terms, 1)", conn);
            cmd.Parameters.AddWithValue("@id", contactId);
            cmd.Parameters.AddWithValue("@company", (object)s?.CompanyName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@category", (object)s?.ProductCategory ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@terms", (object)s?.PaymentTerms ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }


        private static void UpdateSupplierDetails(Supplier s)
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE SupplierDetails SET
                    CompanyName     = @company,
                    ProductCategory = @category,
                    PaymentTerms    = @terms
                WHERE ContactID = @id", conn);
            cmd.Parameters.AddWithValue("@company", (object)s.CompanyName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@category", (object)s.ProductCategory ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@terms", (object)s.PaymentTerms ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", s.ContactID);
            cmd.ExecuteNonQuery();
        }

        // ?? BULK DELETE ??????????????????????????????????????????
public static void BulkDeleteContacts(List<int> contactIds)
{
    if (contactIds == null || contactIds.Count == 0) return;

    // Converts a list like into a string "1,5,8"
    string idsString = string.Join(",", contactIds);

    try
    {
        using var conn = DatabaseHelper.GetConnection();
        if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

        // The IN operator deletes all matching IDs simultaneously
        string query = $"DELETE FROM Contacts WHERE ContactID IN ({idsString})";
        
        using var cmd = new SqlCommand(query, conn);
        cmd.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to bulk delete contacts: {ex.Message}", ex);
    }
}
    }
}