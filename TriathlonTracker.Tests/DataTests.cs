using Xunit;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Tests
{
    public class DataTests
    {
        [Fact]
        public void ApplicationDbContext_Constructor_ShouldWork()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Act & Assert
            using var context = new ApplicationDbContext(options);
            Assert.NotNull(context);
        }

        [Fact]
        public void ApplicationDbContext_TriathlonsDbSet_ShouldBeAccessible()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act & Assert
            Assert.NotNull(context.Triathlons);
        }

        [Fact]
        public void ApplicationDbContext_UsersDbSet_ShouldBeAccessible()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase3")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act & Assert
            Assert.NotNull(context.Users);
        }

        [Fact]
        public void ApplicationDbContext_OnModelCreating_ShouldConfigureTriathlonEntity()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase4")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(Triathlon));

            // Assert
            Assert.NotNull(entityType);
            
            // Check that the entity has a primary key
            var primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Contains(primaryKey.Properties, p => p.Name == "Id");

            // Check that required properties are configured
            var raceNameProperty = entityType.FindProperty("RaceName");
            Assert.NotNull(raceNameProperty);
            Assert.False(raceNameProperty.IsNullable);

            var locationProperty = entityType.FindProperty("Location");
            Assert.NotNull(locationProperty);
            Assert.False(locationProperty.IsNullable);

            var swimDistanceProperty = entityType.FindProperty("SwimDistance");
            Assert.NotNull(swimDistanceProperty);
            Assert.False(swimDistanceProperty.IsNullable);

            var bikeDistanceProperty = entityType.FindProperty("BikeDistance");
            Assert.NotNull(bikeDistanceProperty);
            Assert.False(bikeDistanceProperty.IsNullable);

            var runDistanceProperty = entityType.FindProperty("RunDistance");
            Assert.NotNull(runDistanceProperty);
            Assert.False(runDistanceProperty.IsNullable);
        }

        [Fact]
        public void ApplicationDbContext_OnModelCreating_ShouldConfigureUserEntity()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase5")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(User));

            // Assert
            Assert.NotNull(entityType);
            
            // Check that required properties are configured
            var firstNameProperty = entityType.FindProperty("FirstName");
            Assert.NotNull(firstNameProperty);
            Assert.False(firstNameProperty.IsNullable);

            var lastNameProperty = entityType.FindProperty("LastName");
            Assert.NotNull(lastNameProperty);
            Assert.False(lastNameProperty.IsNullable);

            var createdAtProperty = entityType.FindProperty("CreatedAt");
            Assert.NotNull(createdAtProperty);
            Assert.False(createdAtProperty.IsNullable);
        }

        [Fact]
        public void ApplicationDbContext_OnModelCreating_ShouldConfigureTriathlonUserRelationship()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase6")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(Triathlon));
            var foreignKey = entityType?.GetForeignKeys().FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(User));

            // Assert
            Assert.NotNull(foreignKey);
            Assert.Equal("UserId", foreignKey.Properties.First().Name);
            Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        }

        [Fact]
        public void ApplicationDbContext_CanAddAndRetrieveTriathlon()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase7")
                .Options;

            using var context = new ApplicationDbContext(options);

            var triathlon = new Triathlon
            {
                RaceName = "Test Race",
                RaceDate = DateTime.UtcNow,
                Location = "Test Location",
                SwimDistance = 1500,
                SwimUnit = "meters",
                SwimTime = TimeSpan.FromMinutes(30),
                BikeDistance = 40,
                BikeUnit = "km",
                BikeTime = TimeSpan.FromHours(2),
                RunDistance = 10,
                RunUnit = "km",
                RunTime = TimeSpan.FromMinutes(45),
                UserId = "test-user-id"
            };

            // Act
            context.Triathlons.Add(triathlon);
            context.SaveChanges();

            var retrievedTriathlon = context.Triathlons.FirstOrDefault(t => t.RaceName == "Test Race");

            // Assert
            Assert.NotNull(retrievedTriathlon);
            Assert.Equal("Test Race", retrievedTriathlon.RaceName);
            Assert.Equal("Test Location", retrievedTriathlon.Location);
            Assert.Equal(1500, retrievedTriathlon.SwimDistance);
            Assert.Equal("test-user-id", retrievedTriathlon.UserId);
        }

        [Fact]
        public void ApplicationDbContext_CanAddAndRetrieveUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase8")
                .Options;

            using var context = new ApplicationDbContext(options);

            var user = new User
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();

            var retrievedUser = context.Users.FirstOrDefault(u => u.Email == "testuser@example.com");

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("testuser@example.com", retrievedUser.Email);
            Assert.Equal("Test", retrievedUser.FirstName);
            Assert.Equal("User", retrievedUser.LastName);
        }

        [Fact]
        public void ApplicationDbContext_CanQueryTriathlonsByUserId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase9")
                .Options;

            using var context = new ApplicationDbContext(options);

            var triathlon1 = new Triathlon
            {
                RaceName = "Race 1",
                RaceDate = DateTime.UtcNow,
                Location = "Location 1",
                SwimDistance = 1500,
                SwimUnit = "meters",
                SwimTime = TimeSpan.FromMinutes(30),
                BikeDistance = 40,
                BikeUnit = "km",
                BikeTime = TimeSpan.FromHours(2),
                RunDistance = 10,
                RunUnit = "km",
                RunTime = TimeSpan.FromMinutes(45),
                UserId = "user1"
            };

            var triathlon2 = new Triathlon
            {
                RaceName = "Race 2",
                RaceDate = DateTime.UtcNow,
                Location = "Location 2",
                SwimDistance = 1500,
                SwimUnit = "meters",
                SwimTime = TimeSpan.FromMinutes(30),
                BikeDistance = 40,
                BikeUnit = "km",
                BikeTime = TimeSpan.FromHours(2),
                RunDistance = 10,
                RunUnit = "km",
                RunTime = TimeSpan.FromMinutes(45),
                UserId = "user2"
            };

            context.Triathlons.AddRange(triathlon1, triathlon2);
            context.SaveChanges();

            // Act
            var user1Triathlons = context.Triathlons.Where(t => t.UserId == "user1").ToList();
            var user2Triathlons = context.Triathlons.Where(t => t.UserId == "user2").ToList();

            // Assert
            Assert.Single(user1Triathlons);
            Assert.Equal("Race 1", user1Triathlons[0].RaceName);
            
            Assert.Single(user2Triathlons);
            Assert.Equal("Race 2", user2Triathlons[0].RaceName);
        }
    }
} 