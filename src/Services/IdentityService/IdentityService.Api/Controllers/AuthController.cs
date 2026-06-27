using System.Threading.Tasks;
using IdentityService.Application.Auth.Commands.Register;
using IdentityService.Application.Auth.Queries.Login;
using IdentityService.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AuthResult))]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AuthResult))]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        var result = await Mediator.Send(query);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
