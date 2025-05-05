using System;
using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models
{
    public class CommunityEvent
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndDateTime { get; set; }

        public string? Location { get; set; }

        [Display(Name = "All Day Event")]
        public bool IsAllDay { get; set; } = false;

        // Optional: Add category like 'Meeting', 'Maintenance', 'Social'
        public string? Category { get; set; }
    }
}