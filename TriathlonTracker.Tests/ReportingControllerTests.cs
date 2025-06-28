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

        [Fact]
        public async Task ReturnsSampleData_WhenDevAndLocalhost()
        {
            // Arrange
            var db = GetDbContextWithData();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Development");
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);
            var controller = new RacesController(db, env.Object, accessor.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
            var root = doc.RootElement;
            Assert.Equal(1, root.GetProperty("page").GetInt32());
            Assert.Equal(1, root.GetProperty("pageSize").GetInt32());
            Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
            Assert.Equal("Sample Ironman", root.GetProperty("data")[0].GetProperty("RaceName").GetString());
        }

        [Fact]
        public async Task ReturnsPagedRealData_WhenNotDevOrNotLocalhost()
        {
            // Arrange
            var db = GetDbContextWithData();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);
            var controller = new RacesController(db, env.Object, accessor.Object);

            // Act
            var result = await controller.GetAll(page: 1, pageSize: 1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
            var root = doc.RootElement;
            Assert.Equal(1, root.GetProperty("page").GetInt32());
            Assert.Equal(1, root.GetProperty("pageSize").GetInt32());
            Assert.Equal(2, root.GetProperty("totalCount").GetInt32());
            Assert.Equal("Ironman Barcelona", root.GetProperty("data")[0].GetProperty("RaceName").GetString());
        }

        [Fact]
        public async Task FiltersByRaceName()
        {
            // Arrange
            var db = GetDbContextWithData();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);
            var controller = new RacesController(db, env.Object, accessor.Object);

            // Act
            var result = await controller.GetAll(raceName: "Sprint");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
            var root = doc.RootElement;
            Assert.Single(root.GetProperty("data").EnumerateArray());
            Assert.Equal("Local Sprint", root.GetProperty("data")[0].GetProperty("RaceName").GetString());
        }

        [Fact]
        public async Task FiltersByUserId()
        {
            // Arrange
            var db = GetDbContextWithData();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);
            var controller = new RacesController(db, env.Object, accessor.Object);

            // Act
            var result = await controller.GetAll(userId: "user2");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
            var root = doc.RootElement;
            Assert.Single(root.GetProperty("data").EnumerateArray());
            Assert.Equal("Local Sprint", root.GetProperty("data")[0].GetProperty("RaceName").GetString());
        }

        [Fact]
        public async Task FiltersByDateRange()
        {
            // Arrange
            var db = GetDbContextWithData();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);
            var controller = new RacesController(db, env.Object, accessor.Object);

            // Act
            var result = await controller.GetAll(fromDate: new DateTime(2023, 6, 1));

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
            var root = doc.RootElement;
            Assert.Single(root.GetProperty("data").EnumerateArray());
            Assert.Equal("Ironman Barcelona", root.GetProperty("data")[0].GetProperty("RaceName").GetString());
        }
    }
} 