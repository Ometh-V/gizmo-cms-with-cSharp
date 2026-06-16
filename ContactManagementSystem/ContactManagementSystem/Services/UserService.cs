using System;
using System.Data;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using ContactManagementSystem.Helpers;

namespace ContactManagementSystem.Services
{
    internal static class UserService
    {
        internal static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }


        internal static bool Login(string username, string password)
        {
            try
            {
                var hash = HashPassword(password);
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT UserID, Username, Role
                    FROM Users
                    WHERE Username = @username
                    AND PasswordHash =@hash
                    AND IsActive = 1", conn);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", hash);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Session.UserID = reader.GetInt32(0);
                    Session.UserName = reader.GetString(1);
                    Session.Role = reader.GetString(2);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Login Failed: {ex.Message}", ex);
            }
        }


        internal static DataTable GetAllUsers()
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
                "SELECT UserID, Username, Role, IsActive, CreatedAt FROM Users ORDER BY Username", conn);
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }


       
        internal static void AddUser(string username, string password, string role)
        {
            try
            {
                // Check for duplicate username first
                if (UsernameExists(username))
                    throw new Exception($"Username '{username}' already exists.");

                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    INSERT INTO Users (Username, PasswordHash, Role)
                    VALUES (@username, @hash, @role)", conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", HashPassword(password));
                cmd.Parameters.AddWithValue("@role", role);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add user: {ex.Message}", ex);
            }
        }


        internal static void DeactivateUser(int userId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("UPDATE Users SET IsActive = 0 WHERE UserID = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to deactivate user: {ex.Message}", ex);
            }
        }


        
        internal static void ActivateUser(int userId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("UPDATE Users SET IsActive = 1 WHERE UserID = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to activate user: {ex.Message}", ex);
            }
        }


       
        internal static void ChangeRole(int userId, string newRole)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("UPDATE Users SET Role = @role WHERE UserID = @id", conn);
                cmd.Parameters.AddWithValue("@role", newRole);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change role: {ex.Message}", ex);
            }
        }


        
        internal static bool UsernameExists(string username)
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }


        
        // Used to prevent deactivating/demoting the last admin
        internal static int GetActiveAdminCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Role = 'Admin' AND IsActive = 1", conn);
            return (int)cmd.ExecuteScalar();
        }


        
        internal static void ResetPassword(int userId, string newPassword)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(
                    "UPDATE Users SET PasswordHash = @hash WHERE UserID = @id", conn);
                cmd.Parameters.AddWithValue("@hash", HashPassword(newPassword));
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reset password: {ex.Message}", ex);
            }
        }
    }
}