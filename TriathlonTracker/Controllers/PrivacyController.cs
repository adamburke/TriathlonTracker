using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace TriathlonTracker.Controllers
{
    public class PrivacyController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Consent(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Policy(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Cookie(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Contact(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }
    }
}