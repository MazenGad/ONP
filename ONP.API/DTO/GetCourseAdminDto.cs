namespace ONP.API.DTO
{
	public class GetCourseAdminDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public Decimal Price { get; set; }
		public string CategoryName { get; set; }
		public string InstructorName { get; set; }
	}
}
