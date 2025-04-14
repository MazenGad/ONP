using Microsoft.AspNetCore.Identity;
using ONP.API.Entity;

public class DbSeeder
{
	public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
	{
		string[] roles = { "Admin", "Instructor", "Student" };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}

		// Create default Admin
		string adminEmail = "admin@onp.com";
		string adminPassword = "Admin@123";

		if (await userManager.FindByEmailAsync(adminEmail) == null)
		{
			var admin = new ApplicationUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				FullName = "System Admin"
			};

			var result = await userManager.CreateAsync(admin, adminPassword);
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(admin, "Admin");
			}
		}
	}
}
