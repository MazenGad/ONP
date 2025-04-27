namespace ONP.API.DTO
{
	public class InstructorDashboardDto
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string? ProfileImageUrl { get; set; }
		public decimal TotalEarnings { get; set; } 

		public List<InstructorCourseSummaryDto> Courses { get; set; }
	}
}
