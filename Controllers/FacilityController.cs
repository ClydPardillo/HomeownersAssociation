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
using Microsoft.EntityFrameworkCore;

namespace HomeownersAssociation.Controllers
{
    [Authorize]
    public class FacilityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacilityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Facility
        public async Task<IActionResult> Index()
        {
            var facilities = await _context.Facilities
                .Where(f => f.IsActive)
                .ToListAsync();
            return View(facilities);
        }

        // GET: Facility/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facility == null)
            {
                return NotFound();
            }

            return View(facility);
        }

        // GET: Facility/Reserve/5
        public async Task<IActionResult> Reserve(int id)
        {
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facility == null)
            {
                return NotFound();
            }

            // Get existing reservations for availability checking
            var existingReservations = await _context.FacilityReservations
                .Where(r => r.FacilityId == id && 
                      (r.Status == "Approved" || r.Status == "Pending"))
                .ToListAsync();

            var viewModel = new FacilityReservationViewModel
            {
                Facility = facility,
                FacilityId = facility.Id,
                ExistingReservations = existingReservations,
                ReservationDate = DateTime.Today
            };

            return View(viewModel);
        }

        // POST: Facility/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(FacilityReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                // Check if there's an overlapping reservation
                var overlapping = await _context.FacilityReservations
                    .AnyAsync(r => r.FacilityId == model.FacilityId &&
                             r.ReservationDate == model.ReservationDate.Date &&
                             ((r.StartTime <= model.StartTime && model.StartTime <= r.EndTime) ||
                              (r.StartTime <= model.EndTime && model.EndTime <= r.EndTime) ||
                              (model.StartTime <= r.StartTime && r.StartTime <= model.EndTime)) &&
                             (r.Status == "Approved" || r.Status == "Pending"));

                if (overlapping)
                {
                    ModelState.AddModelError("", "There is already a reservation during this time.");
                    var facility = await _context.Facilities.FindAsync(model.FacilityId);
                    model.Facility = facility;
                    model.ExistingReservations = await _context.FacilityReservations
                        .Where(r => r.FacilityId == model.FacilityId && 
                               (r.Status == "Approved" || r.Status == "Pending"))
                        .ToListAsync();
                    return View(model);
                }

                var reservation = new FacilityReservation
                {
                    FacilityId = model.FacilityId,
                    UserId = user.Id,
                    ReservationDate = model.ReservationDate.Date,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Purpose = model.Purpose,
                    Status = "Pending"
                };

                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyReservations));
            }

            var facilityForError = await _context.Facilities.FindAsync(model.FacilityId);
            model.Facility = facilityForError;
            model.ExistingReservations = await _context.FacilityReservations
                .Where(r => r.FacilityId == model.FacilityId && 
                       (r.Status == "Approved" || r.Status == "Pending"))
                .ToListAsync();
            
            return View(model);
        }

        // GET: Facility/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var reservations = await _context.FacilityReservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Facility/CancelReservation/5
        public async Task<IActionResult> CancelReservation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var reservation = await _context.FacilityReservations
                .Include(r => r.Facility)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Facility/CancelReservation/5
        [HttpPost, ActionName("CancelReservation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservationConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var reservation = await _context.FacilityReservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.ReservationDate.Date < DateTime.Today)
            {
                return BadRequest("Cannot cancel past reservations");
            }

            reservation.Status = "Cancelled";
            reservation.Cancelled = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyReservations));
        }
    }
} 