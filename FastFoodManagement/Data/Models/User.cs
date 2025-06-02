using System;
using System.ComponentModel.DataAnnotations;

namespace FastFoodManagement.Data.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string FullName { get; set; }
        
        public string Email { get; set; }
        
        public string Role { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
    }
}