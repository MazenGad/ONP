namespace ONP.API.DTO
{
	public class CourseContentDto
	{
		public string Title { get; set; }
		public string ContentType { get; set; }
		public string? VideoUrl { get; set; }
		public string? TextContent { get; set; }
		public int Order { get; set; }
	}
}
