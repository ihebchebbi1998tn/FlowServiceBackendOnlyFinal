using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsolidatedApi.Models
{
    public class Address
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string StreetName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string CountryKey { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string ZipCode { get; set; } = string.Empty;
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        [ForeignKey("UserId")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}