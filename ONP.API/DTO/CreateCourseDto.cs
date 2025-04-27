namespace ONP.API.DTO
{
	public class CreateCourseDto
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public int CategoryId { get; set; }

		public decimal Price { get; set; }

		public IFormFile Image { get; set; } // صورة الدورة
	}

}
