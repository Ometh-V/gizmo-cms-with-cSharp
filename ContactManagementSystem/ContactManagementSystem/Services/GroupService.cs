using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;

namespace ContactManagementSystem.Services
{
    internal static class GroupService
    {
        // Get all groups
        internal static List<Group> GetAll()
        {
            var list = new List<Group>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT g.GroupID, g.GroupName, g.Description,
                           COUNT(cg.ContactID) AS ContactCount
                    FROM Groups g
                    LEFT JOIN ContactGroups cg ON g.GroupID = cg.GroupID
                    GROUP BY g.GroupID, g.GroupName, g.Description
                    ORDER BY g.GroupName", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Group
                    {
                        GroupID = reader.GetInt32(0),
                        GroupName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ContactCount = reader.GetInt32(3),
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to Load Groups: {ex.Message}", ex);
            }
            return list;
        }


        internal static void Add(String groupName, string description = "")
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    INSERT INTO Groups (GroupName, Description)
                    VALUES (@name, @desc)", conn);
                cmd.Parameters.AddWithValue("@name", groupName.Trim());
                cmd.Parameters.AddWithValue("@desc", description.Trim());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add group: {ex.Message}", ex);
            }
        }


        // rename a group
        internal static void Rename(int groupId, string newName)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    UPDATE Groups SET GroupName = @name
                    WHERE GroupID = @id", conn);
                cmd.Parameters.AddWithValue("@name", newName.Trim());
                cmd.Parameters.AddWithValue("@id", groupId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to rename group: {ex.Message}", ex);
            }
        }


        // Delete group 
        internal static void Delete(int groupId)
        {
            try
            {
                var conn = DatabaseHelper.GetConnection();
                // ContactGroups table rows will be deleted automatically because of cascade
                var cmd = new SqlCommand("DELETE FROM Groups WHERE GroupID = @id", conn);
                cmd.Parameters.AddWithValue("@id", groupId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete group: {ex.Message}", ex);
            }
        }


        // assigning a contact to a group
        internal static void AssignContact(int contactId, int groupId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM ContactGroups
                        WHERE ContactID = @cid and GroupID = @gid)
                    INSERT INTO ContactGroups (ContactID, GroupID)
                    VALUES (@cid, @gid)", conn);
                cmd.Parameters.AddWithValue("@cid", contactId);
                cmd.Parameters.AddWithValue("@gid", groupId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to assign contact: {ex.Message}", ex);
            }
        }


        // remove contact from a group
        internal static void RemoveContact(int contactId, int groupId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    DELETE FROM ContactGroups
                    WHERE ContactID = @cid AND GroupID = @gid", conn);
                cmd.Parameters.AddWithValue("@cid", contactId);
                cmd.Parameters.AddWithValue("@gid", groupId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove contact: {ex.Message}", ex);
            }
        }


        // get contacts that are in a group
        internal static List<Contact> GetContactsInGroup(int groupId)
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
                    WHERE  cg.GroupID = @gid
                    ORDER  BY c.FirstName", conn);
                cmd.Parameters.AddWithValue("@gid", groupId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Contact
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
                        UpdatedAt = reader.GetDateTime(9)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get group contacts: {ex.Message}", ex);
            }
            return list;
        }


        //  get contatcs that are not in a group
        internal static List<Contact> GetContactsNotInGroup(int groupId)
        {
            var list = new List<Contact>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT ContactID, ContactType, FirstName, LastName,
                           Phone, Email, Address, Notes, CreatedAt, UpdatedAt
                    FROM   Contacts
                    WHERE  ContactID NOT IN (
                        SELECT ContactID FROM ContactGroups
                        WHERE GroupID = @gid)
                    ORDER  BY FirstName", conn);
                cmd.Parameters.AddWithValue("@gid", groupId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Contact
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
                        UpdatedAt = reader.GetDateTime(9)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available contacts: {ex.Message}", ex);
            }
            return list;
        }


        // get the count of groups
        internal static int GetCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            return (int)new SqlCommand(
                "SELECT COUNT(*) FROM Groups", conn).ExecuteScalar();
        }
    }
}
