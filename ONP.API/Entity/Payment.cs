namespace ONP.API.Entity
{
	public class Payment
	{
		public int Id { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int CourseId { get; set; }
		public Course Course { get; set; }

		public decimal AmountPaid { get; set; }
		public decimal InstructorShare { get; set; }
		public decimal AdminShare { get; set; }

		public DateTime PaidAt { get; set; }
	}

}
