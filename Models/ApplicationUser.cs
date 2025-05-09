using Microsoft.AspNetCore.Identity;

namespace HomeownersAssociation.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public string BlockNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
        public string? ProfilePictureUrl { get; set; }
        public UserType UserType { get; set; } = UserType.Homeowner;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Custom properties
        public string UnitNumber { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserType
    {
        Homeowner,
        Admin,
        Staff
    }
}