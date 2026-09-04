using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Tests
{
    [TestFixture]
    internal class ProductModelTests
    {
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _adminOwnerId = Guid.NewGuid();

        private ProductModel CreateValidProduct()
        {
            return new ProductModel(
                _productId,
                "SKU-12345",
                "Lady Hue",
                "Quality Moisturisers for african",
                _adminOwnerId
            )
            {
                Category = ProductCategory.Moisturisers
            };
        }

        [Test]
        public void Constructor_ShouldSetProductId()
        {
            // Arrange & Act
            var product = CreateValidProduct();

            // Assert
            Assert.That(product.ProductId, Is.EqualTo(_productId));
        }

        [Test]
        public void Constructor_ShouldSetName()
        {
            var product = CreateValidProduct();

            Assert.That(product.Name, Is.EqualTo("Lady Hue"));
        }

        [Test]
        public void Constructor_ShouldSetDescription()
        {
            var product = CreateValidProduct();

            Assert.That(product.Description, Is.EqualTo("Quality Moisturisers for african"));
        }

        [Test]
        public void Constructor_ShouldSetBaseSku()
        {
            var product = CreateValidProduct();

            Assert.That(product.Base_SKU, Is.EqualTo("SKU-12345"));
        }

        [Test]
        public void Constructor_ShouldSetAdminOwnerId()
        {
            var product = CreateValidProduct();

            Assert.That(product.AdminOwnerId, Is.EqualTo(_adminOwnerId));
        }

        [Test]
        public void Variants_ShouldBeInitializedAsEmptyCollection()
        {
            var product = CreateValidProduct();

            Assert.That(product.Variants, Is.Not.Null);
            Assert.That(product.Variants, Is.Empty);
        }

        [Test]
        public void CreatedAt_ShouldBeSetAutomatically()
        {
            var before = DateTime.Now;

            var product = CreateValidProduct();

            var after = DateTime.Now;

            Assert.That(product.CreatedAt, Is.GreaterThanOrEqualTo(before));
            Assert.That(product.CreatedAt, Is.LessThanOrEqualTo(after));
        }

        [Test]
        public void UpdateAt_ShouldInitiallyBeNull()
        {
            var product = CreateValidProduct();

            Assert.That(product.UpdateAt, Is.Null);
        }
    }
}