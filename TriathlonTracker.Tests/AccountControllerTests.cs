using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using TriathlonTracker.Controllers;
using TriathlonTracker.Models;
using TriathlonTracker.ViewModels;
using System.Collections.Generic;

namespace TriathlonTracker.Tests
{
    public class AccountControllerTests
    {
        private Mock<UserManager<User>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private Mock<SignInManager<User>> GetSignInManagerMock(UserManager<User> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(userManager, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);
        }

        private AccountController GetController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            var controller = new AccountController(userManager, signInManager);
            
            // Set up HttpContext and Url
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Mock the Url property
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(true);
            controller.Url = urlHelper.Object;
            
            return controller;
        }

        [Fact]
        public void Login_Get_ShouldReturnView()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.Login(null);
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Register_Get_ShouldReturnView()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.Register(null);
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AccessDenied_Get_ShouldReturnView()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.AccessDenied();
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void GoogleLogin_ShouldReturnChallenge()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.GoogleLogin("/Home/Index");
            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public void GoogleLogin_NoReturnUrl_ShouldReturnChallenge()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.GoogleLogin(null);
            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public void Login_Get_WithReturnUrl_ShouldSetViewData()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.Login("/Home/Index");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Home/Index", viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public void Register_Get_WithReturnUrl_ShouldSetViewData()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var controller = GetController(userManager.Object, signInManager.Object);

            var result = controller.Register("/Home/Index");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Home/Index", viewResult.ViewData["ReturnUrl"]);
        }
    }
} 