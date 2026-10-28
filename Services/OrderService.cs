using System.Diagnostics;
using ECommBackend.CustomErrors;
using ECommBackend.DTOs;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class OrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;

        private readonly IUserRepo _userRepo;

        private readonly AzureServiceBusClass _azureBus;
        public OrderService(IOrderRepo orderRepo, IProductRepo productRepo, IUserRepo userRepo, AzureServiceBusClass azureBus) {
          _orderRepo = orderRepo;
          _productRepo = productRepo;
          _userRepo = userRepo;
          _azureBus = azureBus;
        }

        public async Task<OrderDTO> GetSingleOrder(Guid _orderId, CancellationToken ctx) { 
          var result = await _orderRepo.GetSingleOrder(_orderId, ctx);
          return result.ModelToRecordDTO();
        }
        public async Task<IQueryable<OrderDTO>?> GetAllOrders(Guid _userId, CancellationToken ctx) {
          var result = await _orderRepo.GetAllOrders(_userId, ctx);
          return result.Select(x=>x.ModelToRecordDTO());
        }

        // Called once the final status is saved: records the outcome and tells the buyer.
        public async Task OnOrderFinalized(
            Guid _orderId,
            string outcome,
            DateTime createdAtUtc,
            CancellationToken ctx,
            string? rejectionReason = null)
        {
             OrderTelemetry.RecordFinalized(
                 outcome,
                 rejectionReason,
                 Math.Max(0, (DateTime.UtcNow - DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc)).TotalMilliseconds));
             await PublishOrderEmail(_orderId, ctx);
        }

        public async Task<OrderSummaryDTO> CancelOrder(Guid _orderId, Guid _userId, CancellationToken ctx) {
             var order = await _orderRepo.CancelOrder(_orderId, _userId, ctx);
             Activity.Current?.SetTag("order.id", _orderId);
             await OnOrderFinalized(_orderId, OrderTelemetry.OutcomeCancelled, order.CreatedDate, ctx);
             return order.ModelToSummaryDTO();
        }

        private async Task PublishOrderEmail(Guid _orderId, CancellationToken ctx) {
             var order = await _orderRepo.GetSingleOrder(_orderId, ctx);
             var user = await _userRepo.GetSingleUser(ctx, order.OrderCreatorId)
                 ?? throw new UserNotFoundError(order.OrderCreatorId, $"Order {_orderId} has no creator");

             await _azureBus.PublishOrderEmail(new OrderEmailMessage(
                 order.OrderId,
                 user.Email,
                 $"{user.FirstName} {user.LastName}",
                 order.OrderStatus,
                 order.TotalPrice,
                 order.CreatedDate), ctx);
        }

        public async Task<Guid> CreateOrder(DTOrder order, Guid orderCreatorId, CancellationToken ctx) {
           var productIds = ParseProductIds(order._ProductsIds);

           var products = await _productRepo.GetProductsByIds(productIds, ctx);
           GuardAllProductsExist(productIds, products);
            var orderId = Guid.NewGuid();

           // The variant priced here is the one the processor reserves.
           var lines = products.Select(product => (Product: product, Variant: ResolveVariant(product))).ToList();

           var newOrder = new OrderModel(
               orderId,
               (double)lines.Sum(line => line.Variant.Price),
               orderCreatorId,
               DateTime.UtcNow,
               OrderStatus.Pending);

           foreach (var product in products)
           {
               newOrder.Products.Add(product);
           }

            await _orderRepo.CreateOrder(newOrder, ctx);
            OrderTelemetry.RecordPlaced(newOrder.TotalPrice);
            Activity.Current?.SetTag("order.id", orderId);

           // One message per order: its MessageId is the order id, so a second message for the same
           // order would be skipped as a duplicate. Copies keep EF navigation cycles out of the JSON.
           var orderMess = new OrderModel(
               orderId,
               newOrder.TotalPrice,
               orderCreatorId,
               newOrder.CreatedDate,
               OrderStatus.Pending
               )
           { Products = lines.Select(line => ToMessageProduct(line.Product, line.Variant)).ToList() };
           await _azureBus.CreateOrder(orderMess);

            return orderId;
        }

        private static ProductModel ToMessageProduct(ProductModel product, VariantModel variant) {
            var variantCopy = new VariantModel(
                variant.VariantId,
                variant.Size,
                variant.Price,
                variant.ProductModelId,
                variant.Units,
                variant.VariantImageId);

            return new ProductModel(
                product.ProductId,
                product.Base_SKU,
                product.Name,
                product.Description,
                product.AdminOwnerId,
                product.CategoryId,
                texture: product.Texture,
                skin_Type: product.Skin_Type,
                key_Ingr: product.Key_Ingr
                ){Variants=new List<VariantModel> { variantCopy } };
        }

        private static List<Guid> ParseProductIds(ICollection<string> rawIds) {
            if (rawIds is null || rawIds.Count == 0)
            {
                throw new ArgumentException("An order needs at least one product");
            }

            var productIds = new List<Guid>(rawIds.Count);
            foreach (var rawId in rawIds)
            {
                if (!Guid.TryParse(rawId, out var productId))
                {
                    throw new ArgumentException($"'{rawId}' is not a valid product id");
                }
                productIds.Add(productId);
            }

            return productIds.Distinct().ToList();
        }

        private static void GuardAllProductsExist(IEnumerable<Guid> productIds, IEnumerable<ProductModel> products) {
            var missing = productIds.Except(products.Select(x => x.ProductId)).ToList();
            if (missing.Count != 0)
            {
                throw new ProductNotFoundError($"Products {string.Join(", ", missing)} can't be found");
            }
        }


        private static VariantModel ResolveVariant(ProductModel product) {
            var variant = product.Variants
                .Where(x => x.Units > 0)
                .OrderBy(x => x.Price)
                .FirstOrDefault();

            if (variant is null)
            {
                throw new VariantNotFoundError(product.ProductId, $"Product {product.ProductId} has no variant in stock");
            }

            return variant;
        }

        public async Task<UserDTO> GetOrderCreator(Guid _userId, CancellationToken ctx) { 
          var result = await _orderRepo.GetOrderCreator(_userId, ctx);
          return result.ModelToRecordDTO();
        }
    }
}
