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

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>()
				.HasMany(c => c.Roles)
				.WithMany(s => s.Users);
	}
}
