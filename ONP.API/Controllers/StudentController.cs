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
	public class StudentController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public StudentController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet("dashboard")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetDashboard()
		{
			var user = await _userManager.GetUserAsync(User);

			var enrolledCourses = await _context.Enrollments
				.Where(e => e.StudentId == user.Id)
				.Include(e => e.Course)
					.ThenInclude(c => c.Category)
				.Include(e => e.Course)
					.ThenInclude(c => c.Instructor)
				.ToListAsync();

			var courseDtos = new List<StudentCourseProgressDto>();

			foreach (var enrollment in enrolledCourses)
			{
				var course = enrollment.Course;

				// Total contents
				var totalContents = await _context.CourseContents
					.CountAsync(c => c.CourseId == course.Id);

				// Completed contents by this student
				var completedContents = await _context.CourseProgress
					.CountAsync(p => p.CourseId == course.Id && p.StudentId == user.Id);

				// Student rating (nullable)
				var rating = await _context.CourseRatings
					.Where(r => r.CourseId == course.Id && r.StudentId == user.Id)
					.Select(r => (int?)r.RatingValue)
					.FirstOrDefaultAsync();

				courseDtos.Add(new StudentCourseProgressDto
				{
					CourseId = course.Id,
					Title = course.Title,
					InstructorName = course.Instructor.FullName,
					CategoryName = course.Category.Name,
					ProgressPercentage = totalContents == 0 ? 0 : Math.Round((double)completedContents / totalContents * 100, 1),
					MyRating = rating
				});
			}

			var result = new StudentDashboardDto
			{
				FullName = user.FullName,
				Email = user.Email,
				Courses = courseDtos
			};

			return Ok(result);
		}

		[HttpPut("profile")]
		[Authorize(Roles = "Student")]
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

		[HttpPost("upload-profile-image")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> UploadProfileImage(IFormFile file, [FromServices] CloudinaryService cloudService)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			var user = await _userManager.GetUserAsync(User);

			// ✅ ارفع الصورة على Cloudinary
			string imageUrl;
			try
			{
				imageUrl = await cloudService.UploadImageAsync(file);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Image upload failed: {ex.Message}");
			}

			// ✅ احفظ الرابط في الداتا بيز
			user.ProfileImageUrl = imageUrl;
			await _userManager.UpdateAsync(user);

			return Ok(new { imageUrl });
		}



	}

}
