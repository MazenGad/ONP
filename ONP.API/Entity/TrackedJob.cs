namespace ONP.API.Entity
{
	public class TrackedJob
	{
		public int Id { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int JobId { get; set; }
		public Job Job { get; set; }

		public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
	}

}
