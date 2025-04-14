namespace ONP.API.Entity
{
	public class Course
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }

		public int CategoryId { get; set; }
		public Category Category { get; set; }

		public string InstructorId { get; set; }
		public ApplicationUser Instructor { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ICollection<CourseContent> Contents { get; set; }
	}

}
