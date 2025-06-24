using Xunit;
using TriathlonTracker.Models;

namespace TriathlonTracker.Tests
{
    public class TriathlonTests
    {
        [Fact]
        public void TotalTime_ShouldCalculateCorrectly()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimTime = TimeSpan.FromMinutes(30),
                BikeTime = TimeSpan.FromHours(2),
                RunTime = TimeSpan.FromMinutes(45)
            };

            // Act
            var totalTime = triathlon.TotalTime;

            // Assert
            var expectedTime = TimeSpan.FromMinutes(30) + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(45);
            Assert.Equal(expectedTime, totalTime);
        }

        [Fact]
        public void SwimPace_ShouldCalculateCorrectly()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 1500, // meters
                SwimTime = TimeSpan.FromMinutes(25), // 25 minutes
                SwimUnit = "meters"
            };

            // Act
            var swimPace = triathlon.SwimPace;

            // Assert
            // 25 minutes / (1500/100) = 25 / 15 = 1.67 minutes per 100m
            var expectedPace = 25.0 / 15.0;
            Assert.Equal(expectedPace, swimPace, 2);
        }

        [Fact]
        public void BikePace_ShouldCalculateCorrectly()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                BikeDistance = 40, // km
                BikeTime = TimeSpan.FromHours(1.5), // 1.5 hours
                BikeUnit = "km"
            };

            // Act
            var bikePace = triathlon.BikePace;

            // Assert
            // 40 km / 1.5 hours = 26.67 km/hr
            var expectedPace = 40.0 / 1.5;
            Assert.Equal(expectedPace, bikePace, 2);
        }

        [Fact]
        public void RunPace_ShouldCalculateCorrectly()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                RunDistance = 10, // km
                RunTime = TimeSpan.FromMinutes(45), // 45 minutes
                RunUnit = "km"
            };

            // Act
            var runPace = triathlon.RunPace;

            // Assert
            // 45 minutes / 10 km = 4.5 minutes per km
            var expectedPace = 45.0 / 10.0;
            Assert.Equal(expectedPace, runPace, 2);
        }

        [Fact]
        public void TotalDistance_ShouldConvertUnitsCorrectly()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 1500,
                SwimUnit = "meters",
                BikeDistance = 40,
                BikeUnit = "miles",
                RunDistance = 10,
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
        public void SwimPace_ShouldReturnZero_WhenDistanceIsZero()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                SwimDistance = 0,
                SwimTime = TimeSpan.FromMinutes(30),
                SwimUnit = "meters"
            };

            // Act
            var swimPace = triathlon.SwimPace;

            // Assert
            Assert.Equal(0, swimPace);
        }

        [Fact]
        public void BikePace_ShouldReturnZero_WhenDistanceIsZero()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                BikeDistance = 0,
                BikeTime = TimeSpan.FromHours(2),
                BikeUnit = "km"
            };

            // Act
            var bikePace = triathlon.BikePace;

            // Assert
            Assert.Equal(0, bikePace);
        }

        [Fact]
        public void RunPace_ShouldReturnZero_WhenDistanceIsZero()
        {
            // Arrange
            var triathlon = new Triathlon
            {
                RunDistance = 0,
                RunTime = TimeSpan.FromMinutes(45),
                RunUnit = "km"
            };

            // Act
            var runPace = triathlon.RunPace;

            // Assert
            Assert.Equal(0, runPace);
        }
    }
}