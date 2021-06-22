using System;
using System.Collections.Generic;

namespace EatCleanAPI.Models
{
    public partial class Products
    {
        public Products()
        {
            MenuDetails = new HashSet<MenuDetails>();
        }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public int? Calories { get; set; }
        public float? Protein { get; set; }
        public float? Carb { get; set; }
        public float? Fat { get; set; }

        public virtual ICollection<MenuDetails> MenuDetails { get; set; }
    }
}
