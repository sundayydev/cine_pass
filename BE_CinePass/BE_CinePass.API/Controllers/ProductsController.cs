using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Product;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả sản phẩm
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResult(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Lỗi khi lấy danh sách sản phẩm"));
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đang hoạt động
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponseDto>>> GetActive(CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync(cancellationToken);
            return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResult(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active products");
            return StatusCode(500, ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Lỗi khi lấy danh sách sản phẩm"));
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo danh mục
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponseDto>>> GetByCategory(ProductCategory category, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productService.GetByCategoryAsync(category, cancellationToken);
            return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResult(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category {Category}", category);
            return StatusCode(500, ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Lỗi khi lấy danh sách sản phẩm"));
        }
    }

    /// <summary>
    /// Tìm kiếm sản phẩm
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponseDto>>> Search([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));

            var products = await _productService.SearchAsync(searchTerm, cancellationToken);
            return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResult(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Lỗi khi tìm kiếm sản phẩm"));
        }
    }

    /// <summary>
    /// Lấy thông tin sản phẩm theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return NotFound(ApiResponseDto<ProductResponseDto>.ErrorResult($"Không tìm thấy sản phẩm có ID {id}"));

            return Ok(ApiResponseDto<ProductResponseDto>.SuccessResult(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by id {ProductId}", id);
            return StatusCode(500, ApiResponseDto<ProductResponseDto>.ErrorResult("Lỗi khi lấy thông tin sản phẩm"));
        }
    }

    /// <summary>
    /// Tạo sản phẩm mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ProductResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var product = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponseDto<ProductResponseDto>.SuccessResult(product, "Tạo sản phẩm thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, ApiResponseDto<ProductResponseDto>.ErrorResult("Lỗi khi tạo sản phẩm"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin sản phẩm
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, [FromBody] ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ProductResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var product = await _productService.UpdateAsync(id, dto, cancellationToken);
            if (product == null)
                return NotFound(ApiResponseDto<ProductResponseDto>.ErrorResult($"Không tìm thấy sản phẩm có ID {id}"));

            return Ok(ApiResponseDto<ProductResponseDto>.SuccessResult(product, "Cập nhật sản phẩm thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, ApiResponseDto<ProductResponseDto>.ErrorResult("Lỗi khi cập nhật sản phẩm"));
        }
    }

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _productService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy sản phẩm có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa sản phẩm"));
        }
    }
}

