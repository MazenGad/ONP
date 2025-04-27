using ONP.API.Entity;

namespace ONP.API.DTO
{
	public class DashboardOverview
	{
		public int TotalCourses { get; set; }
		public int TotalStudents { get; set; }
		public int TotalInstructors { get; set; }
		public int TotalJobs { get; set; }
		public decimal TotalRevenue { get; set; }
		public List<CourseResponseDto> RecentCourses { get; set; } = new List<CourseResponseDto>();
		public List<Job> RecentJobs { get; set; } = new List<Job>();
	}
}
