namespace ONP.API.DTO
{
	public class UpdateProfileDto
	{
		public string FullName { get; set; }
		public string? CurrentPassword { get; set; }
		public string? NewPassword { get; set; }
	}

}
