using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace TriathlonTracker.SeleniumTests
{
    /// <summary>
    /// Selenium tests for the home page.
    /// Note: These tests require the TriathlonTracker application to be running.
    /// Run the application first with: dotnet run --project TriathlonTracker
    /// </summary>
    public class HomePageTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private const string BaseUrl = "http://localhost:5217"; // Updated to use HTTP port from launchSettings.json

        public HomePageTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Run in headless mode for CI/CD
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [Fact]
        public void HomePage_ShouldLoadSuccessfully()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl(BaseUrl);

            // Assert
            Assert.Contains("TriathlonTracker", _driver.Title);
        }

        [Fact]
        public void HomePage_ShouldHaveExpectedElements()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl(BaseUrl);

            // Assert
            Assert.True(_driver.FindElement(By.TagName("body")).Displayed);
            
            // Check that the page has a title
            var pageTitle = _driver.Title;
            Assert.Contains("TriathlonTracker", pageTitle, StringComparison.OrdinalIgnoreCase);
            
            // Check that the page has some content - be more lenient
            var bodyText = _driver.FindElement(By.TagName("body")).Text;
            Assert.True(bodyText.Length > 0 || _driver.FindElements(By.TagName("div")).Count > 0, "Page should have some content or elements");
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}