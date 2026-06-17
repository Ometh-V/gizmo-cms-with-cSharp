using System;
using System.Collections.Generic;
using System.Text;

namespace ContactManagementSystem.Models
{ 
    internal class Supplier : Contact
    {
        //suplier's basic data inherits from Contact
        public int SupplierID { get; set; }
        public string CompanyName { get; set; } = "";
        public string ProductCategory { get; set; } = "";
        public string PaymentTerms { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}