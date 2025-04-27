using Microsoft.AspNetCore.Identity;

namespace ONP.API.Entity
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; }
		public string? ProfileImageUrl { get; set; }
		public decimal TotalEarnings { get; set; } = 0;

	}
}
