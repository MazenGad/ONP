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
	public class EnrollmentController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public EnrollmentController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost("{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> EnrollInCourse(int courseId)
		{
			var user = await _userManager.GetUserAsync(User);
			var course = await _context.Courses.FindAsync(courseId);

			if (course == null) return NotFound("Course not found");

			var exists = await _context.Enrollments
				.AnyAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

			if (exists) return BadRequest("Already enrolled");

			var enrollment = new Enrollment
			{
				StudentId = user.Id,
				CourseId = courseId
			};

			_context.Enrollments.Add(enrollment);
			await _context.SaveChangesAsync();

			return Ok("Enrollment successful");
		}

		[HttpGet("my-courses")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetMyCourses()
		{
			var user = await _userManager.GetUserAsync(User);

			var enrollments = await _context.Enrollments
				.Where(e => e.StudentId == user.Id)
				.Include(e => e.Course)
					.ThenInclude(c => c.Category)
				.Include(e => e.Course.Instructor)
				.Select(e => new EnrolledCourseDto
				{
					CourseId = e.Course.Id,
					Title = e.Course.Title,
					Category = e.Course.Category.Name,
					Instructor = e.Course.Instructor.FullName,
					ImageUrl = e.Course.ImageUrl,
					EnrolledAt = e.EnrolledAt
				})
				.ToListAsync();

			return Ok(enrollments);
		}
	}

}
