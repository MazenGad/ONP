namespace ONP.API.Entity
{
	public class CourseProgress
	{
		public int Id { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int CourseId { get; set; }
		public Course Course { get; set; }

		public int ContentId { get; set; }
		public CourseContent Content { get; set; }

		public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
	}

}
