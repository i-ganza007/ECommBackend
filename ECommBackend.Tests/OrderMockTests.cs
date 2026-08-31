using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Tests
{
    [TestFixture]
    internal class OrderModelTests
    {
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _orderCreatorId = Guid.NewGuid();
        private readonly DateTime _createdDate = DateTime.Now;

        private OrderModel CreateValidOrder()
        {
            return new OrderModel(
                _orderId,
                100.00,
                _orderCreatorId,
                _createdDate,
                OrderStatus.Pending
            );
        }

        private static IList<ValidationResult> Validate(OrderModel order)
        {
            var context = new ValidationContext(order);

            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                order,
                context,
                results,
                validateAllProperties: true
            );

            return results;
        }

        [Test]
        public void Constructor_ShouldSetOrderId()
        {
            var order = CreateValidOrder();

            Assert.That(order.OrderId, Is.EqualTo(_orderId));
        }

        [Test]
        public void Constructor_ShouldSetTotalPrice()
        {
            var order = CreateValidOrder();

            Assert.That(order.TotalPrice, Is.EqualTo(100.00));
        }

        [Test]
        public void Constructor_ShouldSetOrderCreatorId()
        {
            var order = CreateValidOrder();

            Assert.That(order.OrderCreatorId, Is.EqualTo(_orderCreatorId));
        }

        [Test]
        public void Constructor_ShouldSetCreatedDate()
        {
            var order = CreateValidOrder();

            Assert.That(order.CreatedDate, Is.EqualTo(_createdDate));
        }

        [Test]
        public void Constructor_ShouldSetOrderStatus()
        {
            var order = CreateValidOrder();

            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Pending));
        }

        [Test]
        public void Products_ShouldBeInitializedAsEmptyCollection()
        {
            var order = CreateValidOrder();

            Assert.That(order.Products, Is.Not.Null);
            Assert.That(order.Products, Is.Empty);
        }

        [Test]
        public void TotalPrice_GreaterThanOrEqualToOne_ShouldPassValidation()
        {
            var order = CreateValidOrder();

            var results = Validate(order);

            Assert.That(
                results.Any(x =>
                    x.MemberNames.Contains(nameof(OrderModel.TotalPrice))),
                Is.False
            );
        }

        [Test]
        public void TotalPrice_Zero_ShouldFailValidation()
        {
            var order = new OrderModel(
                Guid.NewGuid(),
                0,
                Guid.NewGuid(),
                DateTime.Now,
                OrderStatus.Pending
            );

            var results = Validate(order);

            Assert.That(
                results.Any(x =>
                    x.MemberNames.Contains(nameof(OrderModel.TotalPrice))),
                Is.True
            );
        }

        [Test]
        public void TotalPrice_Negative_ShouldFailValidation()
        {
            var order = new OrderModel(
                Guid.NewGuid(),
                -50,
                Guid.NewGuid(),
                DateTime.Now,
                OrderStatus.Pending
            );

            var results = Validate(order);

            Assert.That(
                results.Any(x =>
                    x.MemberNames.Contains(nameof(OrderModel.TotalPrice))),
                Is.True
            );
        }

        [Test]
        public void Products_CanContainProducts()
        {
            var order = CreateValidOrder();

            var product = new ProductModel(
                Guid.NewGuid(),
                "SKU123",
                "Cleanser",
                "Cleanser remover and cleaner",
                Guid.NewGuid()
            );

            order.Products.Add(product);

            Assert.That(order.Products, Has.Count.EqualTo(1));
            Assert.That(order.Products.First(), Is.EqualTo(product));
        }
    }
}