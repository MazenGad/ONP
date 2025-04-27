namespace ONP.API.DTO
{
	public class CreateSnippetDto
	{
		public string Title { get; set; }
		public string? Description { get; set; }
		public string Language { get; set; }
		public string Code { get; set; }
	}

}
