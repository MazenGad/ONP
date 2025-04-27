namespace ONP.API.DTO
{
	public class CreateCourseContentDto
	{
		public string Title { get; set; }
		public string ContentType { get; set; } // "Video" or "Text"
		public string? VideoUrl { get; set; }
		public string? TextContent { get; set; }
		public int Order { get; set; }
		public List<LessonCodesDto>? LessonCodes { get; set; }

	}

}
