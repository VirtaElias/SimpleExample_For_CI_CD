using FluentAssertions;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        user.Should().NotBeNull();
        user.FirstName.Should().Be("Matti");
        user.LastName.Should().Be("Meikäläinen");
        user.Email.Should().Be("matti@example.com");
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        Action act = () => new User("", "Meikäläinen", "test@test.com");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("firstName");
    }

    [Fact]
    public void Constructor_WithTooShortFirstName_ShouldThrowArgumentException()
    {
        Action act = () => new User("AB", "Meikäläinen", "test@test.com");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("firstName");
    }

    [Fact]
    public void Constructor_WithEmptyLastName_ShouldThrowArgumentException()
    {
        Action act = () => new User("Matti", "", "test@test.com");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("lastName");
    }

    [Fact]
    public void Constructor_WithTooShortLastName_ShouldThrowArgumentException()
    {
        Action act = () => new User("Matti", "XY", "test@test.com");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("lastName");
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
    {
        Action act = () => new User("Matti", "Meikäläinen", "invalid-email");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("email");
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrowArgumentNullException()
    {
        Action act = () => new User(null!, "Meikäläinen", "test@test.com");

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData("Mat")]
    [InlineData("Matti")]
    [InlineData("MattiJohannes")]
    public void Constructor_WithValidFirstNameLengths_ShouldSucceed(string firstName)
    {
        User user = new User(firstName, "Meikäläinen", "test@test.com");

        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateUser()
    {
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        user.UpdateBasicInfo("Maija", "Virtanen");

        user.FirstName.Should().Be("Maija");
        user.LastName.Should().Be("Virtanen");
    }

    [Fact]
    public void UpdateBasicInfo_WithTooShortFirstName_ShouldThrowArgumentException()
    {
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        Action act = () => user.UpdateBasicInfo("AB", "Virtanen");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("firstName");
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        user.UpdateEmail("uusi@example.com");

        user.Email.Should().Be("uusi@example.com");
    }

    [Fact]
    public void UpdateEmail_WithInvalidEmail_ShouldThrowArgumentException()
    {
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        Action act = () => user.UpdateEmail("invalid-email");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("email");
    }
}
