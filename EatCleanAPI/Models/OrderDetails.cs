using System;
using System.Collections.Generic;

namespace EatCleanAPI.Models
{
    public partial class OrderDetails
    {
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public int? Quantity { get; set; }

        public virtual Menus Menu { get; set; }
        public virtual Orders Order { get; set; }
    }
}
