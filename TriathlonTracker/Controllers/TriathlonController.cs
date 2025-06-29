using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers
{
    [Authorize(Roles = "User")]
    public class TriathlonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TriathlonController> _logger;
        private readonly IAuditService _auditService;

        public TriathlonController(ApplicationDbContext context, UserManager<User> userManager, ILogger<TriathlonController> logger, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _auditService = auditService;
        }

        // GET: Triathlon
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogDebug("User {UserId} accessing triathlon index", userId);

                var triathlons = await _context.Triathlons
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.RaceDate)
                    .ToListAsync();

                _logger.LogInformation("User {UserId} viewed {Count} triathlons", userId, triathlons.Count);
                await _auditService.LogAsync("ViewTriathlons", "Triathlon", null, $"Viewed {triathlons.Count} triathlons", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                return View(triathlons);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogError(ex, "Error retrieving triathlons for user {UserId}", userId);
                await _auditService.LogAsync("ViewTriathlonsError", "Triathlon", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        // GET: Triathlon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details requested with null ID");
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var triathlon = await _context.Triathlons
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await _auditService.LogAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    return NotFound();
                }

                _logger.LogInformation("User {UserId} viewed triathlon {TriathlonId}", userId, id);
                await _auditService.LogAsync("ViewTriathlon", "Triathlon", id.ToString(), $"Viewed triathlon: {triathlon.RaceName}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogError(ex, "Error retrieving triathlon {TriathlonId} for user {UserId}", id, userId);
                await _auditService.LogAsync("ViewTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        // GET: Triathlon/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogDebug("User {UserId} accessing triathlon create form", userId);
            return View();
        }

        // POST: Triathlon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RaceName,RaceDate,Location,SwimDistance,SwimTime,SwimUnit,BikeDistance,BikeTime,BikeUnit,RunDistance,RunTime,RunUnit")] Triathlon triathlon)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    if (userId == null)
                    {
                        _logger.LogWarning("User not authenticated for triathlon creation");
                        return RedirectToAction("Login", "Account");
                    }
                    
                    triathlon.UserId = userId;
                    triathlon.CreatedAt = DateTime.UtcNow;
                    triathlon.UpdatedAt = DateTime.UtcNow;

                    _context.Add(triathlon);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User {UserId} created triathlon {TriathlonId}: {RaceName} at {Location}", userId, triathlon.Id, triathlon.RaceName, triathlon.Location);
                    await _auditService.LogAsync("CreateTriathlon", "Triathlon", triathlon.Id.ToString(), $"Created triathlon: {triathlon.RaceName} at {triathlon.Location} on {triathlon.RaceDate:yyyy-MM-dd}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    var userId = _userManager.GetUserId(User);
                    _logger.LogError(ex, "Error creating triathlon for user {UserId}", userId);
                    await _auditService.LogAsync("CreateTriathlonError", "Triathlon", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                    ModelState.AddModelError("", "An error occurred while creating the triathlon.");
                }
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogWarning("Invalid model state for triathlon creation by user {UserId}", userId);
                await _auditService.LogAsync("CreateTriathlonValidationError", "Triathlon", null, "Invalid model state", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
            }

            return View(triathlon);
        }

        // GET: Triathlon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit requested with null ID");
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var triathlon = await _context.Triathlons
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await _auditService.LogAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied for edit", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    return NotFound();
                }

                _logger.LogDebug("User {UserId} accessing edit form for triathlon {TriathlonId}", userId, id);
                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogError(ex, "Error accessing edit form for triathlon {TriathlonId} by user {UserId}", id, userId);
                await _auditService.LogAsync("EditTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        // POST: Triathlon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RaceName,RaceDate,Location,SwimDistance,SwimTime,SwimUnit,BikeDistance,BikeTime,BikeUnit,RunDistance,RunTime,RunUnit")] Triathlon triathlon)
        {
            if (id != triathlon.Id)
            {
                _logger.LogWarning("ID mismatch in triathlon edit: {ExpectedId} vs {ActualId}", id, triathlon.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var existingTriathlon = await _context.Triathlons
                        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                    if (existingTriathlon == null)
                    {
                        _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                        await _auditService.LogAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied for edit", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                        return NotFound();
                    }

                    // Store original values for audit
                    var originalRaceName = existingTriathlon.RaceName;
                    var originalLocation = existingTriathlon.Location;
                    var originalDate = existingTriathlon.RaceDate;

                    // Update properties
                    existingTriathlon.RaceName = triathlon.RaceName;
                    existingTriathlon.RaceDate = triathlon.RaceDate;
                    existingTriathlon.Location = triathlon.Location;
                    existingTriathlon.SwimDistance = triathlon.SwimDistance;
                    existingTriathlon.SwimTime = triathlon.SwimTime;
                    existingTriathlon.SwimUnit = triathlon.SwimUnit;
                    existingTriathlon.BikeDistance = triathlon.BikeDistance;
                    existingTriathlon.BikeTime = triathlon.BikeTime;
                    existingTriathlon.BikeUnit = triathlon.BikeUnit;
                    existingTriathlon.RunDistance = triathlon.RunDistance;
                    existingTriathlon.RunTime = triathlon.RunTime;
                    existingTriathlon.RunUnit = triathlon.RunUnit;
                    existingTriathlon.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User {UserId} updated triathlon {TriathlonId}: {OriginalRaceName} -> {NewRaceName}", userId, id, originalRaceName, triathlon.RaceName);
                    await _auditService.LogAsync("UpdateTriathlon", "Triathlon", id.ToString(), $"Updated triathlon: {originalRaceName} -> {triathlon.RaceName}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var userId = _userManager.GetUserId(User);
                    _logger.LogError(ex, "Concurrency error updating triathlon {TriathlonId} by user {UserId}", id, userId);
                    await _auditService.LogAsync("UpdateTriathlonConcurrencyError", "Triathlon", id.ToString(), $"Concurrency error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                    
                    if (!TriathlonExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    var userId = _userManager.GetUserId(User);
                    _logger.LogError(ex, "Error updating triathlon {TriathlonId} by user {UserId}", id, userId);
                    await _auditService.LogAsync("UpdateTriathlonError", "Triathlon", id.ToString(), $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                    ModelState.AddModelError("", "An error occurred while updating the triathlon.");
                }
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogWarning("Invalid model state for triathlon update {TriathlonId} by user {UserId}", id, userId);
                await _auditService.LogAsync("UpdateTriathlonValidationError", "Triathlon", id.ToString(), "Invalid model state", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
            }

            return View(triathlon);
        }

        // GET: Triathlon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete requested with null ID");
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var triathlon = await _context.Triathlons
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await _auditService.LogAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied for delete", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    return NotFound();
                }

                _logger.LogDebug("User {UserId} accessing delete confirmation for triathlon {TriathlonId}", userId, id);
                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogError(ex, "Error accessing delete form for triathlon {TriathlonId} by user {UserId}", id, userId);
                await _auditService.LogAsync("DeleteTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        // POST: Triathlon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var triathlon = await _context.Triathlons
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await _auditService.LogAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied for delete", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    return NotFound();
                }

                var raceName = triathlon.RaceName;
                var location = triathlon.Location;
                var date = triathlon.RaceDate;

                _context.Triathlons.Remove(triathlon);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} deleted triathlon {TriathlonId}: {RaceName} at {Location} on {Date}", userId, id, raceName, location, date.ToString("yyyy-MM-dd"));
                await _auditService.LogAsync("DeleteTriathlon", "Triathlon", id.ToString(), $"Deleted triathlon: {raceName} at {location} on {date:yyyy-MM-dd}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogError(ex, "Error deleting triathlon {TriathlonId} by user {UserId}", id, userId);
                await _auditService.LogAsync("DeleteTriathlonError", "Triathlon", id.ToString(), $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        private bool TriathlonExists(int id)
        {
            return _context.Triathlons.Any(e => e.Id == id);
        }
    }
} 