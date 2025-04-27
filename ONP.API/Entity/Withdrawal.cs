namespace ONP.API.Entity
{
	public class Withdrawal
	{
		public int Id { get; set; }
		public string InstructorId { get; set; }
		public ApplicationUser Instructor { get; set; }
		public decimal Amount { get; set; }
		public DateTime WithdrawnAt { get; set; }
		public string PayPalEmail { get; set; }
	}

}
