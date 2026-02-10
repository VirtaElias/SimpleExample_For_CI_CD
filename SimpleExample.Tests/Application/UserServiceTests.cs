using FluentAssertions;
using Moq;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using SimpleExample.Application.Services;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _service = new UserService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        // Mock: Email ei ole käytössä
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        // Varmista että AddAsync kutsuttiin kerran
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "existing@example.com"
        };

        User existingUser = new User("Maija", "Virtanen", "existing@example.com");

        // Mock: Email on jo käytössä!
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*jo olemassa*");

        // Varmista että AddAsync EI kutsuttu
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    // TEHTÄVÄ: Kirjoita itse testit seuraaville:
    // 1. GetByIdAsync - löytyy
    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        User user = new User("Matti", "Meikäläinen", "matti@test.com");

        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(user);

        // Act
        UserDto result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("matti@test.com");
    }

    // 2. GetByIdAsync - ei löydy
    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(id);

        result.Should().BeNull();
    }


    // 3. GetAllAsync - palauttaa listan
    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        List<User> users = new()
    {
        new User("Matti", "Meikäläinen", "matti@test.com"),
        new User("Maija", "Virtanen", "maija@test.com")
    };

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    // 4. UpdateAsync - onnistuu
    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUser()
    {
        Guid id = Guid.NewGuid();
        User user = new User("Old", "Name", "old@test.com");

        UpdateUserDto dto = new()
        {
            FirstName = "New",
            LastName = "Name",
            Email = "new@test.com"
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(user);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var result = await _service.UpdateAsync(id, dto);

        result.FirstName.Should().Be("New");
        result.Email.Should().Be("new@test.com");
    }

    // 5. UpdateAsync - käyttäjää ei löydy
    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        UpdateUserDto dto = new();

        var result = await _service.UpdateAsync(id, dto);

        result.Should().BeNull();
    }


    // 6. DeleteAsync - onnistuu
    [Fact]
    public async Task DeleteAsync_WhenUserExists_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        bool result = await _service.DeleteAsync(id);

        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
    }


    // 7. DeleteAsync - käyttäjää ei löydy
    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        Guid id = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(false);

        bool result = await _service.DeleteAsync(id);

        result.Should().BeFalse();
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }


}
