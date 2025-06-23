using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace TriathlonTracker.SeleniumTests
{
    /// <summary>
    /// Selenium tests for triathlon pages.
    /// Note: These tests require the TriathlonTracker application to be running.
    /// Run the application first with: dotnet run --project TriathlonTracker
    /// </summary>
    public class TriathlonPageTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string BaseUrl = "http://localhost:5217"; // Updated to use HTTP port from launchSettings.json

        public TriathlonPageTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void TriathlonIndexPage_ShouldLoadSuccessfully()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon");

            // Assert
            Assert.Contains("TriathlonTracker", _driver.Title);
            
            // Should redirect to login page since we're not authenticated
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonCreatePage_ShouldRedirectToLoginWhenNotAuthenticated()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Create");

            // Assert
            // Should redirect to login page since we're not authenticated
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonEditPage_ShouldRedirectToLoginWhenNotAuthenticated()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Edit/1");

            // Assert
            // Should redirect to login page since we're not authenticated
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonDeletePage_ShouldRedirectToLoginWhenNotAuthenticated()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Delete/1");

            // Assert
            // Should redirect to login page since we're not authenticated
            Assert.Contains("login", _driver.Url.ToLower());
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
} 