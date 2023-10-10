using ListUser.Data;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ListUser.Services.RoleService;

public class RoleService: IRoleService
{
	private AppDbContext _context;
	public RoleService(AppDbContext context)
	{
		_context = context;
	}

	public async Task AddUser(int id, int userId)
	{
		var role = await _context.Roles.FindAsync(id);
		var user = await _context.Users.FindAsync(userId);
		role.Users.Add(user);
		//role.Users.Append(user);

		await _context.SaveChangesAsync();
	}

	public async Task<bool> DoesRoleExistAsync(int id)
	{
		return await _context.Roles.AnyAsync(u => u.Id == id);
	}
}
