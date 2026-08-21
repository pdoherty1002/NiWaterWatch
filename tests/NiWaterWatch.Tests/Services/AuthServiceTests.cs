using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using NiWaterWatch.Api.Contracts;
using NiWaterWatch.Api.Services;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Domain.Interfaces;

namespace NiWaterWatch.Tests.Services;

public class AuthServiceTests
{
    // Builds a real IConfiguration backed by an in-memory dictionary instead of
    // appsettings.json — same values GenerateToken needs, no file on disk required.
    private static IConfiguration CreateConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "this-is-a-fake-test-only-signing-key-not-the-real-one" },
            { "Jwt:Issuer", "NiWaterWatch" },
            { "Jwt:Audience", "NiWaterWatch" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenEmailIsNew()
    {
        // Arrange — no existing user with this email.
        var mockRepo = new Mock<IRepository<ApplicationUser, Guid>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(new List<ApplicationUser>());

        var service = new AuthService(mockRepo.Object, CreateConfiguration());

        // Act
        var result = await service.RegisterAsync(new RegisterRequest("new@test.com", "password123"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@test.com", result!.Email);
        Assert.False(string.IsNullOrEmpty(result.Token));

        // Confirms the service actually tried to save the new user, not just
        // returned a token without persisting anything.
        mockRepo.Verify(r => r.AddAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        // Arrange — repository reports a user already sitting at this email.
        var existingUser = new ApplicationUser { Email = "taken@test.com", PasswordHash = "irrelevant" };

        var mockRepo = new Mock<IRepository<ApplicationUser, Guid>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(new List<ApplicationUser> { existingUser });

        var service = new AuthService(mockRepo.Object, CreateConfiguration());

        // Act
        var result = await service.RegisterAsync(new RegisterRequest("taken@test.com", "password123"));

        // Assert
        Assert.Null(result);

        // Confirms registration bailed out before ever trying to add a duplicate.
        mockRepo.Verify(r => r.AddAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreCorrect()
    {
        // Arrange — a real BCrypt hash, same as AuthService would have created at registration.
        var realHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var existingUser = new ApplicationUser { Email = "user@test.com", PasswordHash = realHash };

        var mockRepo = new Mock<IRepository<ApplicationUser, Guid>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(new List<ApplicationUser> { existingUser });

        var service = new AuthService(mockRepo.Object, CreateConfiguration());

        // Act
        var result = await service.LoginAsync(new LoginRequest("user@test.com", "correct-password"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user@test.com", result!.Email);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsWrong()
    {
        // Arrange
        var realHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var existingUser = new ApplicationUser { Email = "user@test.com", PasswordHash = realHash };

        var mockRepo = new Mock<IRepository<ApplicationUser, Guid>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(new List<ApplicationUser> { existingUser });

        var service = new AuthService(mockRepo.Object, CreateConfiguration());

        // Act — same email, wrong password.
        var result = await service.LoginAsync(new LoginRequest("user@test.com", "wrong-password"));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenEmailDoesNotExist()
    {
        // Arrange — repository finds nobody at all with this email.
        var mockRepo = new Mock<IRepository<ApplicationUser, Guid>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(new List<ApplicationUser>());

        var service = new AuthService(mockRepo.Object, CreateConfiguration());

        // Act
        var result = await service.LoginAsync(new LoginRequest("nobody@test.com", "whatever"));

        // Assert
        Assert.Null(result);
    }
}