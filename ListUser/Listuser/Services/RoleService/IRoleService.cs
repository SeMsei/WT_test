using Domain.Entities;
using Domain.Models;

namespace ListUser.Services.RoleService;

public interface IRoleService
{
	public Task AddUser(int id, int userId);
	public Task<bool> DoesRoleExistAsync(int id);
}
