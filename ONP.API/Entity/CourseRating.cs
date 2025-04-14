namespace ONP.API.Entity
{
	public class CourseRating
	{
		public int Id { get; set; }

		public int CourseId { get; set; }
		public Course Course { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int RatingValue { get; set; } // بين 1 و 5

		public DateTime RatedAt { get; set; } = DateTime.UtcNow;
	}

}
