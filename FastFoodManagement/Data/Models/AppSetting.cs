using System.ComponentModel.DataAnnotations;

namespace FastFoodManagement.Data.Models
{
    public class AppSetting
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SettingKey { get; set; }
        
        [Required]
        public string SettingValue { get; set; }
        
        public string SettingGroup { get; set; }
        
        public string Description { get; set; }
    }
}