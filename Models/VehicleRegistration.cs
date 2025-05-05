using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for ForeignKey

namespace HomeownersAssociation.Models
{
    public class VehicleRegistration
    {
        public int Id { get; set; }

        [Required]
        public string Make { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;

        [Required]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        // Foreign Key to the resident owner (assuming ApplicationUser is your Identity user)
        [Required]
        public string ResidentOwnerId { get; set; } = string.Empty;

        [ForeignKey("ResidentOwnerId")]
        public virtual ApplicationUser? ResidentOwner { get; set; } // Navigation property

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}