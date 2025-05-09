using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;
using HomeownersAssociation.Data;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PendingApprovalsCount = await _context.Users.CountAsync(u => !u.IsApproved && u.UserType == UserType.Homeowner);
            ViewBag.ActiveUsersCount = await _context.Users.CountAsync(u => u.IsApproved && u.IsActive);
            ViewBag.OpenRequestsCount = await _context.ServiceRequests.CountAsync(sr => sr.Status != ServiceRequestStatus.Completed && sr.Status != ServiceRequestStatus.Cancelled);
            ViewBag.RecentPaymentsCount = await _context.Payments.CountAsync(p => p.PaymentDate >= DateTime.Today.AddDays(-7));
            return View();
        }

        public async Task<IActionResult> PendingApprovals()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => !u.IsApproved && u.UserType == UserType.Homeowner)
                .ToListAsync();

            return View(pendingUsers);
        }

        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Add to correct role based on user type
                if (user.UserType == UserType.Homeowner)
                {
                    await _userManager.AddToRoleAsync(user, "Homeowner");
                }

                TempData["SuccessMessage"] = $"User {user.Email} has been approved.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error approving user.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been rejected and removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error rejecting user.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        // Staff Management
        public async Task<IActionResult> StaffMembers()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var staffMembers = await _userManager.Users
                .Where(u => u.UserType == UserType.Staff)
                .ToListAsync();

            return View(staffMembers);
        }

        public IActionResult CreateStaff()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            return View(new CreateStaffViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    LotNumber = "N/A",
                    BlockNumber = "N/A",
                    RegistrationDate = DateTime.Now,
                    IsApproved = true, // Staff is auto-approved
                    UserType = UserType.Staff,
                    Role = "Staff" // Explicitly set the Role property
                };

                // Handle profile picture upload
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add staff to Staff role
                    await _userManager.AddToRoleAsync(user, "Staff");

                    TempData["SuccessMessage"] = "Staff member created successfully!";
                    return RedirectToAction(nameof(StaffMembers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> EditStaff(string id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var staff = await _userManager.FindByIdAsync(id);
            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            var model = new EditStaffViewModel
            {
                Id = staff.Id,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Email = staff.Email ?? string.Empty,
                Address = staff.Address,
                ProfilePictureUrl = staff.ProfilePictureUrl,
                IsActive = staff.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(EditStaffViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var staff = await _userManager.FindByIdAsync(model.Id);
            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            staff.FirstName = model.FirstName;
            staff.LastName = model.LastName;
            staff.Address = model.Address;
            staff.IsActive = model.IsActive;
            staff.Role = "Staff"; // Ensure Role is set on edit

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(staff.ProfilePictureUrl))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, staff.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }

                staff.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(staff);
            if (result.Succeeded)
            {
                // Handle password change if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(staff);
                    var passwordResult = await _userManager.ResetPasswordAsync(staff, token, model.NewPassword);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "Staff member updated successfully!";
                return RedirectToAction(nameof(StaffMembers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStaffStatus(string userId)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var staffMember = await _userManager.FindByIdAsync(userId);

            if (staffMember == null || staffMember.UserType != UserType.Staff)
            {
                TempData["ErrorMessage"] = "Staff member not found.";
                return RedirectToAction(nameof(StaffMembers));
            }

            staffMember.IsActive = !staffMember.IsActive;
            var result = await _userManager.UpdateAsync(staffMember);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Staff member {staffMember.Email} status has been updated to {(staffMember.IsActive ? "Active" : "Inactive")}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating staff member status.";
            }

            return RedirectToAction(nameof(StaffMembers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var staff = await _userManager.FindByIdAsync(id);

            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(staff);

            if (result.Succeeded)
            {
                // Delete profile picture if exists
                if (!string.IsNullOrEmpty(staff.ProfilePictureUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, staff.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                TempData["SuccessMessage"] = $"Staff member {staff.Email} has been deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting staff member.";
            }

            return RedirectToAction(nameof(StaffMembers));
        }

        // API Endpoints for Dashboard Widgets
        [HttpGet("Admin/Dashboard/CommunityGrowth")]
        public async Task<JsonResult> GetCommunityGrowthData()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-12);
            // Fetch raw data first
            var userRegistrations = await _context.Users
                .Where(u => u.RegistrationDate >= twelveMonthsAgo)
                .Select(u => new { u.RegistrationDate })
                .ToListAsync();

            // Perform grouping and transformation in memory
            var registrationsByMonth = userRegistrations
                .GroupBy(u => new { Year = u.RegistrationDate.Year, Month = u.RegistrationDate.Month })
                .Select(g => new {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToList(); // Ensure it's a list for consistent processing below

            // Ensure all months in the last 12 are present, even if count is 0
            var labels = new List<string>();
            var data = new List<int>();
            for (int i = 11; i >= 0; i--) // Iterate from 11 months ago to current month
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var monthLabel = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                labels.Add(monthLabel.ToString("MMM yyyy"));
                data.Add(registrationsByMonth.FirstOrDefault(r => r.Label.Year == monthLabel.Year && r.Label.Month == monthLabel.Month)?.Count ?? 0);
            }

            return Json(new { labels, data });
        }

        [HttpGet("Admin/Dashboard/BillStatusDistribution")]
        public async Task<JsonResult> GetBillStatusDistributionData()
        {
            var billCounts = await _context.Bills
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();
            
            // Ensure all enum values are present, even if count is 0
            var statuses = Enum.GetNames(typeof(BillStatus));
            var labels = new List<string>();
            var data = new List<int>();

            foreach (var statusName in statuses)
            {
                labels.Add(statusName);
                data.Add(billCounts.FirstOrDefault(bc => bc.Status == statusName)?.Count ?? 0);
            }

            return Json(new { labels, data });
        }

        [HttpGet("Admin/Dashboard/RecentAnnouncements")]
        public async Task<JsonResult> GetRecentAnnouncementsSummary(int count = 3)
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.DatePosted)
                .Take(count)
                .Select(a => new 
                {
                    a.Title,
                    DatePosted = a.DatePosted.ToString("MMM dd, yyyy"),
                    Snippet = a.Content.Length > 100 ? a.Content.Substring(0, 100) + "..." : a.Content,
                    Url = Url.Action("Details", "Announcements", new { id = a.Id })
                })
                .ToListAsync();
            return Json(announcements);
        }

        [HttpGet("Admin/Dashboard/LatestActivity")]
        public async Task<JsonResult> GetLatestActivitySummary(int count = 3)
        {
            // For now, let's get users pending approval
            var pendingUsers = await _context.Users
                .Where(u => !u.IsApproved && u.UserType == UserType.Homeowner)
                .OrderByDescending(u => u.RegistrationDate)
                .Take(count)
                .Select(u => new 
                {
                    Activity = $"New user pending approval: {u.FullName} ({u.Email})",
                    Date = u.RegistrationDate.ToString("MMM dd, yyyy"),
                    Url = Url.Action("PendingApprovals", "Admin")
                })
                .ToListAsync();
            
            // In a real scenario, you might want to combine different types of activities here
            // For example, recent service requests, new documents, etc.

            return Json(pendingUsers);
        }

        [HttpGet("Admin/Dashboard/PaymentAmounts")]
        public async Task<JsonResult> GetPaymentAmountsData(int months = 6)
        {
            var startDate = DateTime.Now.AddMonths(-months + 1).Date;
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            // Fetch raw data first
            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startDate)
                .Select(p => new { p.PaymentDate, p.AmountPaid })
                .ToListAsync();

            // Perform grouping and summing in memory
            var paymentsByMonth = payments
                .GroupBy(p => new { Year = p.PaymentDate.Year, Month = p.PaymentDate.Month })
                .Select(g => new
                {
                    LabelDate = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalAmount = g.Sum(p => p.AmountPaid)
                })
                .OrderBy(x => x.LabelDate)
                .ToList();

            var resultLabels = new List<string>();
            var resultData = new List<decimal>();
            for (int i = 0; i < months; i++)
            {
                var currentDateLoop = DateTime.Now.AddMonths(-(months - 1) + i);
                var monthLabel = new DateTime(currentDateLoop.Year, currentDateLoop.Month, 1);
                resultLabels.Add(monthLabel.ToString("MMM yyyy"));
                resultData.Add(paymentsByMonth.FirstOrDefault(pbm => pbm.LabelDate.Year == monthLabel.Year && pbm.LabelDate.Month == monthLabel.Month)?.TotalAmount ?? 0m);
            }

            return Json(new { labels = resultLabels, data = resultData });
        }
    }
}