namespace ONP.API.DTO
{
	public class StudentDashboardDto
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string? ProfileImageUrl { get; set; }

		public List<StudentCourseProgressDto> Courses { get; set; }
	}
}
