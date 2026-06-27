using System.Threading;
using System.Threading.Tasks;
using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Auth.Queries.Login;

public record LoginQuery : IRequest<AuthResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResult>
{
    private readonly IIdentityService _identityService;

    public LoginQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password);
    }
}
