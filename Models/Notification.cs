using System;

namespace HomeownersAssociation.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "ServiceRequest", "Billing", "Facility", etc.
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }
} 