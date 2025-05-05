using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for ForeignKey

namespace HomeownersAssociation.Models
{
    public class EmergencyContact
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Relationship { get; set; } = string.Empty; // e.g., Spouse, Parent, Friend

        // Foreign Key to the resident this contact belongs to
        [Required]
        public string ResidentId { get; set; } = string.Empty;

        [ForeignKey("ResidentId")]
        public virtual ApplicationUser? Resident { get; set; } // Navigation property
    }
}