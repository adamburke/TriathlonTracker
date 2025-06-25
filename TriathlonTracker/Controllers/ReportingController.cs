using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace TriathlonTracker.Controllers;

/// <summary>
/// API endpoints for reporting system integration. Secured with JWT Bearer (OAuth2).
/// </summary>
[ApiController]
[Route("api/reporting/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")] // JWT Bearer auth
public class RacesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor for RacesController.
    /// </summary>
    /// <param name="context">Application database context</param>
    /// <param name="env">Web host environment</param>
    /// <param name="httpContextAccessor">HTTP context accessor</param>
    public RacesController(ApplicationDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Retrieves all race data for all users, with pagination and filtering. Secured with JWT Bearer (OAuth2).
    /// </summary>
    /// <remarks>
    /// Returns a paged list of triathlon race data for all users. Supports filtering by race name, user ID, and race date range.
    /// Requires a valid Bearer token in the Authorization header.
    /// </remarks>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Page size (default: 100, max: 1000)</param>
    /// <param name="raceName">Optional: filter by race name (contains, case-insensitive)</param>
    /// <param name="userId">Optional: filter by user ID</param>
    /// <param name="fromDate">Optional: filter by race date from (inclusive)</param>
    /// <param name="toDate">Optional: filter by race date to (inclusive)</param>
    /// <response code="200">Returns paged race data</response>
    /// <response code="401">Unauthorized - missing or invalid Bearer token</response>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 100,
        string? raceName = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        // Return sample data ONLY if running in Development and request is from localhost
        var isDev = _env.IsDevelopment();
        var isLocalhost = _httpContextAccessor.HttpContext?.Request.Host.Host == "localhost" ||
                          _httpContextAccessor.HttpContext?.Request.Host.Host == "127.0.0.1";
        if (isDev && isLocalhost)
        {
            var sample = new[]
            {
                new {
                    Id = 1,
                    RaceName = "Sample Ironman",
                    RaceDate = new DateTime(2023, 10, 1),
                    Location = "Sample City",
                    SwimDistance = 3800,
                    SwimUnit = "meters",
                    SwimTime = "01:10:00",
                    BikeDistance = 180,
                    BikeUnit = "km",
                    BikeTime = "05:00:00",
                    RunDistance = 42.2,
                    RunUnit = "km",
                    RunTime = "03:45:00",
                    TotalDistance = 226,
                    TotalTime = "09:55:00",
                    UserId = "sample-user-id",
                    User = new {
                        Id = "sample-user-id",
                        Email = "athlete@example.com",
                        FirstName = "Jane",
                        LastName = "Doe"
                    },
                    CreatedAt = new DateTime(2023, 10, 1, 12, 0, 0),
                    UpdatedAt = new DateTime(2023, 10, 1, 12, 0, 0)
                }
            };
            return Ok(new {
                page = 1,
                pageSize = 1,
                totalCount = 1,
                totalPages = 1,
                data = sample
            });
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 100;
        if (pageSize > 1000) pageSize = 1000;

        var query = _context.Triathlons
            .AsNoTracking()
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(raceName))
            query = query.Where(t => t.RaceName.ToLower().Contains(raceName.ToLower()));
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(t => t.UserId == userId);
        if (fromDate.HasValue)
            query = query.Where(t => t.RaceDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.RaceDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var races = await query
            .OrderByDescending(t => t.RaceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new {
                t.Id,
                t.RaceName,
                t.RaceDate,
                t.Location,
                t.SwimDistance,
                t.SwimUnit,
                t.SwimTime,
                t.BikeDistance,
                t.BikeUnit,
                t.BikeTime,
                t.RunDistance,
                t.RunUnit,
                t.RunTime,
                t.TotalDistance,
                t.TotalTime,
                t.UserId,
                User = t.User == null ? null : new {
                    t.User.Id,
                    t.User.Email,
                    t.User.FirstName,
                    t.User.LastName
                },
                t.CreatedAt,
                t.UpdatedAt
            })
            .ToListAsync();

        return Ok(new {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            data = races
        });
    }
} 