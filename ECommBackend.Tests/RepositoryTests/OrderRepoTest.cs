using ECommBackend.CustomErrors;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Tests.RepositoryTests
{
    // Each SQLiteConn plays one side of the cancel/processor race. A shared-cache in-memory
    // database lets separate contexts and connections see the same rows.
    [TestFixture]
    internal class OrderRepoTest
    {
        private const int StartingUnits = 5;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _variantId = Guid.NewGuid();

        private string _connectionString = null!;
        private SqliteConnection _keepAlive = null!;

        [SetUp]
        public async Task SetUp()
        {
            _connectionString = $"Data Source=orders-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            // The in-memory database lives only while a connection to it is open.
            _keepAlive = new SqliteConnection(_connectionString);
            await _keepAlive.OpenAsync();

            await using var db = CreateContext();
            await db.Database.EnsureCreatedAsync();

            var adminId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var imageId = Guid.NewGuid();

            var admin = new AdminModel(adminId, "Ada", "Admin", "admin@test.local", 30, "hash", "refresh");
            db.Admins.Add(admin);
            db.Categories.Add(new CategoryModel(categoryId, "Skincare", "Skincare products"));
            db.Images.Add(new ImageModel(imageId, new byte[] { 1 }));
            // The owner FK is the shadow column OwnerUserId, not AdminOwnerId, so set the navigation.
            db.Products.Add(new ProductModel(productId, "SKU-1", "Serum", "A serum", adminId, categoryId) { Owner = admin });
            db.Variants.Add(new VariantModel(_variantId, 30.0, 25.50m, productId, StartingUnits, imageId));
            db.Users.Add(new UserModel(_userId, "Una", "User", "user@test.local", 25, "hash", "refresh"));
            db.Orders.Add(CreateValidOrder());
            await db.SaveChangesAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _keepAlive.DisposeAsync();
        }

        private SQLiteConn CreateContext() =>
            new(new DbContextOptionsBuilder<SQLiteConn>().UseSqlite(_connectionString).Options);

        private OrderModel CreateValidOrder() =>
            new(_orderId, 25.50, _userId, DateTime.UtcNow, OrderStatus.Pending);

        private IReadOnlyCollection<VariantReservation> OneUnit() =>
            new[] { new VariantReservation(_variantId, 1) };

        private async Task<(OrderStatus Status, int Units)> ReadState()
        {
            await using var db = CreateContext();
            var order = await db.Orders.AsNoTracking().SingleAsync(x => x.OrderId == _orderId);
            var variant = await db.Variants.AsNoTracking().SingleAsync(x => x.VariantId == _variantId);
            return (order.OrderStatus, variant.Units);
        }

        [Test]
        public void Constructor_ShouldAssignVersion()
        {
            Assert.That(CreateValidOrder().Version, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public async Task CancelOrder_ShouldCancel_WhenPendingAndOwned()
        {
            await using var db = CreateContext();

            var order = await new OrderRepo(db).CancelOrder(_orderId, _userId, CancellationToken.None);

            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Cancelled));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Cancelled, StartingUnits)));
        }

        [Test]
        public async Task CancelOrder_ShouldThrowNotFound_ForOtherUser()
        {
            await using var db = CreateContext();

            Assert.ThrowsAsync<OrderNotFoundException>(() =>
                new OrderRepo(db).CancelOrder(_orderId, Guid.NewGuid(), CancellationToken.None));
            Assert.That((await ReadState()).Status, Is.EqualTo(OrderStatus.Pending));
        }

        [Test]
        public async Task CancelOrder_ShouldThrowNotPending_WhenProcessed()
        {
            await using (var processorDb = CreateContext())
            {
                await new OrderRepo(processorDb).TryFinalizeOrder(_orderId, OneUnit(), CancellationToken.None);
            }

            await using var cancelDb = CreateContext();
            Assert.ThrowsAsync<OrderNotPendingError>(() =>
                new OrderRepo(cancelDb).CancelOrder(_orderId, _userId, CancellationToken.None));
        }

        [Test]
        public async Task TryFinalizeOrder_ShouldLeaveStockUntouched_WhenCancelWinsRace()
        {
            await using var processorDb = CreateContext();
            await using var cancelDb = CreateContext();
            var processorRepo = new OrderRepo(processorDb);

            // The processor reads the order while it is still Pending...
            await processorRepo.GetSingleOrder(_orderId, CancellationToken.None);
            // ...then the cancellation commits before the processor finalizes.
            await new OrderRepo(cancelDb).CancelOrder(_orderId, _userId, CancellationToken.None);

            var result = await processorRepo.TryFinalizeOrder(_orderId, OneUnit(), CancellationToken.None);

            Assert.That(result, Is.EqualTo(FinalizeResult.Cancelled));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Cancelled, StartingUnits)));
        }

        [Test]
        public async Task CancelOrder_ShouldThrowNotPending_WhenProcessorWinsRace()
        {
            await using var processorDb = CreateContext();
            await using var cancelDb = CreateContext();

            // The cancellation reads the order while it is still Pending...
            await cancelDb.Orders.SingleAsync(x => x.OrderId == _orderId);
            // ...then the processor commits first, so only the concurrency token can stop the cancel.
            var result = await new OrderRepo(processorDb).TryFinalizeOrder(_orderId, OneUnit(), CancellationToken.None);

            Assert.That(result, Is.EqualTo(FinalizeResult.Processed));
            Assert.ThrowsAsync<OrderNotPendingError>(() =>
                new OrderRepo(cancelDb).CancelOrder(_orderId, _userId, CancellationToken.None));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Processed, StartingUnits - 1)));
        }

        [Test]
        public async Task TryFinalizeOrder_ShouldRestoreStock_WhenReservationFailsPartway()
        {
            await using var db = CreateContext();
            var reservations = new[]
            {
                new VariantReservation(_variantId, 1),
                new VariantReservation(_variantId, StartingUnits * 10)
            };

            var result = await new OrderRepo(db).TryFinalizeOrder(_orderId, reservations, CancellationToken.None);

            Assert.That(result, Is.EqualTo(FinalizeResult.Rejected));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Rejected, StartingUnits)));
        }

        [Test]
        public async Task TryFinalizeOrder_ShouldReject_WhenNothingToReserve()
        {
            await using var db = CreateContext();

            var result = await new OrderRepo(db).TryFinalizeOrder(
                _orderId, Array.Empty<VariantReservation>(), CancellationToken.None);

            Assert.That(result, Is.EqualTo(FinalizeResult.Rejected));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Rejected, StartingUnits)));
        }

        [Test]
        public async Task TryFinalizeOrder_ShouldReportAlreadyFinalized_ForDuplicateMessage()
        {
            await using (var firstDb = CreateContext())
            {
                await new OrderRepo(firstDb).TryFinalizeOrder(_orderId, OneUnit(), CancellationToken.None);
            }

            await using var secondDb = CreateContext();
            var result = await new OrderRepo(secondDb).TryFinalizeOrder(_orderId, OneUnit(), CancellationToken.None);

            Assert.That(result, Is.EqualTo(FinalizeResult.AlreadyFinalized));
            Assert.That(await ReadState(), Is.EqualTo((OrderStatus.Processed, StartingUnits - 1)));
        }
    }
}
