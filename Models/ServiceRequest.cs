using System;

namespace HomeownersAssociation.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; } // 1 = High, 2 = Medium, 3 = Low
        public string Status { get; set; } // "Pending", "InProgress", "Completed", "Cancelled"
        public DateTime? Cancelled { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public virtual ServiceCategory Category { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
} 