using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HomeownersAssociation.Models;

namespace HomeownersAssociation.Models.ViewModels
{
    public class FacilityReservationViewModel
    {
        public Facility Facility { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        
        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }
        
        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Purpose of Reservation")]
        public string Purpose { get; set; }
        
        public IEnumerable<FacilityReservation> ExistingReservations { get; set; }
    }
} 