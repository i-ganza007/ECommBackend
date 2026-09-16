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

        private readonly AzureServiceBusClass _azureBus;
        public OrderService(IOrderRepo orderRepo, IProductRepo productRepo, AzureServiceBusClass azureBus) {
          _orderRepo = orderRepo;
          _productRepo = productRepo;
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

        public async Task UpdateOrderStatus(Guid _orderId,OrderStatus status,CancellationToken ctx) {
             await _orderRepo.UpdateOrder(_orderId, status, ctx);
        }

        public async Task<Guid> CreateOrder(DTOrder order, Guid orderCreatorId, CancellationToken ctx) {
           var productIds = ParseProductIds(order._ProductsIds);

           // One query instead of one per id, so nothing here has to fan out into nested awaits.
           var products = await _productRepo.GetProductsByIds(productIds, ctx);
           GuardAllProductsExist(productIds, products);
            var orderId = Guid.NewGuid();

           var newOrder = new OrderModel(
               orderId,
               (double)products.Sum(ResolveUnitPrice),
               orderCreatorId,
               DateTime.UtcNow,
               OrderStatus.Pending);

           foreach (var product in products)
           {
                foreach(var variant in product.Variants)
                {
                    var productVar = new ProductModel(
                        product.ProductId,
                        product.Base_SKU,
                        product.Name,
                        product.Description,
                        product.AdminOwnerId,
                        product.CategoryId,
                        Texture: product.Texture,
                        Skin_Type: product.Skin_Type,
                        Key_Ingr: product.Key_Ingr
                        ){Variants=new List<VariantModel> { variant} };


                    var orderMess = new OrderModel(
                        orderId,
                        (double)variant.Price,
                        orderCreatorId,
                        DateTime.Now,
                        OrderStatus.Pending
                        )
                    { Products=new List<ProductModel> { productVar } };
                    await _azureBus.CreateOrder(orderMess);
                }
               // Tracked entities from the same scoped context, so this only writes the join rows.
               newOrder.Products.Add(product);
           }

            

            return await _orderRepo.CreateOrder(newOrder, ctx);
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

        // DTOrder only carries product ids, so an order has no variant of its own to price against.
        // Cheapest in-stock variant is the stand-in rule; replace this once orders carry variants.
        private static decimal ResolveUnitPrice(ProductModel product) {
            var variant = product.Variants
                .Where(x => x.Units > 0)
                .OrderBy(x => x.Price)
                .FirstOrDefault();

            if (variant is null)
            {
                throw new VariantNotFoundError(product.ProductId, $"Product {product.ProductId} has no variant in stock");
            }

            return variant.Price;
        }

        public async Task<UserDTO> GetOrderCreator(Guid _userId, CancellationToken ctx) { 
          var result = await _orderRepo.GetOrderCreator(_userId, ctx);
          return result.ModelToRecordDTO();
        }
    }
}
