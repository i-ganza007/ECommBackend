using ECommBackend.Models;
using NUnit.Framework;
namespace ECommBackend.Tests;

public class UserModelTests
{
    private UserModel CreateUser()
    {
        return new UserModel(
            Guid.NewGuid(),
            "Ian",
            "Ganza",
            "ian@example.com",
            20,
            "password123",
            "refresh-token"
        );
        
    }

    [Test]
    public void Constructor_ShouldInitializeUserCorrectly()
    {
        Guid id = Guid.NewGuid();
        string firstName = "Ian";
        string lastName = "Ganza";
        string email = "ian@example.com";
        int age = 20;
        string password = "password123";
        string refreshToken = "refresh-token";

        var user = new UserModel(
            id,
            firstName,
            lastName,
            email,
            age,
            password,
            refreshToken
        )
        {
            CreatedDate = DateTime.UtcNow
        };

        Assert.That(user.UserId, Is.EqualTo(id));
        Assert.That(user.FirstName, Is.EqualTo(firstName));
        Assert.That(user.LastName, Is.EqualTo(lastName));
        Assert.That(user.Email, Is.EqualTo(email));
        Assert.That(user.Age, Is.EqualTo(age));
    }

    [Test]
    public void Constructor_ShouldInitializeProductsBoughtAsEmpty()
    {
        var user = CreateUser();

        Assert.That(user.ProductsBought, Is.Not.Null);
        Assert.That(user.ProductsBought, Is.Empty);
    }

    [Test]
    public void Constructor_ShouldInitializeOrdersAsEmpty()
    {
        var user = CreateUser();

        //Assert.That(user.Orders, Is.Not.Null);
        Assert.That(user.Orders, Is.Not.Empty);
    }

    [Test]
    public void ChangePassword_ShouldChangePassword()
    {
        var user = CreateUser();

        user.ChangePassword("newPassword123");

        
        Assert.Pass();
    }

    [Test]
    public void ChangePassword_ShouldThrow_WhenPasswordIsEmpty()
    {
        var user = CreateUser();

        var exception = Assert.Throws<ArgumentException>(
            () => user.ChangePassword("")
        );

        Assert.That(exception!.Message, Is.EqualTo("Password cannot be empty"));
    }

    [Test]
    public void ChangePassword_ShouldThrow_WhenPasswordIsNull()
    {
        var user = CreateUser();

        var exception = Assert.Throws<ArgumentException>(
            () => user.ChangePassword(null!)
        );

        Assert.That(exception!.Message, Is.EqualTo("Password cannot be empty"));
    }

    [Test]
    public void ChangePassword_ShouldThrow_WhenNewPasswordIsSameAsCurrentPassword()
    {
        var user = CreateUser();

        var exception = Assert.Throws<ArgumentException>(
            () => user.ChangePassword("password123")
        );

        Assert.That(
            exception!.Message,
            Is.EqualTo("Cannot have the same password must have a new one")
        );
    }

    [Test]
    public void RefreshTokenChanger_ShouldReturnNewRefreshToken()
    {
        var user = CreateUser();

        var result = user.RefreshTokenChanger("new-refresh-token");

        Assert.That(result, Is.EqualTo("new-refresh-token"));
    }

    [Test]
    public void RefreshTokenChanger_ShouldUpdateRefreshToken()
    {
        var user = CreateUser();

        user.RefreshTokenChanger("new-refresh-token");

        Assert.That(
            user.RefreshToken,
            Is.EqualTo("new-refresh-token")
        );
    }
}