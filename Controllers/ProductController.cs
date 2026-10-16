using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> Products = new() {
        new() { Id = 1, Name = "Laptop", Price = 799.99m },
        new() { Id = 2, Name = "Wireless Mouse", Price = 24.50m },
        new() { Id = 3, Name = "Keyboard", Price = 49.90m },
        new() { Id = 4, Name = "Monitor", Price = 219.00m }
    };

    // This endpoint is public with respect to JWT, but requires the API Key middleware.
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<Product>> GetProducts() => Ok(Products);

    // Requires both a valid API Key and JWT.
    [HttpGet("secure")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetSecureProducts() => Ok(new {
        message = "Access granted to protected product resource.",
        user = User.Identity?.Name,
        role = User.FindFirst(ClaimTypes.Role)?.Value,
        claims = User.Claims.Select(c => new { c.Type, c.Value }),
        products = Products
    });
}
