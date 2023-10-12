using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRazorPage.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        [Required(ErrorMessage = "ProductName is required")]
        public string ProductName { get; set; } = null!;
        [Required(ErrorMessage = "CategoryId is required")]
        public int? CategoryId { get; set; }
        [Required(ErrorMessage = "QuantityPerUnit is required")]
        public string? QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        [Required(ErrorMessage = "UnitsInStock is required")]
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        [Required(ErrorMessage = "Discontinued is required")]
        public bool Discontinued { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
