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
	[Route("api/courses/{courseId}/content")]
	public class CourseContentController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CourseContentController(AppDbContext context , UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> AddContent(int courseId, CreateCourseContentDto dto)
		{
			// تحقق إن الكورس موجود
			var course = await _context.Courses.FindAsync(courseId);
			if (course == null) return NotFound("Course not found");

			var content = new CourseContent
			{
				CourseId = courseId,
				Title = dto.Title,
				ContentType = dto.ContentType,
				VideoUrl = dto.VideoUrl,
				TextContent = dto.TextContent,
				Order = dto.Order
			};

			_context.CourseContents.Add(content);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				content.Id,
				content.Title,
				content.ContentType,
				content.VideoUrl,
				content.TextContent,
				content.Order
			});
		}
		[HttpGet]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> GetCourseContents(int courseId)
		{
			var user = await _userManager.GetUserAsync(User);

			// هل الكورس فعلاً يخص هذا الـ Instructor؟
			var course = await _context.Courses
				.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == user.Id);

			if (course == null)
				return Forbid("You are not the owner of this course.");

			var contents = await _context.CourseContents
				.Where(c => c.CourseId == courseId)
				.OrderBy(c => c.Order)
				.Select(c => new
				{
					c.Id,
					c.Title,
					c.ContentType,
					c.VideoUrl,
					c.TextContent,
					c.Order
				})
				.ToListAsync();

			return Ok(contents);
		}

		[HttpPut("{contentId}")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> UpdateContent(int courseId, int contentId, CreateCourseContentDto dto)
		{
			var user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == user.Id);

			if (course == null)
				return Forbid("You are not the owner of this course.");

			var content = await _context.CourseContents
				.FirstOrDefaultAsync(c => c.Id == contentId && c.CourseId == courseId);

			if (content == null)
				return NotFound("Content not found.");

			content.Title = dto.Title;
			content.ContentType = dto.ContentType;
			content.VideoUrl = dto.VideoUrl;
			content.TextContent = dto.TextContent;
			content.Order = dto.Order;

			await _context.SaveChangesAsync();

			return Ok("Content updated successfully.");
		}

		[HttpDelete("{contentId}")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> DeleteContent(int courseId, int contentId)
		{
			var user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == user.Id);

			if (course == null)
				return Forbid("You are not the owner of this course.");

			var content = await _context.CourseContents
				.FirstOrDefaultAsync(c => c.Id == contentId && c.CourseId == courseId);

			if (content == null)
				return NotFound("Content not found.");

			_context.CourseContents.Remove(content);
			await _context.SaveChangesAsync();

			return Ok("Content deleted successfully.");
		}


	}

}
