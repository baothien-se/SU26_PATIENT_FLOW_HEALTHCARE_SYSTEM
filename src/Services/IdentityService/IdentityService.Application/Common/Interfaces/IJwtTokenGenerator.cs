using System.Collections.Generic;

namespace IdentityService.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);
}
