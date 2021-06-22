using System;
using System.Collections.Generic;

namespace EatCleanAPI.Models
{
    public partial class MenuDetails
    {
        public int MenuId { get; set; }
        public int ProductId { get; set; }
        public int? Quantity { get; set; }

        public virtual Menus Menu { get; set; }
        public virtual Products Product { get; set; }
    }
}
