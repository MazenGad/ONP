using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ONP.API.Data;
using ONP.API.DTO;
using ONP.API.Entity;

namespace ONP.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CourseRatingController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CourseRatingController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost("{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> RateCourse(int courseId, CourseRatingDto dto)
		{
			if (dto.RatingValue < 1 || dto.RatingValue > 5)
				return BadRequest("Rating must be between 1 and 5.");

			var user = await _userManager.GetUserAsync(User);

			var isEnrolled = await _context.Enrollments
				.AnyAsync(e => e.CourseId == courseId && e.StudentId == user.Id);

			if (!isEnrolled)
				return Unauthorized("You must be enrolled in the course to rate it.");

			var existingRating = await _context.CourseRatings
				.FirstOrDefaultAsync(r => r.CourseId == courseId && r.StudentId == user.Id);

			if (existingRating != null)
			{
				existingRating.RatingValue = dto.RatingValue;
				existingRating.RatedAt = DateTime.UtcNow;
			}
			else
			{
				var rating = new CourseRating
				{
					CourseId = courseId,
					StudentId = user.Id,
					RatingValue = dto.RatingValue
				};
				_context.CourseRatings.Add(rating);
			}

			await _context.SaveChangesAsync();
			return Ok("Rating saved.");
		}

		[HttpGet("{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetMyRating(int courseId)
		{
			var user = await _userManager.GetUserAsync(User);

			var rating = await _context.CourseRatings
				.Where(r => r.CourseId == courseId && r.StudentId == user.Id)
				.Select(r => r.RatingValue)
				.FirstOrDefaultAsync();

			return Ok(rating);
		}
	}

}
