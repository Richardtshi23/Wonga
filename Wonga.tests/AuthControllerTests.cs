using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wonga.Server.Controllers;
using Wonga.Server.Models;
using Wonga.Server.interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class AuthControllerTests
{
    private readonly WongaDbContext _db;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<WongaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        _db = new WongaDbContext(options);

        _tokenServiceMock = new Mock<ITokenService>();
        _configMock = new Mock<IConfiguration>();

        _controller = new AuthController(_db, _tokenServiceMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenEmailIsNew()
    {
        // Arrange
        var dto = new userDTO { Name = "John", Surname = "Doe", Email = "john@wonga.com", Password = "Password123" };

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<OkResult>();
        _db.UserAccounts.Should().Contain(u => u.Email == "john@wonga.com");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailExists()
    {
        // Arrange
        var existingUser = new UserAccount { Email = "duplicate@wonga.com", Password = "...", Name = "X", Surname = "Y" };
        _db.UserAccounts.Add(existingUser);
        await _db.SaveChangesAsync();

        var dto = new userDTO { Email = "duplicate@wonga.com", Password = "password" };

        // Act
        var result = await _controller.Register(dto);

        // Assert
        var badRequest = result.As<BadRequestObjectResult>();
        badRequest.Value.Should().Be("Email already registered.");
    }

    [Fact]
    public async Task Me_ReturnsUserData_WhenAuthenticated()
    {
        // Arrange
        var user = new UserAccount { Id = 1, Name = "Wonga", Surname = "Dev", Email = "dev@wonga.com", Password = "..." };
        _db.UserAccounts.Add(user);
        await _db.SaveChangesAsync();

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.Me();

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Value.Should().NotBeNull();
    }
}