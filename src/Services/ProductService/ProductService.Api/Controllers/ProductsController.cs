using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Products.Commands.CreateProduct;
using ProductService.Application.Products.Commands.DeleteProduct;
using ProductService.Application.Products.Commands.UpdateProduct;
using ProductService.Application.Products.Dtos;
using ProductService.Application.Products.Queries.GetProductById;
using ProductService.Application.Products.Queries.GetProducts;

namespace ProductService.Api.Controllers;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get()
    {
        var products = await Mediator.Send(new GetProductsQuery());
        return Ok(products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await Mediator.Send(new GetProductByIdQuery(id));
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] CreateProductCommand command)
    {
        var productId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = productId }, productId);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "ID mismatch between route and body." });
        }

        var result = await Mediator.Send(command);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id));
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
