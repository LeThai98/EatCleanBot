using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EatCleanAPI.Models
{
    public partial class Payment
    {
        public Payment()
        {
            Orders = new HashSet<Order>();
        }

        public int PaymentId { get; set; }
        public bool? Status { get; set; }
        public string UpdateTime { get; set; }
        public string EmailAddress { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
