using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastFoodManagement.Data.Models
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public byte[] Image { get; set; }

        // Navigation property
        public virtual ICollection<Product> Products { get; set; }
    }
}