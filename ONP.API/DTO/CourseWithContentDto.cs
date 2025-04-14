namespace ONP.API.DTO
{
	public class CourseWithContentDto
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string CategoryName { get; set; }
		public string InstructorName { get; set; }
		public List<CourseContentDto> Contents { get; set; }
	}
}
