namespace ONP.API.DTO
{
	public class CourseResponseDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string CategoryName { get; set; }
		public string InstructorName { get; set; }
		public DateTime CreatedAt { get; set; }

		public int EnrolledStudents { get; set; }
		public double AverageRating { get; set; }
	}


}
