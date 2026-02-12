using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.DTOs;
using Pos.Backend.Api.Core.Entities;
using Pos.Backend.Api.Core.Security;
using Pos.Backend.Api.Infrastructure.Data;

namespace Pos.Backend.Api.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly PosDbContext _context;

    public ProductsController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.CatalogProductsRead)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get()
    {
        var products = await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Price = p.Price,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.CatalogProductsWrite)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Name))
        {
            return BadRequest(new { error = "NAME_REQUIRED" });
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { error = "CATEGORY_NOT_FOUND" });
        }

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name.Trim(),
            Price = dto.Price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Price = product.Price,
            IsActive = product.IsActive
        };

        return CreatedAtAction(nameof(Get), new { id = product.Id }, response);
    }
}
