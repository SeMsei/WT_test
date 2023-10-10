using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListUser.Data;

public class DbInitializer
{
	public static async Task SeedData(WebApplication app)
	{
		var _roles = new List<Role>()
		{
			new Role {Id = 1, Name = "User"},
			new Role {Id = 2, Name = "Admin"},
			new Role {Id = 3, Name = "Support"},
			new Role {Id = 4, Name = "SuperAdmin"}
		};

		using var scope = app.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		if (context.Database.GetPendingMigrations().Any())
		{
			await context.Database.MigrateAsync();
		}

		if (!context.Roles.Any())
		{
			await context.Roles.AddRangeAsync(_roles!);
			await context.SaveChangesAsync();
		}
	}
}
