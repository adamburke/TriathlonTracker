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
    public class TriathlonController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TriathlonController(ApplicationDbContext context, UserManager<User> userManager, ILogger<TriathlonController> logger, IAuditService auditService)
            : base(auditService, logger, userManager)
        {
            _context = context;
        }

        // GET: Triathlon
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing triathlon index", userId);

                var triathlons = await _context.Triathlons
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.RaceDate)
                    .ToListAsync();

                _logger.LogInformation("User {UserId} viewed {Count} triathlons", userId, triathlons.Count);
                await AuditAsync("ViewTriathlons", "Triathlon", null, $"Viewed {triathlons.Count} triathlons", userId, "Information");

                return View(triathlons);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error retrieving triathlons for user {UserId}", userId);
                await AuditAsync("ViewTriathlonsError", "Triathlon", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
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
                var userId = GetCurrentUserId();
                var triathlon = await _context.Triathlons
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await AuditAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, "Warning");
                    return NotFound();
                }

                _logger.LogInformation("User {UserId} viewed triathlon {TriathlonId}", userId, id);
                await AuditAsync("ViewTriathlon", "Triathlon", id.ToString(), $"Viewed triathlon: {triathlon.RaceName}", userId, "Information");

                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error retrieving triathlon {TriathlonId} for user {UserId}", id, userId);
                await AuditAsync("ViewTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        // GET: Triathlon/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
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
                    var userId = GetCurrentUserId();
                    if (string.IsNullOrWhiteSpace(userId) || userId == "Unknown")
                    {
                        _logger.LogWarning("User not authenticated for triathlon creation");
                        return RedirectToAction("Login", "Account");
                    }

                    triathlon.UserId = userId;
                    triathlon.CreatedAt = DateTime.UtcNow;
                    triathlon.UpdatedAt = DateTime.UtcNow;

                    _context.Add(triathlon);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} created triathlon {TriathlonId}", userId, triathlon.Id);
                    await AuditAsync("CreateTriathlon", "Triathlon", triathlon.Id.ToString(), $"Created triathlon: {triathlon.RaceName}", userId, "Information");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    var userId = GetCurrentUserId();
                    _logger.LogError(ex, "Error creating triathlon for user {UserId}", userId);
                    await AuditAsync("CreateTriathlonError", "Triathlon", null, $"Error: {ex.Message}", userId, "Error");
                    return ErrorView();
                }
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
                var userId = GetCurrentUserId();
                var triathlon = await _context.Triathlons.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await AuditAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, "Warning");
                    return NotFound();
                }
                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error retrieving triathlon {TriathlonId} for edit for user {UserId}", id, userId);
                await AuditAsync("EditTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        // POST: Triathlon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RaceName,RaceDate,Location,SwimDistance,SwimTime,SwimUnit,BikeDistance,BikeTime,BikeUnit,RunDistance,RunTime,RunUnit,CreatedAt,UpdatedAt")] Triathlon triathlon)
        {
            if (id != triathlon.Id)
            {
                _logger.LogWarning("Edit POST ID mismatch: {Id} != {TriathlonId}", id, triathlon.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var existing = await _context.Triathlons.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                    if (existing == null)
                    {
                        _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                        await AuditAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, "Warning");
                        return NotFound();
                    }

                    // Update properties of the tracked entity
                    existing.RaceName = triathlon.RaceName;
                    existing.RaceDate = triathlon.RaceDate.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(triathlon.RaceDate, DateTimeKind.Utc) : triathlon.RaceDate;
                    existing.Location = triathlon.Location;
                    existing.SwimDistance = triathlon.SwimDistance;
                    existing.SwimUnit = triathlon.SwimUnit;
                    existing.SwimTime = triathlon.SwimTime;
                    existing.BikeDistance = triathlon.BikeDistance;
                    existing.BikeUnit = triathlon.BikeUnit;
                    existing.BikeTime = triathlon.BikeTime;
                    existing.RunDistance = triathlon.RunDistance;
                    existing.RunUnit = triathlon.RunUnit;
                    existing.RunTime = triathlon.RunTime;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} edited triathlon {TriathlonId}", userId, id);
                    await AuditAsync("EditTriathlon", "Triathlon", id.ToString(), $"Edited triathlon: {triathlon.RaceName}", userId, "Information");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var userId = GetCurrentUserId();
                    _logger.LogError(ex, "Concurrency error editing triathlon {TriathlonId} for user {UserId}", id, userId);
                    await AuditAsync("EditTriathlonConcurrencyError", "Triathlon", id.ToString(), $"Concurrency error: {ex.Message}", userId, "Error");
                    return ErrorView();
                }
                catch (Exception ex)
                {
                    var userId = GetCurrentUserId();
                    _logger.LogError(ex, "Error editing triathlon {TriathlonId} for user {UserId}", id, userId);
                    await AuditAsync("EditTriathlonError", "Triathlon", id.ToString(), $"Error: {ex.Message}", userId, "Error");
                    return ErrorView();
                }
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
                var userId = GetCurrentUserId();
                var triathlon = await _context.Triathlons.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await AuditAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, "Warning");
                    return NotFound();
                }
                return View(triathlon);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error retrieving triathlon {TriathlonId} for delete for user {UserId}", id, userId);
                await AuditAsync("DeleteTriathlonError", "Triathlon", id?.ToString(), $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        // POST: Triathlon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var triathlon = await _context.Triathlons.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                if (triathlon == null)
                {
                    _logger.LogWarning("Triathlon {TriathlonId} not found or access denied for user {UserId}", id, userId);
                    await AuditAsync("AccessDenied", "Triathlon", id.ToString(), "Triathlon not found or access denied", userId, "Warning");
                    return NotFound();
                }
                _context.Triathlons.Remove(triathlon);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} deleted triathlon {TriathlonId}", userId, id);
                await AuditAsync("DeleteTriathlon", "Triathlon", id.ToString(), $"Deleted triathlon: {triathlon.RaceName}", userId, "Information");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error deleting triathlon {TriathlonId} for user {UserId}", id, userId);
                await AuditAsync("DeleteTriathlonError", "Triathlon", id.ToString(), $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        // Add this private method for unit tests
        private bool TriathlonExists(int id)
        {
            var userId = GetCurrentUserId();
            return _context.Triathlons.Any(t => t.Id == id && t.UserId == userId);
        }
    }
} 