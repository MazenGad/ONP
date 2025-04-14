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
	public class InstructorController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public InstructorController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet("dashboard")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> GetDashboard()
		{
			var user = await _userManager.GetUserAsync(User);

			var courses = await _context.Courses
				.Where(c => c.InstructorId == user.Id)
				.ToListAsync();

			var courseDtos = new List<InstructorCourseSummaryDto>();

			foreach (var course in courses)
			{
				var studentCount = await _context.Enrollments
					.CountAsync(e => e.CourseId == course.Id);

				var ratings = await _context.CourseRatings
					.Where(r => r.CourseId == course.Id)
					.Select(r => r.RatingValue)
					.ToListAsync();

				var avgRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 0;

				courseDtos.Add(new InstructorCourseSummaryDto
				{
					CourseId = course.Id,
					Title = course.Title,
					StudentCount = studentCount,
					AverageRating = avgRating
				});
			}

			var result = new InstructorDashboardDto
			{
				FullName = user.FullName,
				Email = user.Email,
				Courses = courseDtos
			};

			return Ok(result);
		}

		[HttpPut("profile")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
		{
			var user = await _userManager.GetUserAsync(User);

			user.FullName = dto.FullName;

			IdentityResult passwordResult = IdentityResult.Success;

			if (!string.IsNullOrWhiteSpace(dto.NewPassword))
			{
				if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
					return BadRequest("Current password is required to change password.");

				passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

				if (!passwordResult.Succeeded)
					return BadRequest(passwordResult.Errors);
			}

			var updateResult = await _userManager.UpdateAsync(user);

			if (!updateResult.Succeeded)
				return BadRequest(updateResult.Errors);

			return Ok("Profile updated successfully");
		}

		[HttpGet("ratings/{courseId}")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> GetCourseRatings(int courseId)
		{
			var user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == user.Id);

			if (course == null)
				return Forbid("You are not the owner of this course.");

			var ratings = await _context.CourseRatings
				.Where(r => r.CourseId == courseId)
				.Include(r => r.Student)
				.Select(r => new
				{
					StudentName = r.Student.FullName,
					RatingValue = r.RatingValue,
					RatedAt = r.RatedAt
				})
				.ToListAsync();

			return Ok(ratings);
		}

		[HttpPost("upload-profile-image")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> UploadProfileImage(IFormFile file, [FromServices] CloudinaryService cloudService)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			var user = await _userManager.GetUserAsync(User);

			string imageUrl;
			try
			{
				imageUrl = await cloudService.UploadImageAsync(file);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Image upload failed: {ex.Message}");
			}

			user.ProfileImageUrl = imageUrl;
			await _userManager.UpdateAsync(user);

			return Ok(new { imageUrl });
		}




	}

}
