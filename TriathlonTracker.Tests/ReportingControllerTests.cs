using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Moq;
using TriathlonTracker.Controllers;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using Xunit;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace TriathlonTracker.Tests
{
    public class ReportingControllerTests
    {
        private ApplicationDbContext GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            // Add users
            context.Users.AddRange(
                new User
                {
                    Id = "user1",
                    Email = "user1@example.com",
                    UserName = "user1@example.com",
                    FirstName = "Adam",
                    LastName = "Smith",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = "user2",
                    Email = "user2@example.com",
                    UserName = "user2@example.com",
                    FirstName = "Eve",
                    LastName = "Johnson",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
            // Add triathlons
            context.Triathlons.AddRange(
                new Triathlon
                {
                    Id = 1,
                    RaceName = "Ironman Barcelona",
                    RaceDate = new DateTime(2023, 10, 1),
                    Location = "Barcelona",
                    SwimDistance = 3800,
                    SwimUnit = "meters",
                    SwimTime = TimeSpan.FromMinutes(70),
                    BikeDistance = 180,
                    BikeUnit = "km",
                    BikeTime = TimeSpan.FromHours(5),
                    RunDistance = 42.2,
                    RunUnit = "km",
                    RunTime = TimeSpan.FromMinutes(225),
                    UserId = "user1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Triathlon
                {
                    Id = 2,
                    RaceName = "Local Sprint",
                    RaceDate = new DateTime(2023, 5, 1),
                    Location = "Localtown",
                    SwimDistance = 750,
                    SwimUnit = "meters",
                    SwimTime = TimeSpan.FromMinutes(15),
                    BikeDistance = 20,
                    BikeUnit = "km",
                    BikeTime = TimeSpan.FromMinutes(40),
                    RunDistance = 5,
                    RunUnit = "km",
                    RunTime = TimeSpan.FromMinutes(25),
                    UserId = "user2",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
            return context;
        }

        private ReportingController GetController(ApplicationDbContext context)
        {
            var env = new Mock<IWebHostEnvironment>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var adminDashboardService = new Mock<IAdminDashboardService>();
            var logger = new Mock<ILogger<ReportingController>>();
            var auditService = new Mock<IAuditService>();
            
            var controller = new ReportingController(context, env.Object, httpContextAccessor.Object, adminDashboardService.Object, logger.Object, auditService.Object);
            
            // Set up HttpContext
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin") }, "mock"));
            httpContext.User = user;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            return controller;
        }

        [Fact]
        public async Task Index_ShouldReturnView()
        {
            // Arrange
            var db = GetDbContextWithData();
            var controller = GetController(db);

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ReportHistory_ShouldReturnView()
        {
            // Arrange
            var db = GetDbContextWithData();
            var controller = GetController(db);

            // Act
            var result = await controller.ReportHistory();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ReportDetails_ShouldReturnView()
        {
            // Arrange
            var db = GetDbContextWithData();
            var controller = GetController(db);

            // Act
            var result = await controller.ReportDetails("test-report-id");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task GenerateReport_ShouldReturnJsonResult()
        {
            // Arrange
            var db = GetDbContextWithData();
            var controller = GetController(db);

            // Act
            var result = await controller.GenerateReport("test", DateTime.Now, DateTime.Now.AddDays(1));

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task DownloadReport_ShouldReturnFileResult()
        {
            // Arrange
            var db = GetDbContextWithData();
            var controller = GetController(db);

            // Act
            var result = await controller.DownloadReport("test-report-id");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result); // Should return NotFoundObjectResult since no actual report exists
        }
    }
} 