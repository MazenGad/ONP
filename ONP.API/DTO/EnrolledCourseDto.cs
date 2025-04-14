namespace ONP.API.DTO
{
	public class EnrolledCourseDto
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string Category { get; set; }
		public string Instructor { get; set; }
		public DateTime EnrolledAt { get; set; }
	}

}
