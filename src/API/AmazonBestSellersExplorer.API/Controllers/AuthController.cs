using AmazonBestSellersExplorer.API.Common;
using AmazonBestSellersExplorer.API.Contracts.Auth;
using AmazonBestSellersExplorer.API.Extensions;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;
using AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
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

        if (result.TryGetValue(out var response))
        {
            return Ok(response);
        }

        return this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.ValidationFailed);
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

        if (result.TryGetValue(out var response))
        {
            return Ok(response);
        }

        if (result.ErrorCode is AuthErrorCode.InvalidCredentials)
        {
            return this.ToProblem(result, StatusCodes.Status401Unauthorized, ApiProblemTitles.AuthenticationFailed);
        }

        return this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.ValidationFailed);
    }
}
