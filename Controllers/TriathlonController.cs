using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Controllers
{
    [Authorize]
    public class TriathlonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TriathlonController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Triathlon
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var triathlons = await _context.Triathlons
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.RaceDate)
                .ToListAsync();
            return View(triathlons);
        }

        // GET: Triathlon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Triathlon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RaceName,RaceDate,Location,SwimDistance,SwimUnit,SwimTime,BikeDistance,BikeUnit,BikeTime,RunDistance,RunUnit,RunTime")] Triathlon triathlon)
        {
            // Set UserId before validation
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                ModelState.AddModelError("", "You must be logged in to create a race.");
                return View(triathlon);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Unable to determine user. Please log in again.");
                return View(triathlon);
            }

            triathlon.UserId = userId;

            // Validate that UserId is set
            if (string.IsNullOrEmpty(triathlon.UserId))
            {
                ModelState.AddModelError("UserId", "User ID is required.");
                return View(triathlon);
            }

            // Ensure RaceDate is UTC
            if (triathlon.RaceDate.Kind != DateTimeKind.Utc)
            {
                triathlon.RaceDate = DateTime.SpecifyKind(triathlon.RaceDate, DateTimeKind.Utc);
            }

            // Try to parse TimeSpan values from form data if they're zero
            if (triathlon.SwimTime == TimeSpan.Zero && !string.IsNullOrEmpty(Request.Form["SwimTime"]))
            {
                if (TimeSpan.TryParse(Request.Form["SwimTime"], out TimeSpan swimTime))
                {
                    triathlon.SwimTime = swimTime;
                }
            }
            
            if (triathlon.BikeTime == TimeSpan.Zero && !string.IsNullOrEmpty(Request.Form["BikeTime"]))
            {
                if (TimeSpan.TryParse(Request.Form["BikeTime"], out TimeSpan bikeTime))
                {
                    triathlon.BikeTime = bikeTime;
                }
            }
            
            if (triathlon.RunTime == TimeSpan.Zero && !string.IsNullOrEmpty(Request.Form["RunTime"]))
            {
                if (TimeSpan.TryParse(Request.Form["RunTime"], out TimeSpan runTime))
                {
                    triathlon.RunTime = runTime;
                }
            }

            // Validate TimeSpan values only when distances are provided
            if (triathlon.SwimDistance > 0 && triathlon.SwimTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("SwimTime", "Swim time is required when swim distance is provided.");
            }
            
            if (triathlon.BikeDistance > 0 && triathlon.BikeTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("BikeTime", "Bike time is required when bike distance is provided.");
            }
            
            if (triathlon.RunDistance > 0 && triathlon.RunTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("RunTime", "Run time is required when run distance is provided.");
            }

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                triathlon.CreatedAt = DateTime.UtcNow;
                triathlon.UpdatedAt = DateTime.UtcNow;
                _context.Add(triathlon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(triathlon);
        }

        // GET: Triathlon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User)!;
            var triathlon = await _context.Triathlons
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (triathlon == null)
            {
                return NotFound();
            }

            return View(triathlon);
        }

        // POST: Triathlon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RaceName,RaceDate,Location,SwimDistance,SwimUnit,SwimTime,BikeDistance,BikeUnit,BikeTime,RunDistance,RunUnit,RunTime")] Triathlon triathlon)
        {
            if (id != triathlon.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User)!;
            var existingTriathlon = await _context.Triathlons
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (existingTriathlon == null)
            {
                return NotFound();
            }

            // Validate TimeSpan values
            if (triathlon.SwimDistance > 0 && triathlon.SwimTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("SwimTime", "Swim time is required when swim distance is provided.");
            }
            
            if (triathlon.BikeDistance > 0 && triathlon.BikeTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("BikeTime", "Bike time is required when bike distance is provided.");
            }
            
            if (triathlon.RunDistance > 0 && triathlon.RunTime == TimeSpan.Zero)
            {
                ModelState.AddModelError("RunTime", "Run time is required when run distance is provided.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingTriathlon.RaceName = triathlon.RaceName;
                    // Ensure RaceDate is UTC
                    if (triathlon.RaceDate.Kind != DateTimeKind.Utc)
                    {
                        existingTriathlon.RaceDate = DateTime.SpecifyKind(triathlon.RaceDate, DateTimeKind.Utc);
                    }
                    else
                    {
                        existingTriathlon.RaceDate = triathlon.RaceDate;
                    }
                    existingTriathlon.Location = triathlon.Location;
                    existingTriathlon.SwimDistance = triathlon.SwimDistance;
                    existingTriathlon.SwimUnit = triathlon.SwimUnit;
                    existingTriathlon.SwimTime = triathlon.SwimTime;
                    existingTriathlon.BikeDistance = triathlon.BikeDistance;
                    existingTriathlon.BikeUnit = triathlon.BikeUnit;
                    existingTriathlon.BikeTime = triathlon.BikeTime;
                    existingTriathlon.RunDistance = triathlon.RunDistance;
                    existingTriathlon.RunUnit = triathlon.RunUnit;
                    existingTriathlon.RunTime = triathlon.RunTime;
                    existingTriathlon.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingTriathlon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TriathlonExists(triathlon.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(triathlon);
        }

        // GET: Triathlon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User)!;
            var triathlon = await _context.Triathlons
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (triathlon == null)
            {
                return NotFound();
            }

            return View(triathlon);
        }

        // POST: Triathlon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var triathlon = await _context.Triathlons
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (triathlon != null)
            {
                _context.Triathlons.Remove(triathlon);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TriathlonExists(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            return _context.Triathlons.Any(e => e.Id == id && e.UserId == userId);
        }
    }
} 