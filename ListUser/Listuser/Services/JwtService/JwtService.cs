using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ListUser.Services.JwtService;
public class JwtService: IJwtService
{
	private readonly string _secretKey;
	private readonly string _issuer;

	public JwtService(string secretKey, string issuer)
	{
		_secretKey = secretKey;
		_issuer = issuer;
	}

	public string GenerateToken(string userId)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_secretKey);

		var claims = new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.Name, userId),
		});

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = claims,
			Expires = DateTime.UtcNow.AddHours(1), // Устанавливает срок действия токена
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature
			),
			Issuer = _issuer,
			Audience = _issuer
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}

	public ClaimsPrincipal ValidateToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_secretKey);

		var validationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _issuer,
			ValidAudience = _issuer,
			IssuerSigningKey = new SymmetricSecurityKey(key),
		};

		try
		{
			var principal = tokenHandler.ValidateToken(token, validationParameters, out var _);
			return principal;
		}
		catch (Exception)
		{
			// Токен недействителен или истек
			return null;
		}
	}
}