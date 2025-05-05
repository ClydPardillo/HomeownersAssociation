using Microsoft.AspNetCore.Identity;
using System; // Required for DateTime
using System.ComponentModel.DataAnnotations; // Required for Display attribute if needed
using System.Collections.Generic; // Required for ICollection

namespace HomeownersAssociation.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    // This assumes you have a class inheriting from IdentityUser, often named ApplicationUser.
    // If your user class has a different name or location, adjust accordingly.
    public class ApplicationUser : IdentityUser
    {
        // Custom user properties added based on usage in controllers/views
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Lot Number")]
        public string LotNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Block Number")]
        public string BlockNumber { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public string? ProfilePictureUrl { get; set; }

        [Required]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Account Approved")]
        public bool IsApproved { get; set; } = false; // Default to not approved

        [Required]
        [Display(Name = "User Type")]
        public UserType UserType { get; set; } = UserType.Homeowner; // Default to Homeowner


        // Navigation properties to link back to the new models
        public virtual ICollection<VisitorPassRequest>? VisitorPassRequests { get; set; }
        public virtual ICollection<VehicleRegistration>? VehicleRegistrations { get; set; }
        public virtual ICollection<EmergencyContact>? EmergencyContacts { get; set; }
        // Add other navigation properties if needed, e.g., for Bills, Payments
        public virtual ICollection<Bill>? Bills { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
    }

    // Define the UserType enum
    public enum UserType
    {
        Homeowner,
        Staff,
        Admin
    }
}