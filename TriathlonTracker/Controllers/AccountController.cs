using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TriathlonTracker.Models;
using TriathlonTracker.ViewModels;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuditService _auditService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt failed due to invalid model state for user {Email}", model.Email);
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: user not found for {Email}", model.Email);
                await _auditService.LogAsync("LoginFailed", "User", null, "User not found", null, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", model.Email);
                await _auditService.LogAsync("LoginSuccess", "User", user.Id, "User logged in", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out", model.Email);
                await _auditService.LogAsync("LoginLockedOut", "User", user.Id, "Account locked out", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                return View("Lockout");
            }
            _logger.LogWarning("Invalid login attempt for user {Email}", model.Email);
            await _auditService.LogAsync("LoginFailed", "User", user.Id, "Invalid login attempt", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User {Email} registered and logged in", model.Email);
                    await _auditService.LogAsync("Register", "User", user.Id, "User registered", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    return RedirectToLocal(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("Registration error for {Email}: {Error}", model.Email, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = _userManager.GetUserId(User);
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserId} logged out", userId);
            await _auditService.LogAsync("Logout", "User", userId, "User logged out", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                }
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 