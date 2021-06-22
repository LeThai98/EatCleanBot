using System;
using System.Collections.Generic;

namespace EatCleanAPI.Models
{
    public partial class Menus
    {
        public Menus()
        {
            MenuDetails = new HashSet<MenuDetails>();
            OrderDetails = new HashSet<OrderDetails>();
        }

        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public virtual ICollection<MenuDetails> MenuDetails { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
