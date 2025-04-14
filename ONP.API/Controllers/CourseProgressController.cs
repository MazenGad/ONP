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
	public class CourseProgressController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CourseProgressController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost("{contentId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> MarkAsCompleted(int contentId)
		{
			var user = await _userManager.GetUserAsync(User);

			var content = await _context.CourseContents
				.Include(c => c.Course)
				.FirstOrDefaultAsync(c => c.Id == contentId);

			if (content == null)
				return NotFound("Content not found.");

			var isEnrolled = await _context.Enrollments
				.AnyAsync(e => e.StudentId == user.Id && e.CourseId == content.CourseId);

			if (!isEnrolled)
				return Unauthorized("You are not enrolled in this course.");

			var alreadyExists = await _context.CourseProgress
				.AnyAsync(p => p.StudentId == user.Id && p.ContentId == contentId);

			if (alreadyExists)
				return Ok("Already marked as completed.");

			var progress = new CourseProgress
			{
				StudentId = user.Id,
				CourseId = content.CourseId,
				ContentId = contentId
			};

			_context.CourseProgress.Add(progress);
			await _context.SaveChangesAsync();

			return Ok("Progress marked.");
		}

		[HttpGet("{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetCourseProgress(int courseId)
		{
			var user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.Include(c => c.Contents)
				.FirstOrDefaultAsync(c => c.Id == courseId);

			if (course == null)
				return NotFound("Course not found.");

			var isEnrolled = await _context.Enrollments
				.AnyAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

			if (!isEnrolled)
				return Unauthorized("You are not enrolled in this course.");

			var completed = await _context.CourseProgress
				.CountAsync(p => p.StudentId == user.Id && p.CourseId == courseId);

			var total = course.Contents.Count;

			var dto = new CourseProgressDto
			{
				CourseId = courseId,
				CourseTitle = course.Title,
				CompletedContents = completed,
				TotalContents = total,
				ProgressPercentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 2)
			};

			return Ok(dto);
		}
	}

}
