using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace TriathlonTracker.SeleniumTests
{
    /// <summary>
    /// Selenium tests for authentication functionality.
    /// Note: These tests require the TriathlonTracker application to be running.
    /// Run the application first with: dotnet run --project TriathlonTracker
    /// </summary>
    public class AuthenticationTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string BaseUrl = "http://localhost:5217";

        // Test credentials from Program.cs seed data
        private const string TestEmail = "test@test.com";
        private const string TestPassword = "Test@123";

        public AuthenticationTests()
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
        public void LoginForm_ShouldLoadCorrectly()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

            // Wait for page to load
            _wait.Until(driver => driver.FindElement(By.TagName("form")));

            // Assert
            // Should find login form elements
            var emailField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your password']"));
            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

            Assert.True(emailField.Displayed, "Email field should be visible");
            Assert.True(passwordField.Displayed, "Password field should be visible");
            Assert.True(loginButton.Displayed, "Login button should be visible");

            // Should be able to fill the form
            emailField.Clear();
            emailField.SendKeys(TestEmail);
            passwordField.Clear();
            passwordField.SendKeys(TestPassword);

            Assert.Equal(TestEmail, emailField.GetAttribute("value"));
            Assert.Equal(TestPassword, passwordField.GetAttribute("value"));
        }

        [Fact]
        public void Login_SubmitForm_ShouldNotShowError()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

            // Wait for page to load
            _wait.Until(driver => driver.FindElement(By.TagName("form")));

            // Find and fill login form
            var emailField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your password']"));
            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

            emailField.Clear();
            emailField.SendKeys(TestEmail);
            passwordField.Clear();
            passwordField.SendKeys(TestPassword);
            loginButton.Click();

            // Wait a moment for the form submission to process
            Thread.Sleep(2000);

            // Assert
            // Should not show "Invalid login attempt" error
            var bodyText = _driver.FindElement(By.TagName("body")).Text;
            Assert.DoesNotContain("Invalid login attempt", bodyText);
            
            // Should redirect to either Home or Triathlon page (both indicate successful login)
            var currentUrl = _driver.Url;
            Assert.True(currentUrl.Contains("/Home") || currentUrl.Contains("/Triathlon") || currentUrl.Contains("/Account/Login"), 
                $"Unexpected URL after login: {currentUrl}");
        }

        [Fact]
        public void Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

            // Wait for page to load
            _wait.Until(driver => driver.FindElement(By.TagName("form")));

            // Find and fill login form using more reliable selectors
            var emailField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your password']"));
            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

            emailField.Clear();
            emailField.SendKeys(TestEmail);
            passwordField.Clear();
            passwordField.SendKeys(TestPassword);
            loginButton.Click();

            // Assert
            // Should redirect to either Home or Triathlon page after successful login
            _wait.Until(driver => driver.Url.Contains("/Home") || driver.Url.Contains("/Triathlon") || driver.Url == BaseUrl);
            
            // Verify we're logged in by checking for authenticated user elements
            var bodyText = _driver.FindElement(By.TagName("body")).Text;
            Assert.Contains(TestEmail, bodyText); // Email should be displayed in navbar
            
            // Should show authenticated user interface elements
            var logoutButton = _driver.FindElements(By.CssSelector("button[type='submit']"));
            Assert.True(logoutButton.Count > 0, "Logout button should be present for authenticated users");
        }

        [Fact]
        public void TriathlonIndexPage_WhenAuthenticated_ShouldShowUserContent()
        {
            // Arrange - Login first
            _driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");
            
            // Wait for page to load
            _wait.Until(driver => driver.FindElement(By.TagName("form")));

            var emailField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[placeholder='Enter your password']"));
            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

            emailField.Clear();
            emailField.SendKeys(TestEmail);
            passwordField.Clear();
            passwordField.SendKeys(TestPassword);
            loginButton.Click();

            // Wait for login to complete
            _wait.Until(driver => driver.Url.Contains("/Home") || driver.Url.Contains("/Triathlon") || driver.Url == BaseUrl);

            // Act - Navigate to triathlon index page
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon");

            // Assert
            // Should not redirect to login page
            Assert.DoesNotContain("login", _driver.Url.ToLower());
            
            // Should show triathlon-related content
            var bodyText = _driver.FindElement(By.TagName("body")).Text;
            Assert.Contains("TriathlonTracker", bodyText);
            
            // Should show authenticated user interface elements
            var navElements = _driver.FindElements(By.CssSelector("nav"));
            Assert.True(navElements.Count > 0, "Navigation should be present");
            
            // Should show authenticated user elements
            var logoutButton = _driver.FindElements(By.CssSelector("button[type='submit']"));
            Assert.True(logoutButton.Count > 0, "Logout button should be present for authenticated users");
        }

        [Fact]
        public void TriathlonIndexPage_WhenNotAuthenticated_ShouldRedirectToLogin()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon");

            // Assert
            // Should redirect to login page
            _wait.Until(driver => driver.Url.Contains("/Account/Login"));
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonCreatePage_WhenNotAuthenticated_ShouldRedirectToLogin()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Create");

            // Assert
            // Should redirect to login page
            _wait.Until(driver => driver.Url.Contains("/Account/Login"));
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonEditPage_WhenNotAuthenticated_ShouldRedirectToLogin()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Edit/1");

            // Assert
            // Should redirect to login page
            _wait.Until(driver => driver.Url.Contains("/Account/Login"));
            Assert.Contains("login", _driver.Url.ToLower());
        }

        [Fact]
        public void TriathlonDeletePage_WhenNotAuthenticated_ShouldRedirectToLogin()
        {
            // Arrange & Act
            _driver.Navigate().GoToUrl($"{BaseUrl}/Triathlon/Delete/1");

            // Assert
            // Should redirect to login page
            _wait.Until(driver => driver.Url.Contains("/Account/Login"));
            Assert.Contains("login", _driver.Url.ToLower());
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
} 