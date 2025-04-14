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
	public class CourseController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CourseController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> CreateCourse(CreateCourseDto dto)
		{
			var instructor = await _userManager.GetUserAsync(User);

			var course = new Course
			{
				Title = dto.Title,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				InstructorId = instructor.Id
			};

			_context.Courses.Add(course);
			await _context.SaveChangesAsync();

			var category = await _context.Categories.FindAsync(dto.CategoryId);

			var response = new CourseResponseDto
			{
				Id = course.Id,
				Title = course.Title,
				Description = course.Description,
				CategoryName = category?.Name,
				InstructorName = instructor.FullName,
				CreatedAt = course.CreatedAt
			};

			return Ok(response);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> UpdateCourse(int id, CreateCourseDto dto)
		{
			var instructor = await _userManager.GetUserAsync(User);

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

			if (course == null)
				return NotFound("Course not found");

			if (course.InstructorId != instructor.Id)
				return Forbid("You can only update your own courses.");

			course.Title = dto.Title;
			course.Description = dto.Description;
			course.CategoryId = dto.CategoryId;

			await _context.SaveChangesAsync();

			return Ok("Course updated successfully.");
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> DeleteCourse(int id)
		{
			var instructor = await _userManager.GetUserAsync(User);

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

			if (course == null)
				return NotFound("Course not found");

			if (course.InstructorId != instructor.Id)
				return Forbid("You can only delete your own courses.");

			_context.Courses.Remove(course);
			await _context.SaveChangesAsync();

			return Ok("Course deleted.");
		}

		[HttpGet("by-category")]
		public async Task<IActionResult> GetAllCourses([FromQuery] int? categoryId = null)
		{
			var query = _context.Courses
				.Include(c => c.Category)
				.Include(c => c.Instructor)
				.AsQueryable();

			if (categoryId.HasValue)
			{
				query = query.Where(c => c.CategoryId == categoryId.Value);
			}

			var courses = await query
				.Select(c => new CourseResponseDto
				{
					Id = c.Id,
					Title = c.Title,
					Description = c.Description,
					CategoryName = c.Category.Name,
					InstructorName = c.Instructor.FullName,
					CreatedAt = c.CreatedAt
				})
				.ToListAsync();

			return Ok(courses);
		}

		[HttpGet("{id}/student-view")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetFullCourseForStudent(int id)
		{
			var user = await _userManager.GetUserAsync(User);

			var isEnrolled = await _context.Enrollments
				.AnyAsync(e => e.StudentId == user.Id && e.CourseId == id);

			if (!isEnrolled)
				return Unauthorized("You must enroll in this course to view its content.");

			var course = await _context.Courses
				.Include(c => c.Category)
				.Include(c => c.Instructor)
				.Include(c => c.Contents.OrderBy(cc => cc.Order))
				.FirstOrDefaultAsync(c => c.Id == id);

			if (course == null)
				return NotFound("Course not found");

			var result = new CourseWithContentDto
			{
				CourseId = course.Id,
				Title = course.Title,
				Description = course.Description,
				CategoryName = course.Category.Name,
				InstructorName = course.Instructor.FullName,
				Contents = course.Contents.Select(c => new CourseContentDto
				{
					Title = c.Title,
					ContentType = c.ContentType,
					VideoUrl = c.VideoUrl,
					TextContent = c.TextContent,
					Order = c.Order
				}).ToList()
			};

			return Ok(result);
		}

		[HttpGet("{id}/public-view")]
		[AllowAnonymous]
		public async Task<IActionResult> GetCoursePublicInfo(int id)
		{
			var course = await _context.Courses
				.Include(c => c.Category)
				.Include(c => c.Instructor)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (course == null)
				return NotFound("Course not found");

			var enrollCount = await _context.Enrollments
				.CountAsync(e => e.CourseId == id);

			var ratings = await _context.CourseRatings
				.Where(r => r.CourseId == id)
				.Select(r => r.RatingValue)
				.ToListAsync();

			var avgRating = ratings.Any() ? ratings.Average() : 0;

			var result = new CourseResponseDto
			{
				Id = course.Id,
				Title = course.Title,
				Description = course.Description,
				CategoryName = course.Category.Name,
				InstructorName = course.Instructor.FullName,
				CreatedAt = course.CreatedAt,
				EnrolledStudents = enrollCount,
				AverageRating = Math.Round(avgRating, 1)
			};

			return Ok(result);
		}
	}

}
