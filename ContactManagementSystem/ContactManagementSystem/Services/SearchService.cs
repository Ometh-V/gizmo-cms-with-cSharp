using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;

namespace ContactManagementSystem.Services
{
    internal static class SearchService
    {
        // Search contacts
        internal static List<Contact> Search(string query)
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM   Contacts
                    WHERE  FirstName LIKE @q
                    OR     LastName  LIKE @q
                    OR     Phone     LIKE @q
                    OR     Email     LIKE @q
                    OR     (FirstName + ' ' + LastName) LIKE @q
                    ORDER  BY FirstName, LastName", conn);

                cmd.Parameters.AddWithValue("@q", $"%{query}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Search Failed: {ex.Message}", ex);
            }
            return list;
        }


        // filter by contact type
        internal static List<Contact> FilterByType(string type)
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM   Contacts
                    WHERE  ContactType = @type
                    ORDER  BY FirstName, LastName", conn);

                cmd.Parameters.AddWithValue("@type", type);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Filter failed: {ex.Message}", ex);
            }
            return list;
        }


        // Sort contacts
        internal static List<Contact> GetSorted(string sortBy)
        {
            var list = new List<Contact>();
            try
            {
                string orderClause = sortBy switch
                {
                    "A-Z" => "ORDER BY FirstName ASC, LastName ASC",
                    "Z-A" => "ORDER BY FirstName DESC, LastName DESC",
                    "Newest" => "ORDER BY CreatedAt DESC",
                    "Oldest" => "ORDER BY CreatedAt ASC",
                    _ => "ORDER BY FirstName ASC"
                };

                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand($@"
                    SELECT ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM   Contacts
                    {orderClause}", conn);


                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Sort Failed: {ex.Message}", ex);
            }
            return list;
        }


        // search in a group
        internal static List<Contact> SearchInGroup(string query, int groupId)
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT c.ContactID, c.ContactType, c.FirstName, c.LastName,
                           c.Phone, c.Email, c.Address, c.Notes,
                           c.CreatedAt, c.UpdatedAt
                    FROM   Contacts c
                    JOIN   ContactGroups cg ON c.ContactID = cg.ContactID
                    WHERE  cg.GroupID = @groupId
                    AND   (c.FirstName LIKE @q OR c.LastName LIKE @q
                    OR     c.Phone     LIKE @q OR c.Email   LIKE @q)
                    ORDER  BY c.FirstName", conn);

                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.Parameters.AddWithValue("@q", $"%{query}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapContact(reader));
            }
            catch (Exception ex)
            {
                throw new Exception($"Group Search Failed: {ex.Message}", ex);
            }
            return list;
        }


        // private mapper
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
    }
}
