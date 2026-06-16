using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ContactManagementSystem.Helpers;
using ContactManagementSystem.Models;

namespace ContactManagementSystem.Services
{
    internal static class LoyaltyService
    {
        // ── ADD POINTS FROM A PURCHASE ─────────────────────────
        // Call this whenever a customer makes a purchase
        // Every Rs. 100 spent = 1 point
        internal static void AddPoints(int customerID, decimal purchaseAmount, string description = "Purchase")
        {
            try
            {
                int pointsEarned = Customer.CalculatePoints(purchaseAmount);

                using var conn = DatabaseHelper.GetConnection();

                // Step 1 — update CustomerDetails totals
                var updateCmd = new SqlCommand(@"
                    UPDATE CustomerDetails SET
                        LoyaltyPoints    = LoyaltyPoints + @points,
                        TotalPurchases   = TotalPurchases + @amount,
                        LastPurchaseDate = GETDATE()
                    WHERE CustomerID = @id", conn);

                updateCmd.Parameters.AddWithValue("@points", pointsEarned);
                updateCmd.Parameters.AddWithValue("@amount", purchaseAmount);
                updateCmd.Parameters.AddWithValue("@id", customerID);
                updateCmd.ExecuteNonQuery();

                // Step 2 — log the transaction
                var logCmd = new SqlCommand(@"
                    INSERT INTO LoyaltyTransactions
                        (CustomerID, PointsEarned, PurchaseAmount, Description)
                    VALUES
                        (@id, @points, @amount, @desc)", conn);

                logCmd.Parameters.AddWithValue("@id", customerID);
                logCmd.Parameters.AddWithValue("@points", pointsEarned);
                logCmd.Parameters.AddWithValue("@amount", purchaseAmount);
                logCmd.Parameters.AddWithValue("@desc", description);
                logCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add loyalty points: {ex.Message}", ex);
            }
        }

        // ── GET TIER FROM POINTS ───────────────────────────────
        // Returns tier label based on point total
        // Newbie: 0-199 | Regular: 200-599 | VIP: 600+
        internal static string GetTier(int points)
        {
            return points switch
            {
                < 200 => "🌱 Newbie",
                < 600 => "⭐ Regular",
                _ => "👑 VIP"
            };
        }

        // ── GET TIER COLOR ─────────────────────────────────────
        // Returns a color matching the tier for UI display
        internal static System.Drawing.Color GetTierColor(int points)
        {
            return points switch
            {
                < 200 => System.Drawing.Color.FromArgb(100, 180, 100),  // green  — Newbie
                < 600 => System.Drawing.Color.FromArgb(250, 200, 50),   // gold   — Regular
                _ => System.Drawing.Color.FromArgb(100, 160, 255)   // blue   — VIP
            };
        }

        // ── GET CUSTOMER LOYALTY SUMMARY ──────────────────────
        // Returns points, tier, total purchases and last purchase date
        internal static LoyaltySummary GetSummary(int customerID)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT CustomerID, LoyaltyPoints, TotalPurchases,
                           LastPurchaseDate, MemberSince
                    FROM   CustomerDetails
                    WHERE  CustomerID = @id", conn);

                cmd.Parameters.AddWithValue("@id", customerID);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int points = reader.GetInt32(1);
                    return new LoyaltySummary
                    {
                        CustomerID = reader.GetInt32(0),
                        LoyaltyPoints = points,
                        Tier = GetTier(points),
                        TierColor = GetTierColor(points),
                        TotalPurchases = reader.GetDecimal(2),
                        LastPurchaseDate = reader.IsDBNull(3)
                                           ? null
                                           : reader.GetDateTime(3),
                        MemberSince = reader.GetDateTime(4),
                        PointsToNextTier = GetPointsToNextTier(points)
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get loyalty summary: {ex.Message}", ex);
            }
            return null;
        }

        // ── GET TRANSACTION HISTORY ────────────────────────────
        // Returns last N transactions for a customer
        internal static List<LoyaltyTransaction> GetHistory(int customerID, int limit = 10)
        {
            var list = new List<LoyaltyTransaction>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT TOP (@limit)
                           TransactionID, PointsEarned,
                           PurchaseAmount, Description, TransactionDate
                    FROM   LoyaltyTransactions
                    WHERE  CustomerID = @id
                    ORDER  BY TransactionDate DESC", conn);

                cmd.Parameters.AddWithValue("@limit", limit);
                cmd.Parameters.AddWithValue("@id", customerID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new LoyaltyTransaction
                    {
                        TransactionID = reader.GetInt32(0),
                        PointsEarned = reader.GetInt32(1),
                        PurchaseAmount = reader.GetDecimal(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        TransactionDate = reader.GetDateTime(4)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get transaction history: {ex.Message}", ex);
            }
            return list;
        }

        // ── GET ALL CUSTOMERS BY TIER ──────────────────────────
        // Useful for dashboard stats — how many in each tier
        internal static (int Newbie, int Regular, int VIP) GetTierCounts()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand(@"
                    SELECT
                        SUM(CASE WHEN LoyaltyPoints < 200  THEN 1 ELSE 0 END) AS Newbie,
                        SUM(CASE WHEN LoyaltyPoints BETWEEN 200 AND 599
                                                           THEN 1 ELSE 0 END) AS Regular,
                        SUM(CASE WHEN LoyaltyPoints >= 600 THEN 1 ELSE 0 END) AS VIP
                    FROM CustomerDetails", conn);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (
                        reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get tier counts: {ex.Message}", ex);
            }
            return (0, 0, 0);
        }

        // ── PRIVATE HELPER ─────────────────────────────────────
        // How many more points needed to reach next tier
        private static int GetPointsToNextTier(int points)
        {
            if (points < 200) return 200 - points;   // points to Regular
            if (points < 600) return 600 - points;   // points to VIP
            return 0;                                  // already VIP
        }
    }

    // ── SUPPORTING MODELS ──────────────────────────────────────
    // These are small data containers used only by LoyaltyService
    // No need to put them in the Models folder

    internal class LoyaltySummary
    {
        public int CustomerID { get; set; }
        public int LoyaltyPoints { get; set; }
        public string Tier { get; set; } = "";
        public System.Drawing.Color TierColor { get; set; }
        public decimal TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public DateTime MemberSince { get; set; }
        public int PointsToNextTier { get; set; }

        public string LastPurchaseFormatted =>
            LastPurchaseDate.HasValue
                ? LastPurchaseDate.Value.ToString("dd MMM yyyy")
                : "No purchases yet";

        public string MemberSinceFormatted =>
            MemberSince.ToString("dd MMM yyyy");
    }

    internal class LoyaltyTransaction
    {
        public int TransactionID { get; set; }
        public int PointsEarned { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string Description { get; set; } = "";
        public DateTime TransactionDate { get; set; }

        public string DateFormatted =>
            TransactionDate.ToString("dd MMM yyyy  hh:mm tt");
    }
}
