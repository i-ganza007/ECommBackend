using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using NUnit.Framework;

namespace ECommBackend.Tests.Models;

[TestFixture]
public class UserModelTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var createdDate = new DateTime(2025, 1, 2);
        var user = new UserModel(userId, "Jane", "Doe", "jane@example.com", 30, "password", "refresh-token", createdDate);

        Assert.Multiple(() =>
        {
            Assert.That(user.UserId, Is.EqualTo(userId));
            Assert.That(user.FirstName, Is.EqualTo("Jane"));
            Assert.That(user.LastName, Is.EqualTo("Doe"));
            Assert.That(user.Email, Is.EqualTo("jane@example.com"));
            Assert.That(user.Age, Is.EqualTo(30));
            Assert.That(user.Password, Is.EqualTo("password"));
            Assert.That(user.RefreshToken, Is.EqualTo("refresh-token"));
            Assert.That(user.CreatedDate, Is.EqualTo(createdDate));
        });
    }

    [Test]
    public void PasswordChanger_UpdatesAndReturnsPassword()
    {
        var user = CreateUser();

        var result = user.PasswordChanger("new-password");

        Assert.That(result, Is.EqualTo("new-password"));
        Assert.That(user.Password, Is.EqualTo("new-password"));
    }

    [Test]
    public void RefreshTokenChanger_UpdatesAndReturnsToken()
    {
        var user = CreateUser();

        var result = user.RefreshTokenChanger("new-refresh-token");

        Assert.That(result, Is.EqualTo("new-refresh-token"));
        Assert.That(user.RefreshToken, Is.EqualTo("new-refresh-token"));
    }

    private static UserModel CreateUser() =>
        new(Guid.NewGuid(), "Jane", "Doe", "jane@example.com", 30, "password", "refresh-token", DateTime.UtcNow);
}

[TestFixture]
public class AdminModelTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var createdDate = new DateTime(2025, 2, 3);
        var admin = new AdminModel(userId, "John", "Smith", "john@example.com", 35, "password", "refresh-token", createdDate);

        Assert.Multiple(() =>
        {
            Assert.That(admin.UserId, Is.EqualTo(userId));
            Assert.That(admin.FirstName, Is.EqualTo("John"));
            Assert.That(admin.LastName, Is.EqualTo("Smith"));
            Assert.That(admin.Email, Is.EqualTo("john@example.com"));
            Assert.That(admin.Age, Is.EqualTo(35));
            Assert.That(admin.Password, Is.EqualTo("password"));
            Assert.That(admin.RefreshToken, Is.EqualTo("refresh-token"));
            Assert.That(admin.CreatedDate, Is.EqualTo(createdDate));
        });
    }

    [Test]
    public void PasswordChanger_UpdatesAndReturnsPassword()
    {
        var admin = CreateAdmin();

        var result = admin.PasswordChanger("new-password");

        Assert.That(result, Is.EqualTo("new-password"));
        Assert.That(admin.Password, Is.EqualTo("new-password"));
    }

    [Test]
    public void RefreshTokenChanger_UpdatesAndReturnsToken()
    {
        var admin = CreateAdmin();

        var result = admin.RefreshTokenChanger("new-refresh-token");

        Assert.That(result, Is.EqualTo("new-refresh-token"));
        Assert.That(admin.RefreshToken, Is.EqualTo("new-refresh-token"));
    }

    private static AdminModel CreateAdmin() =>
        new(Guid.NewGuid(), "John", "Smith", "john@example.com", 35, "password", "refresh-token", DateTime.UtcNow);
}

[TestFixture]
public class ProductModelTests
{
    [Test]
    public void Constructor_SetsAllPublicProperties()
    {
        var productId = Guid.NewGuid();
        var createdAt = new DateTime(2025, 3, 4);
        var owner = CreateAdmin();
        var product = new ProductModel(productId, "Laptop", "Portable computer", 999.99m, createdAt, owner);

        Assert.Multiple(() =>
        {
            Assert.That(product.ProductId, Is.EqualTo(productId));
            Assert.That(product.Name, Is.EqualTo("Laptop"));
            Assert.That(product.Description, Is.EqualTo("Portable computer"));
            Assert.That(product.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(product.Owner, Is.SameAs(owner));
        });
    }

    [Test]
    public void PriceChanger_UpdatesAndReturnsPrice()
    {
        var product = new ProductModel(Guid.NewGuid(), "Laptop", "Portable computer", 999.99m, DateTime.UtcNow, CreateAdmin());

        var result = product.PriceChanger(799.99m);

        Assert.That(result, Is.EqualTo(799.99m));
    }

    private static AdminModel CreateAdmin() =>
        new(Guid.NewGuid(), "John", "Smith", "john@example.com", 35, "password", "refresh-token", DateTime.UtcNow);
}

[TestFixture]
public class OrderModelTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var orderId = Guid.NewGuid();
        var createdDate = new DateTime(2025, 4, 5);
        var creator = new UserModel(Guid.NewGuid(), "Jane", "Doe", "jane@example.com", 30, "password", "refresh-token", DateTime.UtcNow);
        var products = Array.Empty<ProductModel>();
        var order = new OrderModel(orderId, 149.50m, creator, products, createdDate, OrderStatus.Pending);

        Assert.Multiple(() =>
        {
            Assert.That(order.OrderId, Is.EqualTo(orderId));
            Assert.That(order.TotalPrice, Is.EqualTo(149.50m));
            Assert.That(order.OrderCreator, Is.SameAs(creator));
            Assert.That(order.Products, Is.SameAs(products));
            Assert.That(order.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Pending));
        });
    }
}
