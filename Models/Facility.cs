using System;
using System.Collections.Generic;

namespace HomeownersAssociation.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal RatePerHour { get; set; }
        public bool IsActive { get; set; } = true;
        public string MaintenanceSchedule { get; set; }

        // Navigation property
        public virtual ICollection<FacilityReservation> Reservations { get; set; }
    }
} 