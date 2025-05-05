using System;
using System.Linq;
using HomeownersAssociation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeownersAssociation.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
            
            // Look for existing data
            if (context.Facilities.Any() && context.ServiceCategories.Any())
            {
                return; // DB has been seeded
            }

            // Seed Facilities
            var facilities = new[]
            {
                new Facility 
                { 
                    Name = "Main Function Hall", 
                    Description = "Large hall suitable for community gatherings, parties and events.",
                    Capacity = 150,
                    RatePerHour = 1500.00M,
                    IsActive = true,
                    MaintenanceSchedule = "Every Monday morning, 8am-12pm"
                },
                new Facility 
                { 
                    Name = "Basketball Court", 
                    Description = "Regulation-size basketball court with seating for spectators.",
                    Capacity = 50,
                    RatePerHour = 500.00M,
                    IsActive = true,
                    MaintenanceSchedule = "First Friday of every month"
                },
                new Facility 
                { 
                    Name = "Swimming Pool", 
                    Description = "Olympic-size swimming pool with separate kiddie pool area.",
                    Capacity = 75,
                    RatePerHour = 800.00M,
                    IsActive = true,
                    MaintenanceSchedule = "Thursdays, 6am-10am"
                },
                new Facility 
                { 
                    Name = "Tennis Court", 
                    Description = "Professional tennis court with lighting for evening play.",
                    Capacity = 20,
                    RatePerHour = 400.00M,
                    IsActive = true,
                    MaintenanceSchedule = "Every other Tuesday, 7am-11am"
                }
            };

            context.Facilities.AddRange(facilities);
            
            // Seed Service Categories
            var serviceCategories = new[]
            {
                new ServiceCategory 
                { 
                    Name = "Plumbing", 
                    Description = "Issues related to water pipes, fixtures, and drainage",
                    IsActive = true
                },
                new ServiceCategory 
                { 
                    Name = "Electrical", 
                    Description = "Issues related to electrical systems, wiring, and power",
                    IsActive = true
                },
                new ServiceCategory 
                { 
                    Name = "Landscaping", 
                    Description = "Issues related to common area gardening and landscaping",
                    IsActive = true
                },
                new ServiceCategory 
                { 
                    Name = "Security", 
                    Description = "Issues related to security systems, gates, and guards",
                    IsActive = true
                },
                new ServiceCategory 
                { 
                    Name = "Cleaning", 
                    Description = "Issues related to cleanliness of common areas",
                    IsActive = true
                },
                new ServiceCategory 
                { 
                    Name = "General Maintenance", 
                    Description = "General repairs and maintenance issues",
                    IsActive = true
                }
            };

            context.ServiceCategories.AddRange(serviceCategories);
            
            context.SaveChanges();
        }
    }
} 