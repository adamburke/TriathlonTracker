# TriathlonTracker Selenium Tests

This project contains Selenium WebDriver tests for the TriathlonTracker web application.

## Prerequisites

- .NET 8.0 SDK
- Chrome browser installed
- ChromeDriver (automatically managed by Selenium.WebDriver.ChromeDriver package)

## Test Structure

- **HomePageTests**: Tests for the home page functionality
- **TriathlonPageTests**: Tests for triathlon-related pages and authentication redirects

## Running the Tests

### Important Note
The Selenium tests are currently **skipped by default** because they require the TriathlonTracker web application to be running. This prevents test failures when the application isn't available.

### To Run Selenium Tests

1. **Start the web application** in a separate terminal:
   ```bash
   dotnet run --project TriathlonTracker
   ```

2. **Remove the Skip attributes** from the tests you want to run. Edit the test files and remove `(Skip = "...")` from the `[Fact]` attributes.

3. **Run the Selenium tests**:
   ```bash
   dotnet test TriathlonTracker.SeleniumTests
   ```

### Alternative: Run All Tests (Unit + Selenium)

If you want to run both unit tests and Selenium tests together:

1. Start the web application in one terminal:
   ```bash
   dotnet run --project TriathlonTracker
   ```

2. In another terminal, run all tests:
   ```bash
   dotnet test
   ```

## Test Configuration

- Tests run in headless Chrome mode for CI/CD compatibility
- SSL certificate errors are ignored for localhost testing
- Implicit wait timeout is set to 10 seconds
- Tests verify authentication redirects and page loading

## Troubleshooting

- **Connection refused errors**: Make sure the TriathlonTracker application is running
- **ChromeDriver issues**: The ChromeDriver is automatically managed by the NuGet package
- **SSL certificate errors**: Tests are configured to ignore localhost SSL issues

## CI/CD Integration

For continuous integration, you may want to:
1. Start the application as part of the test setup
2. Remove Skip attributes from tests
3. Ensure Chrome/ChromeDriver is available in the CI environment 