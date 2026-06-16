using System;
using System.Collections.Generic;
using System.Text;

namespace ContactManagementSystem.Models
{
    internal class Contact
    {
        public int ContactID { get; set; }
        public string ContactType { get; set; } = "Customer";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FullName =>
            $"{FirstName} {LastName}".Trim();

        public string Initials =>
            $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}".ToUpper().Trim() +
            $"{(LastName.Length > 0 ? LastName[0] : ' ')}".ToUpper().Trim();

    }
}
