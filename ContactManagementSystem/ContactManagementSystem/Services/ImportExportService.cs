using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;

namespace ContactManagementSystem.Services
{
    internal static class ImportExportService
    {
        // ── EXPORT TO CSV ──────────────────────────────────────
        // Saves all contacts to a CSV file at the given path
        // Returns number of contacts exported
        internal static int ExportToCSV(string filePath)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT c.ContactID, c.ContactType,
                           c.FirstName,  c.LastName,
                           c.Phone,      c.Email,
                           c.Address,    c.Notes,
                           c.CreatedAt,
                           -- Customer fields (NULL for suppliers)
                           cd.LoyaltyPoints, cd.TotalPurchases,
                           -- Supplier fields (NULL for customers)
                           sd.CompanyName, sd.ProductCategory, sd.PaymentTerms
                    FROM   Contacts c
                    LEFT JOIN CustomerDetails cd ON c.ContactID = cd.ContactID
                    LEFT JOIN SupplierDetails sd ON c.ContactID = sd.ContactID
                    ORDER  BY c.FirstName, c.LastName", conn);

                using var reader = cmd.ExecuteReader();
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                // Write CSV header row
                writer.WriteLine(
                    "ContactID,ContactType,FirstName,LastName," +
                    "Phone,Email,Address,Notes,CreatedAt," +
                    "LoyaltyPoints,TotalPurchases," +
                    "CompanyName,ProductCategory,PaymentTerms");

                int count = 0;
                while (reader.Read())
                {
                    // Escape each field — wrap in quotes to handle commas
                    var line = string.Join(",",
                        Escape(reader.GetInt32(0).ToString()),
                        Escape(reader.GetString(1)),
                        Escape(reader.IsDBNull(2) ? "" : reader.GetString(2)),
                        Escape(reader.IsDBNull(3) ? "" : reader.GetString(3)),
                        Escape(reader.IsDBNull(4) ? "" : reader.GetString(4)),
                        Escape(reader.IsDBNull(5) ? "" : reader.GetString(5)),
                        Escape(reader.IsDBNull(6) ? "" : reader.GetString(6)),
                        Escape(reader.IsDBNull(7) ? "" : reader.GetString(7)),
                        Escape(reader.GetDateTime(8).ToString("yyyy-MM-dd HH:mm:ss")),
                        Escape(reader.IsDBNull(9) ? "" : reader.GetInt32(9).ToString()),
                        Escape(reader.IsDBNull(10) ? "" : reader.GetDecimal(10).ToString()),
                        Escape(reader.IsDBNull(11) ? "" : reader.GetString(11)),
                        Escape(reader.IsDBNull(12) ? "" : reader.GetString(12)),
                        Escape(reader.IsDBNull(13) ? "" : reader.GetString(13))
                    );
                    writer.WriteLine(line);
                    count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Export failed: {ex.Message}", ex);
            }
        }

        // ── IMPORT FROM CSV ────────────────────────────────────
        // Reads a CSV file and inserts contacts into the database
        // Returns an ImportResult with success/fail counts
        internal static ImportResult ImportFromCSV(string filePath)
        {
            var result = new ImportResult();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    result.Errors.Add("CSV file is empty or has no data rows.");
                    return result;
                }

                // Skip header row (index 0) — start from index 1
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        var fields = ParseCSVLine(line);

                        // Validate minimum required fields
                        if (fields.Length < 4)
                        {
                            result.Errors.Add($"Row {i + 1}: Not enough fields — skipped.");
                            result.Failed++;
                            continue;
                        }

                        string contactType = fields.Length > 1 ? fields[1] : "Customer";
                        string firstName = fields.Length > 2 ? fields[2] : "";
                        string lastName = fields.Length > 3 ? fields[3] : "";

                        if (string.IsNullOrWhiteSpace(firstName) ||
                            string.IsNullOrWhiteSpace(lastName))
                        {
                            result.Errors.Add($"Row {i + 1}: First or last name is empty — skipped.");
                            result.Failed++;
                            continue;
                        }

                        if (contactType != "Customer" && contactType != "Supplier")
                            contactType = "Customer"; // default to Customer

                        // Build contact object
                        Contact contact = contactType == "Supplier"
                            ? new Supplier
                            {
                                CompanyName = fields.Length > 11 ? fields[11] : "",
                                ProductCategory = fields.Length > 12 ? fields[12] : "",
                                PaymentTerms = fields.Length > 13 ? fields[13] : ""
                            }
                            : new Contact();

                        contact.ContactType = contactType;
                        contact.FirstName = firstName;
                        contact.LastName = lastName;
                        contact.Phone = fields.Length > 4 ? fields[4] : "";
                        contact.Email = fields.Length > 5 ? fields[5] : "";
                        contact.Address = fields.Length > 6 ? fields[6] : "";
                        contact.Notes = fields.Length > 7 ? fields[7] : "";

                        // Insert into database
                        ContactService.Add(contact);
                        result.Succeeded++;
                    }
                    catch (Exception rowEx)
                    {
                        result.Errors.Add($"Row {i + 1}: {rowEx.Message}");
                        result.Failed++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Import failed: {ex.Message}", ex);
            }

            return result;
        }

        // ── GENERATE SAMPLE CSV ────────────────────────────────
        // Creates a template CSV file so users know the format
        internal static void GenerateSampleCSV(string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                writer.WriteLine(
                    "ContactID,ContactType,FirstName,LastName," +
                    "Phone,Email,Address,Notes,CreatedAt," +
                    "LoyaltyPoints,TotalPurchases," +
                    "CompanyName,ProductCategory,PaymentTerms");

                // Sample customer row
                writer.WriteLine(
                    ",Customer,Ashan,Kumara," +
                    "0771234567,ashan@email.com," +
                    "Colombo 03,Regular customer,," +
                    ",,,,");

                // Sample supplier row
                writer.WriteLine(
                    ",Supplier,Nimal,Perera," +
                    "0112345678,nimal@supplier.com," +
                    "Kandy,Main supplier,,," +
                    ",Nimal Supplies,Electronics,Net 30");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate sample CSV: {ex.Message}", ex);
            }
        }

        // ── PRIVATE HELPERS ────────────────────────────────────

        // Wraps a field in quotes and escapes any quotes inside it
        private static string Escape(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        // Parses a single CSV line — handles quoted fields with commas inside
        private static string[] ParseCSVLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Handle escaped quote ("")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString().Trim());
            return fields.ToArray();
        }
    }

    // ── IMPORT RESULT ──────────────────────────────────────────
    internal class ImportResult
    {
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
        public int Total => Succeeded + Failed;

        public string Summary =>
            $"Import complete.\n\n" +
            $"✅ Imported: {Succeeded}\n" +
            $"❌ Failed:   {Failed}\n" +
            $"📋 Total:    {Total}";
    }
}
