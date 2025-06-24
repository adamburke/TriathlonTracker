using Xunit;
using TriathlonTracker.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Tests
{
    public class ViewModelTests
    {
        [Fact]
        public void LoginViewModel_DefaultValues_ShouldBeSet()
        {
            // Arrange & Act
            var loginViewModel = new LoginViewModel();

            // Assert
            Assert.Equal(string.Empty, loginViewModel.Email);
            Assert.Equal(string.Empty, loginViewModel.Password);
            Assert.False(loginViewModel.RememberMe);
        }

        [Fact]
        public void LoginViewModel_Properties_ShouldWorkCorrectly()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                RememberMe = true
            };

            // Act & Assert
            Assert.Equal("test@example.com", loginViewModel.Email);
            Assert.Equal("TestPassword123!", loginViewModel.Password);
            Assert.True(loginViewModel.RememberMe);
        }

        [Fact]
        public void LoginViewModel_ValidationAttributes_ShouldBePresent()
        {
            // Arrange
            var loginViewModel = new LoginViewModel();
            var properties = typeof(LoginViewModel).GetProperties();

            // Act & Assert
            var emailProperty = properties.First(p => p.Name == "Email");
            var emailRequiredAttributes = emailProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(emailRequiredAttributes);

            var emailAddressAttributes = emailProperty.GetCustomAttributes(typeof(EmailAddressAttribute), true);
            Assert.NotEmpty(emailAddressAttributes);

            var passwordProperty = properties.First(p => p.Name == "Password");
            var passwordRequiredAttributes = passwordProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(passwordRequiredAttributes);

            var dataTypeAttributes = passwordProperty.GetCustomAttributes(typeof(DataTypeAttribute), true);
            Assert.NotEmpty(dataTypeAttributes);

            var rememberMeProperty = properties.First(p => p.Name == "RememberMe");
            var displayAttributes = rememberMeProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(displayAttributes);
        }

        [Fact]
        public void RegisterViewModel_DefaultValues_ShouldBeSet()
        {
            // Arrange & Act
            var registerViewModel = new RegisterViewModel();

            // Assert
            Assert.Equal(string.Empty, registerViewModel.FirstName);
            Assert.Equal(string.Empty, registerViewModel.LastName);
            Assert.Equal(string.Empty, registerViewModel.Email);
            Assert.Equal(string.Empty, registerViewModel.Password);
            Assert.Equal(string.Empty, registerViewModel.ConfirmPassword);
        }

        [Fact]
        public void RegisterViewModel_Properties_ShouldWorkCorrectly()
        {
            // Arrange
            var registerViewModel = new RegisterViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "SecurePassword123!",
                ConfirmPassword = "SecurePassword123!"
            };

            // Act & Assert
            Assert.Equal("John", registerViewModel.FirstName);
            Assert.Equal("Doe", registerViewModel.LastName);
            Assert.Equal("john.doe@example.com", registerViewModel.Email);
            Assert.Equal("SecurePassword123!", registerViewModel.Password);
            Assert.Equal("SecurePassword123!", registerViewModel.ConfirmPassword);
        }

        [Fact]
        public void RegisterViewModel_ValidationAttributes_ShouldBePresent()
        {
            // Arrange
            var registerViewModel = new RegisterViewModel();
            var properties = typeof(RegisterViewModel).GetProperties();

            // Act & Assert
            var firstNameProperty = properties.First(p => p.Name == "FirstName");
            var firstNameRequiredAttributes = firstNameProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(firstNameRequiredAttributes);

            var firstNameDisplayAttributes = firstNameProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(firstNameDisplayAttributes);

            var lastNameProperty = properties.First(p => p.Name == "LastName");
            var lastNameRequiredAttributes = lastNameProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(lastNameRequiredAttributes);

            var lastNameDisplayAttributes = lastNameProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(lastNameDisplayAttributes);

            var emailProperty = properties.First(p => p.Name == "Email");
            var emailRequiredAttributes = emailProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(emailRequiredAttributes);

            var emailAddressAttributes = emailProperty.GetCustomAttributes(typeof(EmailAddressAttribute), true);
            Assert.NotEmpty(emailAddressAttributes);

            var emailDisplayAttributes = emailProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(emailDisplayAttributes);

            var passwordProperty = properties.First(p => p.Name == "Password");
            var passwordRequiredAttributes = passwordProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(passwordRequiredAttributes);

            var passwordStringLengthAttributes = passwordProperty.GetCustomAttributes(typeof(StringLengthAttribute), true);
            Assert.NotEmpty(passwordStringLengthAttributes);

            var passwordDataTypeAttributes = passwordProperty.GetCustomAttributes(typeof(DataTypeAttribute), true);
            Assert.NotEmpty(passwordDataTypeAttributes);

            var passwordDisplayAttributes = passwordProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(passwordDisplayAttributes);

            var confirmPasswordProperty = properties.First(p => p.Name == "ConfirmPassword");
            var confirmPasswordDataTypeAttributes = confirmPasswordProperty.GetCustomAttributes(typeof(DataTypeAttribute), true);
            Assert.NotEmpty(confirmPasswordDataTypeAttributes);

            var confirmPasswordDisplayAttributes = confirmPasswordProperty.GetCustomAttributes(typeof(DisplayAttribute), true);
            Assert.NotEmpty(confirmPasswordDisplayAttributes);

            var compareAttributes = confirmPasswordProperty.GetCustomAttributes(typeof(CompareAttribute), true);
            Assert.NotEmpty(compareAttributes);
        }

        [Fact]
        public void RegisterViewModel_PasswordValidation_ShouldHaveCorrectStringLength()
        {
            // Arrange
            var passwordProperty = typeof(RegisterViewModel).GetProperty("Password");
            var stringLengthAttribute = passwordProperty!.GetCustomAttributes(typeof(StringLengthAttribute), true)
                .FirstOrDefault() as StringLengthAttribute;

            // Act & Assert
            Assert.NotNull(stringLengthAttribute);
            Assert.Equal(100, stringLengthAttribute.MaximumLength);
            Assert.Equal(8, stringLengthAttribute.MinimumLength);
        }

        [Fact]
        public void RegisterViewModel_ConfirmPassword_ShouldCompareWithPassword()
        {
            // Arrange
            var confirmPasswordProperty = typeof(RegisterViewModel).GetProperty("ConfirmPassword");
            var compareAttribute = confirmPasswordProperty!.GetCustomAttributes(typeof(CompareAttribute), true)
                .FirstOrDefault() as CompareAttribute;

            // Act & Assert
            Assert.NotNull(compareAttribute);
            Assert.Equal("Password", compareAttribute.OtherProperty);
            Assert.Equal("The password and confirmation password do not match.", compareAttribute.ErrorMessage);
        }

        [Fact]
        public void RegisterViewModel_DisplayNames_ShouldBeSetCorrectly()
        {
            // Arrange
            var properties = typeof(RegisterViewModel).GetProperties();

            // Act & Assert
            var firstNameProperty = properties.First(p => p.Name == "FirstName");
            var firstNameDisplayAttribute = firstNameProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("First Name", firstNameDisplayAttribute?.Name);

            var lastNameProperty = properties.First(p => p.Name == "LastName");
            var lastNameDisplayAttribute = lastNameProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("Last Name", lastNameDisplayAttribute?.Name);

            var emailProperty = properties.First(p => p.Name == "Email");
            var emailDisplayAttribute = emailProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("Email", emailDisplayAttribute?.Name);

            var passwordProperty = properties.First(p => p.Name == "Password");
            var passwordDisplayAttribute = passwordProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("Password", passwordDisplayAttribute?.Name);

            var confirmPasswordProperty = properties.First(p => p.Name == "ConfirmPassword");
            var confirmPasswordDisplayAttribute = confirmPasswordProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("Confirm password", confirmPasswordDisplayAttribute?.Name);
        }

        [Fact]
        public void LoginViewModel_DisplayNames_ShouldBeSetCorrectly()
        {
            // Arrange
            var properties = typeof(LoginViewModel).GetProperties();

            // Act & Assert
            var rememberMeProperty = properties.First(p => p.Name == "RememberMe");
            var rememberMeDisplayAttribute = rememberMeProperty.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;
            Assert.Equal("Remember me?", rememberMeDisplayAttribute?.Name);
        }
    }
} 