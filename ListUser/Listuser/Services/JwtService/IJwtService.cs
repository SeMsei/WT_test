using System.Security.Claims;

namespace ListUser.Services.JwtService;

public interface IJwtService
{
	public string GenerateToken(string userId);
	public ClaimsPrincipal ValidateToken(string token);

}
