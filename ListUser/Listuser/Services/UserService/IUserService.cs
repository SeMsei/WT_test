using Domain.Entities;
using Domain.Models;
using ListUser.Services.Util;

namespace ListUser.Services.UserService;

public interface IUserService
{
	public Task<ResponseData<ListModel<User>>> GetUserListAsync(
								   int? pageNo = null,
								   int? pageSize = null,
								   string? filterBy = null, string? filterValue = null,
								   string? sortBy = null, string? sortOrder = null);
	public Task<ResponseData<User>> GetUserByIdAsync(int id);
	public Task<ResponseData<User>> UpdateUserAsync(int id, User user);
	public Task DeleteUserAsync(int id);
	public Task<ResponseData<User>> CreateUserAsync(User user);
	public Task<ResponseData<User>> AddUserRoleAsync(int id, Role role);
	public Task<bool> IsEmailUnique(string email);
	public Task<bool> DoesUserExistAsync(int id);
	public Task<bool> DoesUserExistAsync(string email, string password);
}
