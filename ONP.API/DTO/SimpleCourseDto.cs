namespace ONP.API.DTO
{
	public class SimpleCourseDto
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string InstructorName { get; set; }
		public string CategoryName { get; set; }

		public string imageUrl { get; set; }
		public decimal Price { get; set; }
	}

}
