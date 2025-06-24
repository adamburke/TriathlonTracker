using Xunit;
using TriathlonTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Tests
{
    public class ModelTests
    {
        [Fact]
        public void User_Properties_ShouldWorkCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = "test-id",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                CreatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("test-id", user.Id);
            Assert.Equal("testuser@example.com", user.UserName);
            Assert.Equal("testuser@example.com", user.Email);
            Assert.Equal("John", user.FirstName);
            Assert.Equal("Doe", user.LastName);
            Assert.Equal(DateTimeKind.Utc, user.CreatedAt.Kind);
        }

        [Fact]
        public void User_DefaultValues_ShouldBeSet()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(string.Empty, user.FirstName);
            Assert.Equal(string.Empty, user.LastName);
            Assert.Equal(DateTimeKind.Utc, user.CreatedAt.Kind);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnTrue_WhenRequestIdIsSet()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = "test-request-id"
            };

            // Act
            var showRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.True(showRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = null
            };

            // Act
            var showRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.False(showRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = ""
            };

            // Act
            var showRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.False(showRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnFalse_WhenRequestIdIsWhitespace()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = "   "
            };

            // Act
            var showRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.True(showRequestId);
        }

        [Fact]
        public void Triathlon_DefaultValues_ShouldBeSet()
        {
            // Arrange & Act
            var triathlon = new Triathlon();

            // Assert
            Assert.Equal(string.Empty, triathlon.RaceName);
            Assert.Equal(string.Empty, triathlon.Location);
            Assert.Equal("yards", triathlon.SwimUnit);
            Assert.Equal("miles", triathlon.BikeUnit);
            Assert.Equal("miles", triathlon.RunUnit);
            Assert.Equal(string.Empty, triathlon.UserId);
            Assert.Equal(DateTimeKind.Utc, triathlon.CreatedAt.Kind);
            Assert.Equal(DateTimeKind.Utc, triathlon.UpdatedAt.Kind);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleYardsToKmConversion()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 1760, // 1 mile in yards
                SwimUnit = "yards",
                BikeDistance = 0,
                BikeUnit = "km",
                RunDistance = 0,
                RunUnit = "km"
            };

            // Act
            var totalDistance = triathlon.TotalDistance;

            // Assert
            // 1760 yards = 1760 * 0.9144 / 1000 = 1.609 km
            var expectedDistance = (1760 * 0.9144) / 1000;
            Assert.Equal(expectedDistance, totalDistance, 3);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleMetersToKmConversion()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 1500, // 1.5km in meters
                SwimUnit = "meters",
                BikeDistance = 0,
                BikeUnit = "km",
                RunDistance = 0,
                RunUnit = "km"
            };

            // Act
            var totalDistance = triathlon.TotalDistance;

            // Assert
            // 1500 meters = 1.5 km
            Assert.Equal(1.5, totalDistance, 1);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleMilesToKmConversion()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 0,
                SwimUnit = "meters",
                BikeDistance = 26.2, // Marathon distance in miles
                BikeUnit = "miles",
                RunDistance = 0,
                RunUnit = "km"
            };

            // Act
            var totalDistance = triathlon.TotalDistance;

            // Assert
            // 26.2 miles = 26.2 * 1.60934 = 42.16 km
            var expectedDistance = 26.2 * 1.60934;
            Assert.Equal(expectedDistance, totalDistance, 2);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleAllUnitsTogether()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 1500, // meters
                SwimUnit = "meters",
                BikeDistance = 40, // miles
                BikeUnit = "miles",
                RunDistance = 10, // km
                RunUnit = "km"
            };

            // Act
            var totalDistance = triathlon.TotalDistance;

            // Assert
            // Swim: 1500m = 1.5km
            // Bike: 40 miles = 40 * 1.60934 = 64.37km
            // Run: 10km = 10km
            // Total: 1.5 + 64.37 + 10 = 75.87km
            var expectedDistance = 1.5 + (40 * 1.60934) + 10;
            Assert.Equal(expectedDistance, totalDistance, 2);
        }

        [Fact]
        public void Triathlon_CreatedAtAndUpdatedAt_ShouldBeSetToUtcNow()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            var triathlon = new Triathlon();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(triathlon.CreatedAt >= beforeCreation && triathlon.CreatedAt <= afterCreation);
            Assert.True(triathlon.UpdatedAt >= beforeCreation && triathlon.UpdatedAt <= afterCreation);
            Assert.Equal(DateTimeKind.Utc, triathlon.CreatedAt.Kind);
            Assert.Equal(DateTimeKind.Utc, triathlon.UpdatedAt.Kind);
        }

        [Fact]
        public void Triathlon_ValidationAttributes_ShouldBePresent()
        {
            // Arrange
            var triathlon = new Triathlon();
            var properties = typeof(Triathlon).GetProperties();

            // Act & Assert
            var raceNameProperty = properties.First(p => p.Name == "RaceName");
            var raceNameAttributes = raceNameProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(raceNameAttributes);

            var stringLengthAttributes = raceNameProperty.GetCustomAttributes(typeof(StringLengthAttribute), true);
            Assert.NotEmpty(stringLengthAttributes);

            var raceDateProperty = properties.First(p => p.Name == "RaceDate");
            var raceDateAttributes = raceDateProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(raceDateAttributes);

            var locationProperty = properties.First(p => p.Name == "Location");
            var locationAttributes = locationProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(locationAttributes);

            var swimDistanceProperty = properties.First(p => p.Name == "SwimDistance");
            var swimDistanceAttributes = swimDistanceProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(swimDistanceAttributes);

            var rangeAttributes = swimDistanceProperty.GetCustomAttributes(typeof(RangeAttribute), true);
            Assert.NotEmpty(rangeAttributes);

            var bikeDistanceProperty = properties.First(p => p.Name == "BikeDistance");
            var bikeDistanceAttributes = bikeDistanceProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(bikeDistanceAttributes);

            var runDistanceProperty = properties.First(p => p.Name == "RunDistance");
            var runDistanceAttributes = runDistanceProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            Assert.NotEmpty(runDistanceAttributes);
        }

        [Fact]
        public void Triathlon_SwimPace_ShouldBeZeroIfNoDistance()
        {
            var triathlon = new Triathlon { SwimDistance = 0, SwimTime = TimeSpan.FromMinutes(10) };
            Assert.Equal(0, triathlon.SwimPace);
        }

        [Fact]
        public void Triathlon_BikePace_ShouldBeZeroIfNoDistance()
        {
            var triathlon = new Triathlon { BikeDistance = 0, BikeTime = TimeSpan.FromMinutes(10) };
            Assert.Equal(0, triathlon.BikePace);
        }

        [Fact]
        public void Triathlon_RunPace_ShouldBeZeroIfNoDistance()
        {
            var triathlon = new Triathlon { RunDistance = 0, RunTime = TimeSpan.FromMinutes(10) };
            Assert.Equal(0, triathlon.RunPace);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleNegativeDistances()
        {
            var triathlon = new Triathlon { SwimDistance = -100, SwimUnit = "meters", BikeDistance = -10, BikeUnit = "km", RunDistance = -5, RunUnit = "km" };
            var total = triathlon.TotalDistance;
            Assert.True(total <= 0);
        }

        [Fact]
        public void Triathlon_TotalDistance_ShouldHandleAllUnitCombinations()
        {
            var triathlon = new Triathlon { SwimDistance = 1000, SwimUnit = "meters", BikeDistance = 10, BikeUnit = "km", RunDistance = 5, RunUnit = "km" };
            Assert.True(triathlon.TotalDistance > 0);
            triathlon = new Triathlon { SwimDistance = 1000, SwimUnit = "yards", BikeDistance = 10, BikeUnit = "miles", RunDistance = 5, RunUnit = "miles" };
            Assert.True(triathlon.TotalDistance > 0);
        }
    }
} 