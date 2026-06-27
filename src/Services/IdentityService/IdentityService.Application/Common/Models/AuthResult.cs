using System.Collections.Generic;

namespace IdentityService.Application.Common.Models;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string Token { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = [];

    public static AuthResult Success(string token) => new() { Succeeded = true, Token = token };
    public static AuthResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}
