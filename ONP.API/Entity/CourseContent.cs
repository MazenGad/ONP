namespace ONP.API.Entity
{
	public class CourseContent
	{
		public int Id { get; set; }
		public int CourseId { get; set; }
		public Course Course { get; set; }

		public string Title { get; set; }
		public string ContentType { get; set; } // "Video" or "Text"

		public string? VideoUrl { get; set; }
		public string? TextContent { get; set; }

		public int Order { get; set; } // ترتيب الظهور في الكورس
	}

}
