using ECommBackend.Models;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Tests
{
    [TestFixture]
    internal class VariantModelTests
    {
        private readonly Guid _variantId = Guid.NewGuid();
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _imageId = Guid.NewGuid();

        private VariantModel CreateValidVariant()
        {
            return new VariantModel(
                _variantId,
                100.0,
                25.50m,
                _productId,
                10,
                _imageId
            );
        }

        [Test]
        public void Constructor_ShouldSetVariantId()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.VariantId, Is.EqualTo(_variantId));
        }

        [Test]
        public void Constructor_ShouldSetSize()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.Size, Is.EqualTo(100.0));
        }

        [Test]
        public void Constructor_ShouldSetPrice()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.Price, Is.EqualTo(25.50m));
        }

        [Test]
        public void Constructor_ShouldSetProductModelId()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.ProductModelId, Is.EqualTo(_productId));
        }

        [Test]
        public void Constructor_ShouldSetUnits()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.Units, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_ShouldSetVariantImageId()
        {
            var variant = CreateValidVariant();

            Assert.That(variant.VariantImageId, Is.EqualTo(_imageId));
        }

        private static IList<ValidationResult> Validate(VariantModel variant)
        {
            var context = new ValidationContext(variant);

            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                variant,
                context,
                results,
                validateAllProperties: true
            );

            return results;
        }

        [Test]
        public void Size_GreaterThanZero_ShouldPassValidation()
        {
            var variant = CreateValidVariant();

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Size))),
                Is.False
            );
        }

        [Test]
        public void Size_Zero_ShouldFailValidation()
        {
            var variant = new VariantModel(
                Guid.NewGuid(),
                0,
                25.50m,
                Guid.NewGuid(),
                10,
                Guid.NewGuid()
            );

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Size))),
                Is.True
            );
        }

        [Test]
        public void Size_Negative_ShouldFailValidation()
        {
            var variant = new VariantModel(
                Guid.NewGuid(),
                -10,
                25.50m,
                Guid.NewGuid(),
                10,
                Guid.NewGuid()
            );

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Size))),
                Is.True
            );
        }

        [Test]
        public void Units_GreaterThanZero_ShouldPassValidation()
        {
            var variant = CreateValidVariant();

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Units))),
                Is.False
            );
        }

        [Test]
        public void Units_Zero_ShouldFailValidation()
        {
            var variant = new VariantModel(
                Guid.NewGuid(),
                100,
                25.50m,
                Guid.NewGuid(),
                0,
                Guid.NewGuid()
            );

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Units))),
                Is.True
            );
        }

        [Test]
        public void Units_Negative_ShouldFailValidation()
        {
            var variant = new VariantModel(
                Guid.NewGuid(),
                100,
                25.50m,
                Guid.NewGuid(),
                -5,
                Guid.NewGuid()
            );

            var results = Validate(variant);

            Assert.That(
                results.Any(x => x.MemberNames.Contains(nameof(VariantModel.Units))),
                Is.True
            );
        }
    }
}