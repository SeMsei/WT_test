using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using ListUser.Data;
using ListUser.Services.UserService;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Domain.Models;
using ListUser.Services.RoleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace ListUser.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
		private readonly IRoleService _roleService;
		private readonly ILogger<UserController> _logger;

        public UserController(IUserService service, IRoleService roleService, ILogger<UserController> logger)
        {
			_service = service;
			_roleService = roleService;
			_logger = logger;
			_logger.LogInformation("User Controller controller called ");
		}
		// GET: api/User
		[HttpGet]
		[SwaggerOperation(
			Summary = "Get list of users",
			Description = "The method allows to get list of users with filtration, sorting and pagination.",
			OperationId = "GetUsers"
		)]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns a json with users list.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Incorrect pagination, filtration or sorting option.")]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers(int? pageNo = null, int? pageSize = null, 
															string? filterBy = null, string? filterValue = null,
															string? sortBy = null, string? sortOrder = null)
        {
			_logger.LogInformation("Method 'GetUsers' called with pageNo: {pageNo}, pageSize: {pageSize}," +
				"filterBy: {filterBy}, filterValue: {filterValue}, sortBy: {sortBy}, sortOrder: {sortOrder}",
				pageNo, pageSize, filterBy, filterValue, sortBy, sortOrder);
			var resp = await _service.GetUserListAsync(pageNo, pageSize, filterBy, filterValue, sortBy, sortOrder);

			if (resp.Success)
			{
				_logger.LogInformation("Method 'GetUsers' completed successfuly, return page: {CurrentPage}, total page: {TotalPages}", resp.Data.CurrentPage, resp.Data.TotalPages);
				return Ok(resp);
			} 
			else
			{
				_logger.LogWarning("Method 'GetUsers' failed with error message: {ErrorMessage}", resp.ErrorMessage);
				return BadRequest(resp.ErrorMessage);
			}
		}
		// GET: api/User/5
		[HttpGet("{id}")]
		[SwaggerOperation(
			Summary = "Get specified user by its id",
			Description = "The method allows to get specified user by its id.",
			OperationId = "GetUser"
		)]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns a json with user information.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Incorrect user id.")]
		public async Task<ActionResult<User>> GetUser([SwaggerParameter("The unique Id of the user.", Required = true)] int id)
        {
			_logger.LogInformation("Method 'GetUser' called with user id: {id}", id);
			var resp = await _service.GetUserByIdAsync(id);

			if (resp.Success)
			{
				_logger.LogInformation("Method 'GetUser' completed successfuly, user: {@user}", resp.Data);
				return Ok(await _service.GetUserByIdAsync(id));
			}	
			else
			{
				_logger.LogWarning("Method 'GetUser' failed with error message: {ErrorMessage}", resp.ErrorMessage);
				return BadRequest(resp.ErrorMessage);
			}
				
		}
		
		//PUT: api/User/add-role/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("add-role/{id}")]
		[SwaggerOperation(
			Summary = "Adding a role to a user",
			Description = "The method allows to add a role to a user by his Id.",
			OperationId = "AddRoleToUser"
		)]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns a json with user and role information.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User with specified Id not found.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Role with specified Id not found.")]
		public async Task<IActionResult> AddRoleUser([SwaggerParameter("The unique Id of the user.", Required = true)] int id,
													 [SwaggerParameter("The role to add to the user.", Required = true)] [FromBody] Role role)
		{
			_logger.LogInformation("Method 'AddRoleUser' called with user id: {id}, role: {@role}", id, role);
			if (!(await _roleService.DoesRoleExistAsync(role.Id)))
			{
				_logger.LogWarning("Method 'AddRoleUser' failed with error message: {ErrorMessage}", "There's no such role");
				return BadRequest("There's no such role");
			}

			User user;
			var res = await _service.AddUserRoleAsync(id, role);

			if (!res.Success)
			{
				_logger.LogWarning("Method 'AddRoleUser' failed with error message: {ErrorMessage}", res.ErrorMessage);
				return NotFound(res.ErrorMessage);
			}

			user = res.Data;

			_logger.LogInformation("Method 'AddRoleUser' completed successfuly");
			return Ok(new ResponseData<User>()
			{
				Data = user,
			});
		}
		
		// PUT: api/User/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("{id}")]
		[SwaggerOperation(
			Summary = "Update User information",
			Description = "The method allows to update user information by its id.",
			OperationId = "UpdateUser"
		)]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns OK message.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Given user is null.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "User with specified Id not found.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Role with specified Id not found.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Incorrect user information.")]
		public async Task<IActionResult> PutUser([SwaggerParameter("The unique Id of the user.", Required = true)] int id,
												 [SwaggerParameter("New user information.", Required = true)] [FromBody] User user)
        {
			_logger.LogInformation("Method 'PutUser' called with id : {id}", id);
			if (user == null)
			{
				_logger.LogWarning("Method 'PutUser' failed with Error message: {ErrorMessage}", "User can't be null");
				return BadRequest("User can't be null");
			}

			if (!(await _service.DoesUserExistAsync(id)))
			{
				_logger.LogWarning("Method 'PutUser' failed with Error message: {ErrorMessage}", "There's no such user");
				return BadRequest("There's no such user");
			}

			foreach (var role in user.Roles)
			{
				if (!(await _roleService.DoesRoleExistAsync(role.Id)))
				{
					_logger.LogWarning("Method 'PutUser' failed with Error message: {ErrorMessage}", "There's no such role: " + role.Id);
					return BadRequest("There's no such role: " + role.Id);
				}
			}

			var resp = await _service.UpdateUserAsync(id, user);

			if (!resp.Success)
			{
				_logger.LogWarning("Method 'PutUser' failed with Error message: {ErrorMessage}", resp.ErrorMessage);
				return BadRequest(resp.ErrorMessage);
			}

			_logger.LogInformation("Method 'PutUser' completed successfully with updated user: {@user}", resp.Data);
			return Ok($"User with id = {resp.Data.Id} successfully updated");
		}
		
		// POST: api/User
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create new User",
			Description = "The method allows to create new user.",
			OperationId = "CreateUser"
		)]
		[SwaggerResponse(StatusCodes.Status201Created, "Returns json with user information.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Given user is null.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "User with specified Id already exist.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Incorrect user roles.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Incorrect user information.")]
		public async Task<ActionResult<User>> PostUser([SwaggerParameter("New user information.", Required = true)] [FromBody] User user)
        {
			_logger.LogInformation("Method 'PostUser' called");
			if (user == null)
			{
				_logger.LogWarning("Method 'PostUser' failed with Error message: {ErrorMessage}", "User is null");
				return BadRequest(new ResponseData<User>()
				{
					Data = null,
					Success = false,
					ErrorMessage = "User is null"
				});
			}

			if ((await _service.DoesUserExistAsync(user.Id)))
			{
				_logger.LogWarning("Method 'PostUser' failed with Error message: {ErrorMessage}", "There's already user with such id: " + user.Id);
				return BadRequest(new ResponseData<User>()
				{
					Data = null,
					Success = false,
					ErrorMessage = "There's already user with such id: " + user.Id
				});
			}

			foreach (var role in user.Roles)
			{
				if (!(await _roleService.DoesRoleExistAsync(role.Id)))
				{
					_logger.LogWarning("Method 'PostUser' failed with Error message: {ErrorMessage}", "There's no such role: " + role.Id);
					return BadRequest(new ResponseData<User>()
					{
						Data = null,
						Success = false,
						ErrorMessage = "There's no such role: " + role.Id
					});
				}
			}

			var response = await _service.CreateUserAsync(user);

			if (!response.Success)
			{
				_logger.LogWarning("Method 'PostUser' failed with Error message: {ErrorMessage}", response.ErrorMessage);
				return BadRequest(response.ErrorMessage);
			}
			
			foreach (var tmp in user.Roles)
			{
				await _roleService.AddUser(tmp.Id, user.Id);
			}

			var ret = CreatedAtAction("GetUser", new { id = user.Id }, new ResponseData<User>()
			{
				Data = user
			});

			_logger.LogInformation("Method 'PostUser' completed successfully with created user: {@user}", user);
			return ret;
		}
		
		// DELETE: api/User/5
		[HttpDelete("{id}")]
		[SwaggerOperation(
			Summary = "Delete User",
			Description = "The method allows to delete user.",
			OperationId = "DeleteUser"
		)]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns OK message.")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "User with specified Id doesn't exist.")]
		public async Task<IActionResult> DeleteUser([SwaggerParameter("The unique Id of the user.", Required = true)] int id)
        {
			_logger.LogInformation("Method 'DeletetUser' called with id: {id}", id);
			if (!(await _service.DoesUserExistAsync(id)))
			{
				_logger.LogWarning("Method 'PostUser' failed with Error message: {ErrorMessage}", "There no such user: " + id);
				return NotFound("There no such user: " + id);
			}

			await _service.DeleteUserAsync(id);

			_logger.LogInformation("Method 'PostUser' completed successfully");
			return Ok("Пользователь с id = " + id + " успешно удален");
		}

        private async Task<bool> UserExists(int id)
        {
			var resp = await _service.GetUserListAsync();
			return resp.Data.Items.Any(x => x.Id == id);
			//return (_context.VideoGame?.Any(e => e.Id == id)).GetValueOrDefault();
		}

		
	}
}
