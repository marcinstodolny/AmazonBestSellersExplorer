using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;
using AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    private const string InvalidCredentialsError = "Invalid username or password.";

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Username, request.Password),
            cancellationToken);

        return ToRegisterActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginUserCommand(request.Username, request.Password),
            cancellationToken);

        return ToLoginActionResult(result);
    }

    private IActionResult ToRegisterActionResult(Result<AuthResponse> result)
    {
        if (result.TryGetValue(out var response))
        {
            return Ok(response);
        }

        return BadRequest(new { errors = result.Errors });
    }

    private IActionResult ToLoginActionResult(Result<AuthResponse> result)
    {
        if (result.TryGetValue(out var response))
        {
            return Ok(response);
        }

        if (result.Errors.Count == 1
            && string.Equals(result.FirstError, InvalidCredentialsError, StringComparison.Ordinal))
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return BadRequest(new { errors = result.Errors });
    }
}
