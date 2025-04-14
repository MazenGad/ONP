namespace ONP.API.Entity
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public ICollection<Course> Courses { get; set; }
	}

}
