namespace ONP.API.DTO
{
	public class CourseProgressDto
	{
		public int CourseId { get; set; }
		public string CourseTitle { get; set; }

		public int TotalContents { get; set; }
		public int CompletedContents { get; set; }

		public double ProgressPercentage { get; set; }
	}

}
