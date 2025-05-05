using System;

namespace HomeownersAssociation.Models
{
    public class FacilityReservation
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public string UserId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Rejected", "Cancelled"
        public DateTime? Cancelled { get; set; }

        // Navigation properties
        public virtual Facility Facility { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
} 