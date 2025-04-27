namespace ONP.API.Entity
{
	public class LessonCode
	{
		public int Id { get; set; }
		public int CourseContentId { get; set; }
		public string Language { get; set; } // "html", "css", "js", etc.
		public string Code { get; set; }

		public CourseContent CourseContent { get; set; }
	}
}
