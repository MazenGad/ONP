namespace ONP.API.Entity
{
	public class FavoriteCourse
	{
		public int Id { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int CourseId { get; set; }
		public Course Course { get; set; }

		public DateTime AddedAt { get; set; } = DateTime.UtcNow;
	}

}
