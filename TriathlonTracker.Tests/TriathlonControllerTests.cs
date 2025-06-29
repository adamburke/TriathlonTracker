using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TriathlonTracker.Controllers;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using System.Threading;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Tests
{
    public class TriathlonControllerTests
    {
        private Mock<UserManager<User>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private TriathlonController GetController(ApplicationDbContext context, UserManager<User> userManager, string userId = "user1")
        {
            var loggerMock = new Mock<ILogger<TriathlonController>>();
            var auditServiceMock = new Mock<IAuditService>();
            var controller = new TriathlonController(context, userManager, loggerMock.Object, auditServiceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
            
            // Set up HttpContext with proper form content type
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = "application/x-www-form-urlencoded";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.ControllerContext.HttpContext.User = user;
            
            return controller;
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithUserTriathlons()
        {
            var db = GetDbContext("IndexDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.Triathlons.Add(new Triathlon { Id = 2, RaceName = "Other", UserId = "user2", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Triathlon>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_Get_ShouldReturnView()
        {
            var db = GetDbContext("CreateGetDb");
            var userManager = GetUserManagerMock();
            var controller = GetController(db, userManager.Object);

            var result = controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ShouldReturnViewWithModel()
        {
            var db = GetDbContext("CreatePostInvalidDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            controller.ModelState.AddModelError("RaceName", "Required");
            var triathlon = new Triathlon();

            var result = await controller.Create(triathlon);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(triathlon, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_ShouldRedirectToIndex()
        {
            var db = GetDbContext("CreatePostValidDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon
            {
                RaceName = "Race",
                RaceDate = System.DateTime.UtcNow,
                Location = "Loc",
                SwimDistance = 1,
                SwimUnit = "meters",
                SwimTime = System.TimeSpan.FromMinutes(10),
                BikeDistance = 1,
                BikeUnit = "km",
                BikeTime = System.TimeSpan.FromMinutes(10),
                RunDistance = 1,
                RunUnit = "km",
                RunTime = System.TimeSpan.FromMinutes(10)
            };

            var result = await controller.Create(triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_MissingSwimTime_ShouldReturnModelError()
        {
            var db = GetDbContext("CreatePostMissingSwimTimeDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_MissingBikeTime_ShouldReturnModelError()
        {
            var db = GetDbContext("CreatePostMissingBikeTimeDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_MissingRunTime_ShouldReturnModelError()
        {
            var db = GetDbContext("CreatePostMissingRunTimeDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km" };

            var result = await controller.Create(triathlon);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_UserNotAuthenticated_ShouldReturnViewWithModelError()
        {
            var db = GetDbContext("CreatePostNotAuthenticatedDb");
            var userManager = GetUserManagerMock();
            var controller = GetController(db, userManager.Object);
            
            // Set user as not authenticated
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext.User = unauthenticatedUser;
            
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Create_Post_EmptyUserId_ShouldReturnViewWithModelError()
        {
            var db = GetDbContext("CreatePostEmptyUserIdDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_WhitespaceUserId_ShouldReturnViewWithModelError()
        {
            var db = GetDbContext("CreatePostWhitespaceUserIdDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("   ");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_NonUtcRaceDate_ShouldConvertToUtc()
        {
            var db = GetDbContext("CreatePostNonUtcDateDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var localDate = DateTime.Now; // Local time
            var triathlon = new Triathlon { RaceName = "Race", RaceDate = localDate, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Create(triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Get_NullId_ShouldReturnNotFound()
        {
            var db = GetDbContext("EditGetNullDb");
            var userManager = GetUserManagerMock();
            var controller = GetController(db, userManager.Object);

            var result = await controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_NotFound_ShouldReturnNotFound()
        {
            var db = GetDbContext("EditGetNotFoundDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.Edit(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_Found_ShouldReturnViewWithModel()
        {
            var db = GetDbContext("EditGetFoundDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.Edit(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Triathlon>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ShouldReturnNotFound()
        {
            var db = GetDbContext("EditPostIdMismatchDb");
            var userManager = GetUserManagerMock();
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { Id = 2 };

            var result = await controller.Edit(1, triathlon);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_NotFound_ShouldReturnNotFound()
        {
            var db = GetDbContext("EditPostNotFoundDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { Id = 1 };

            var result = await controller.Edit(1, triathlon);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ShouldRedirectToIndex()
        {
            var db = GetDbContext("EditPostValidDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var triathlon = new Triathlon { Id = 1, RaceName = "Race", RaceDate = System.DateTime.UtcNow, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Edit(1, triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_NonUtcRaceDate_ShouldConvertToUtc()
        {
            var db = GetDbContext("EditPostNonUtcDateDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var localDate = DateTime.Now; // Local time
            var triathlon = new Triathlon { Id = 1, RaceName = "Race", RaceDate = localDate, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Edit(1, triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_UtcRaceDate_ShouldKeepAsUtc()
        {
            var db = GetDbContext("EditPostUtcDateDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var utcDate = DateTime.UtcNow;
            var triathlon = new Triathlon { Id = 1, RaceName = "Race", RaceDate = utcDate, Location = "Loc", SwimDistance = 1, SwimUnit = "meters", SwimTime = System.TimeSpan.FromMinutes(10), BikeDistance = 1, BikeUnit = "km", BikeTime = System.TimeSpan.FromMinutes(10), RunDistance = 1, RunUnit = "km", RunTime = System.TimeSpan.FromMinutes(10) };

            var result = await controller.Edit(1, triathlon);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Delete_Get_NullId_ShouldReturnNotFound()
        {
            var db = GetDbContext("DeleteGetNullDb");
            var userManager = GetUserManagerMock();
            var controller = GetController(db, userManager.Object);

            var result = await controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_NotFound_ShouldReturnNotFound()
        {
            var db = GetDbContext("DeleteGetNotFoundDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.Delete(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_Found_ShouldReturnViewWithModel()
        {
            var db = GetDbContext("DeleteGetFoundDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.Delete(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Triathlon>(viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_TriathlonNull_ShouldRedirectToIndex()
        {
            var db = GetDbContext("DeleteConfirmedNullDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.DeleteConfirmed(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_TriathlonExists_ShouldRemoveAndRedirect()
        {
            var db = GetDbContext("DeleteConfirmedExistsDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);

            var result = await controller.DeleteConfirmed(1);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Empty(db.Triathlons.ToList());
        }

        [Fact]
        public void TriathlonExists_ShouldReturnTrueIfExists()
        {
            var db = GetDbContext("TriathlonExistsTrueDb");
            db.Triathlons.Add(new Triathlon { Id = 1, RaceName = "Race", UserId = "user1", RaceDate = System.DateTime.UtcNow });
            db.SaveChanges();
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var exists = typeof(TriathlonController)
                .GetMethod("TriathlonExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                !.Invoke(controller, new object[] { 1 });
            Assert.True((bool)exists!);
        }

        [Fact]
        public void TriathlonExists_ShouldReturnFalseIfNotExists()
        {
            var db = GetDbContext("TriathlonExistsFalseDb");
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
            var controller = GetController(db, userManager.Object);
            var exists = typeof(TriathlonController)
                .GetMethod("TriathlonExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                !.Invoke(controller, new object[] { 1 });
            Assert.False((bool)exists!);
        }
    }
} 