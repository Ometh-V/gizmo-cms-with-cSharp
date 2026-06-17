using System;
using System.Collections.Generic;
using System.Text;

namespace ContactManagementSystem.Models
{
    internal class Group
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = "";
        public string Description { get; set; } = "";
        public int ContactCount { get; set; }

        // for display in listboxes
        public override string ToString() => GroupName;
    }
}
