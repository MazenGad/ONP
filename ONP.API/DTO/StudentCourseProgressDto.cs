namespace ONP.API.DTO
{

	public class StudentCourseProgressDto
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string InstructorName { get; set; }
		public string CategoryName { get; set; }

		public double ProgressPercentage { get; set; }
		public int? MyRating { get; set; } // ممكن يكون null لو ما قيّمش
	}
}
