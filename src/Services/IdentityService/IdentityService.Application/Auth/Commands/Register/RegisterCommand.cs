using System.Threading;
using System.Threading.Tasks;
using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Auth.Commands.Register;

public record RegisterCommand : IRequest<AuthResult>
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(request.UserName, request.Email, request.Password);
    }
}
