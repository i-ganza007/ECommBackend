using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariantsController : ControllerBase
    {
        private readonly IVariantRepo _variantRepo;
        public VariantsController(IVariantRepo variantRepo)
        {
            _variantRepo = variantRepo;
        }

        [HttpGet("{_productId}")]
        public async Task<IActionResult> GetVariantsForProduct(CancellationToken ctx,string _productId) {
            var result = await _variantRepo.GetAllVariantsForProduct(Guid.Parse(_productId), ctx);
            return Ok(result);
        }

        [HttpGet("{_variantId}", Name = "SingleVariant")]
        public async Task<IActionResult> GetSingleVariant(CancellationToken ctx, string _variantId) { 
           var result =  await _variantRepo.GetSingleVariant(Guid.Parse(_variantId), ctx);
            return Ok(result);
        }


        [HttpDelete("{_variantId}")]
        public async Task<IActionResult> DeleteSingleVariant(CancellationToken ctx, string _variantId)
        {
            await _variantRepo.DeleteSingleVariant(Guid.Parse(_variantId), ctx);
            return Ok();
        }

        [HttpDelete("{_productId}")]
        public async Task<IActionResult> DeleteProductVariant(CancellationToken ctx, string _productId)
        {
            await _variantRepo.DeleteAllVariantsForProduct(Guid.Parse(_productId), ctx);
            return Ok();
        }

    [HttpPost]
        public async Task<IActionResult> CreateVariant([FromQuery] string productId,[FromForm] DTOVariant variantDTO,IFormFile variantImage,CancellationToken cancellationToken)
        {
            if (variantImage == null || variantImage.Length == 0)
                return BadRequest("No image uploaded");

            await using var memoryStream = new MemoryStream();
            Console.WriteLine($"Uploading Before {memoryStream.Position}%");
            await variantImage.CopyToAsync(memoryStream, cancellationToken);
            Console.WriteLine($"Uploading After {memoryStream.Position}%");

            byte[] imageBytes = memoryStream.ToArray();

            var imageId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var imageModel = new ImageModel(imageId, imageBytes)
            {
                BytesSize = imageBytes.Length
            };

            var variantModel = new VariantModel(
                variantId,
                variantDTO._Size,
                variantDTO._Price,
                Guid.Parse(productId),
                variantDTO._Units,
                imageId);

            await _variantRepo.CreateVariantForProduct(Guid.Parse(productId), variantModel, cancellationToken);

            return CreatedAtRoute("SingleVariant", new { _variantId = variantId }, variantModel);
        }

    }
}
