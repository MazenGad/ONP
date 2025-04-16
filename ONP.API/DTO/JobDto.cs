namespace ONP.API.DTO
{
	public class JobDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public string? ContactEmail { get; set; }
		public DateTime PostedAt { get; set; }
	}

}
