namespace ONP.API.DTO
{

	public class InstructorCourseSummaryDto
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public int StudentCount { get; set; }
		public double AverageRating { get; set; }
	}
}
