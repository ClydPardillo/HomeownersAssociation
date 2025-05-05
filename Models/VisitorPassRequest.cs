using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for ForeignKey

namespace HomeownersAssociation.Models
{
    public class VisitorPassRequest
    {
        public int Id { get; set; }

        [Required]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        public DateTime VisitStartDate { get; set; }

        [Required]
        public DateTime VisitEndDate { get; set; }

        [Required]
        public string VehicleMake { get; set; } = string.Empty;

        [Required]
        public string VehicleModel { get; set; } = string.Empty;

        [Required]
        public string VehicleLicensePlate { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // e.g., Pending, Approved, Denied

        // Foreign Key to the requesting resident (assuming ApplicationUser is your Identity user)
        [Required]
        public string RequestingResidentId { get; set; } = string.Empty;

        [ForeignKey("RequestingResidentId")]
        public virtual ApplicationUser? RequestingResident { get; set; } // Navigation property

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}