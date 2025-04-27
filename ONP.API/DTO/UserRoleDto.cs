namespace ONP.API.DTO
{
	public class UserRoleDto
	{
		public string Id { get; set; }

		public string UserName { get; set; } = string.Empty;

		public string FullName { get; set; } = string.Empty;

		public ICollection<string> UserRoles { get; set; } = new List<string>();

	}
}
