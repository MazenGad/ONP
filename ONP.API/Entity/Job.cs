namespace ONP.API.Entity
{
	public class Job
	{
		public int Id { get; set; }
		public string Title { get; set; }        // ex: "Opportunity in France"
		public string Content { get; set; }      // الإعلان النصي
		public string? ContactEmail { get; set; }
		public DateTime PostedAt { get; set; } = DateTime.UtcNow;

		public ICollection<TrackedJob> TrackedBy { get; set; }
	}

}
