using System;
using System.Collections.Generic;

namespace EatCleanAPI.Models
{
    public partial class Payments
    {
        public Payments()
        {
            Orders = new HashSet<Orders>();
        }

        public int PaymentId { get; set; }
        public bool? Status { get; set; }
        public string UpdateTime { get; set; }
        public string EmailAddress { get; set; }

        public virtual ICollection<Orders> Orders { get; set; }
    }
}
