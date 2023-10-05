using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListUser.Data;

public class AppDbContext : DbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Role> Roles { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
	{
		//Database.EnsureCreated();
	}
}
