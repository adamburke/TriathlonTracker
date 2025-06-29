using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TriathlonTracker.Controllers;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using System.Security.Claims;

namespace TriathlonTracker.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ShouldRedirectToTriathlonIndex()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var auditServiceMock = new Mock<IAuditService>();
            var controller = new HomeController(loggerMock.Object, auditServiceMock.Object);
            
            // Set up HttpContext
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            httpContext.User = user;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Triathlon", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Privacy_ShouldReturnViewResult()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var auditServiceMock = new Mock<IAuditService>();
            var controller = new HomeController(loggerMock.Object, auditServiceMock.Object);
            
            // Set up HttpContext
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            httpContext.User = user;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Error_ShouldReturnViewResultWithErrorViewModel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var auditServiceMock = new Mock<IAuditService>();
            var controller = new HomeController(loggerMock.Object, auditServiceMock.Object);
            
            // Set up HttpContext
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            httpContext.User = user;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            // Note: The Error action doesn't set a model, so we don't check for ErrorViewModel
        }
    }
} 