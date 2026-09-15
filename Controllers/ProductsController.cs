using ECommBackend.DTOs;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepo _productRepo;
        public ProductsController(IProductRepo productRepo) {
         _productRepo = productRepo;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllProducts(CancellationToken ctx, [FromHeader] string _adminId) {
            var result = await _productRepo.GetAllProductsByUser(Guid.Parse(_adminId), ctx);
            return Ok(result);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetAllProducts(CancellationToken ctx, [FromHeader] string _adminId,string productId)
        {
            var result = await _productRepo.GetSingleProduct(Guid.Parse(productId), ctx);
            return Ok(result);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(string productId,CancellationToken ctx) {
            await _productRepo.DeleteSingleProduct(Guid.Parse(productId), ctx);
            return Ok();
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateProduct(ProductDTO product, CancellationToken ctx) {
        //    await _productRepo.CreateProduct(product, ctx);
        //}

        [HttpGet("owner/{productId}")]
        public async Task<IActionResult> GetProductOwner(string productId,CancellationToken ctx) 
        {
            var result = _productRepo.GetProductOwner(Guid.Parse(productId), ctx);
            return Ok(result);
        }
    }
}
