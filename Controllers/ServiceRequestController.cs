using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HomeownersAssociation.Controllers
{
    [Authorize]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var serviceRequests = await _context.ServiceRequests
                .Include(s => s.Category)
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return View(serviceRequests);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.ServiceCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                serviceRequest.UserId = user.Id;
                serviceRequest.Status = "Pending";
                
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                
                // Create notification for the new service request
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = "Service Request Created",
                    Message = $"Your service request #{serviceRequest.Id} has been submitted and is pending review.",
                    Type = "ServiceRequest",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.ServiceCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", serviceRequest.CategoryId);
            return View(serviceRequest);
        }

        // GET: ServiceRequest/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            // Can only cancel requests that are still pending
            if (serviceRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending service requests can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            // Can only cancel requests that are still pending
            if (serviceRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending service requests can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            serviceRequest.Status = "Cancelled";
            serviceRequest.Cancelled = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Create notification for the cancelled service request
            var notification = new Notification
            {
                UserId = user.Id,
                Title = "Service Request Cancelled",
                Message = $"Your service request #{serviceRequest.Id} has been cancelled.",
                Type = "ServiceRequest",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
} 