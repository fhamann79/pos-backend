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
public class CategoriesController : ControllerBase
{
    private readonly PosDbContext _context;

    public CategoriesController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.CatalogCategoriesRead)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.CatalogCategoriesWrite)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Name))
        {
            return BadRequest(new { error = "NAME_REQUIRED" });
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return CreatedAtAction(nameof(Get), new { id = category.Id }, response);
    }
}
