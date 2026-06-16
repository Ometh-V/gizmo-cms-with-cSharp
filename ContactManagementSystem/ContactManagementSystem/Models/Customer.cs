using System;
using System.Text;

namespace ContactManagementSystem.Models
{
    internal class Customer : Contact
    {
        //Customer's basic informations inherit from Contact Class
        public int CustomerID { get; set; }
        public int LoyaltyPoints { get; set; }
        public decimal TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public DateTime MemberSince { get; set; }

        public string LoyaltyTier => LoyaltyPoints switch
        {
            < 200 => "🌱 Newbie",
            < 600 => "⭐ Regular",
            _ => "👑 VIP"
        };

        public static int CalculatePoints(decimal purchaseAmount) => (int)(purchaseAmount / 100);
    }
}
