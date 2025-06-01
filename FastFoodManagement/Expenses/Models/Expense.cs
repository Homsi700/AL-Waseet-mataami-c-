using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodManagement.Expenses.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public string ReceiptImage { get; set; }
        
        // Additional properties
        public string PaymentMethod { get; set; }
        
        public string ReferenceNumber { get; set; }
        
        public bool IsRecurring { get; set; }
        
        public string RecurringFrequency { get; set; }
        
        [StringLength(100)]
        public string Vendor { get; set; }
        
        public string CreatedBy { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public string ModifiedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
    }
}