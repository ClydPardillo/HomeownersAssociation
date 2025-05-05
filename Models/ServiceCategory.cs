using System.Collections.Generic;

namespace HomeownersAssociation.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
    }
} 