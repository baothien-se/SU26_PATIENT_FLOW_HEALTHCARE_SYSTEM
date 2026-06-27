using System.Threading.Tasks;
using IdentityService.Application.Common.Models;

namespace IdentityService.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(string userName, string email, string password);
    Task<AuthResult> LoginAsync(string email, string password);
}
