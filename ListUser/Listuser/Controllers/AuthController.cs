using Domain.Entities;
using ListUser.Controllers;
using ListUser.Services.JwtService;
using ListUser.Services.UserService;
using ListUser.Services.Util;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IJwtService _jwtService;
	private readonly IUserService _userService;
	private readonly ILogger<UserController> _logger;

	public AuthController(IJwtService jwtService, IUserService userService, ILogger<UserController> logger)
	{
		_jwtService = jwtService;
		_userService = userService;
		_logger = logger;
		_logger.LogInformation("Auth Controller controller called ");
	}

	[HttpPost("login")]
	[SwaggerOperation(
			Summary = "Login User",
			Description = "The method allows to login.",
			OperationId = "Login"
		)]
	[SwaggerResponse(StatusCodes.Status200OK, "Returns OK message with jwt token.")]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Given login or password is incorrect.")]
	public async Task<IActionResult> Login([SwaggerParameter("The email of the user.", Required = true)] string email,
										   [SwaggerParameter("The password of the user.", Required = true)] string password)
	{
		_logger.LogInformation("Method 'Login' called");
		password = HashCoder.HashPassword(password);
		var us = await _userService.DoesUserExistAsync(email, password);

		if (!us)
		{
			_logger.LogWarning("Method 'Login' failed with error message: {ErrorMessage}", "Incorrect login or password");
			return BadRequest("Incorrect login or password");
		}
		else
		{
			var token = _jwtService.GenerateToken(email+password);
			_logger.LogInformation("Method 'Login' completed successfully");
			return Ok(new { Token = token });
		}
		
	}

	[HttpPost("register")]
	[SwaggerOperation(
			Summary = "Register User",
			Description = "The method allows to register new User.",
			OperationId = "Register"
		)]
	[SwaggerResponse(StatusCodes.Status201Created, "Returns OK message with jwt token.")]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Given information of user is incorrect.")]
	public async Task<IActionResult> Register([SwaggerParameter("The unique Id of the user.", Required = true)] [FromBody] User user)
	{
		_logger.LogInformation("Method 'Register' called");
		user.Password = HashCoder.HashPassword(user.Password);
		var resp = await _userService.CreateUserAsync(user);

		if (resp.Success)
		{
			_logger.LogInformation("Method 'Register' completed successfully with user {@user}", resp.Data);
			var token = _jwtService.GenerateToken(user.Email + user.Password);
			return Ok(new { Token = token });
		}
		else
		{
			_logger.LogWarning("Method 'Login' failed with error message: {ErrorMessage}", resp.ErrorMessage);
			return BadRequest(resp.ErrorMessage);
		}
	}
}