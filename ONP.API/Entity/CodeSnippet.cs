namespace ONP.API.Entity
{
	public class CodeSnippet
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public string Language { get; set; }
		public string Code { get; set; }

		public string AuthorId { get; set; }
		public ApplicationUser Author { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

}
