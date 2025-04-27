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
		private readonly CloudinaryService _cloudinaryService;


		public CourseController(AppDbContext context, UserManager<ApplicationUser> userManager, CloudinaryService cloudinaryService)
		{
			_context = context;
			_userManager = userManager;
			_cloudinaryService = cloudinaryService;

		}

		[HttpPost]
		[Authorize(Roles = "Instructor")]
		public async Task<IActionResult> CreateCourse(CreateCourseDto dto)
		{
			var instructor = await _userManager.GetUserAsync(User);

			string imageUrl = null;
			if (dto.Image != null && dto.Image.Length > 0)
			{
				imageUrl = await _cloudinaryService.UploadImageAsync(dto.Image);
			}

			var course = new Course
			{
				Title = dto.Title,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				InstructorId = instructor.Id,
				Price = dto.Price,
				ImageUrl = imageUrl
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
				CreatedAt = course.CreatedAt,
				Price = course.Price,
				ImageUrl = imageUrl
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
			course.Price = dto.Price;

			if (dto.Image != null && dto.Image.Length > 0)
			{
				var imageUrl = await _cloudinaryService.UploadImageAsync(dto.Image);
				course.ImageUrl = imageUrl;
			}
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
				return NotFound("Course not found.");

			if (course.InstructorId != instructor.Id)
				return Forbid("You can only delete your own courses.");

			// ✅ Check لو الكورس عليه مدفوعات
			bool hasPayments = await _context.Payments.AnyAsync(p => p.CourseId == id);
			if (hasPayments)
				return BadRequest("Cannot delete course that has active payments.");

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var favorites = _context.FavoriteCourses.Where(f => f.CourseId == id);
				_context.FavoriteCourses.RemoveRange(favorites);

				var cartItems = _context.CartItems.Where(c => c.CourseId == id);
				_context.CartItems.RemoveRange(cartItems);

				_context.Courses.Remove(course);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return Ok(new { message = "Course deleted successfully." });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, $"An error occurred while deleting the course: {ex.Message}");
			}
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
					CreatedAt = c.CreatedAt,
					Price = c.Price,
					ImageUrl = c.ImageUrl,
					// عدد الطلبة
					EnrolledStudents = _context.Enrollments.Count(e => e.CourseId == c.Id),
					// متوسط التقييم
					AverageRating = Math.Round(
						_context.CourseRatings
							.Where(r => r.CourseId == c.Id)
							.Select(r => (double?)r.RatingValue)
							.Average() ?? 0, 1)
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
					.ThenInclude(cc => cc.LessonCodes) // ده المهم
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
					Id = c.Id,
					Title = c.Title,
					ContentType = c.ContentType,
					VideoUrl = c.VideoUrl,
					TextContent = c.TextContent,
					Order = c.Order,
					LessonCodes = c.LessonCodes.Select(s => new LessonCodesDto
					{
						Language = s.Language,
						Code = s.Code
					}).ToList()
				}).ToList()
			};

			return Ok(result);
		}


		[HttpGet("{id}/preview")]
		[AllowAnonymous]
		public async Task<IActionResult> GetCoursePreview(int id)
		{
			var course = await _context.Courses
				.Include(c => c.Category)
				.Include(c => c.Instructor)
				.Include(c => c.Contents.OrderBy(cc => cc.Order))
				.FirstOrDefaultAsync(c => c.Id == id);

			if (course == null)
				return NotFound("Course not found");

			var result = new
			{
				CourseId = course.Id,
				Title = course.Title,
				Description = course.Description,
				CategoryName = course.Category.Name,
				course.ImageUrl,
				InstructorName = course.Instructor.FullName,
				LessonTitles = course.Contents.Select(c => new
				{
					c.Title,
					c.Order
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
				AverageRating = Math.Round(avgRating, 1),
				Price = course.Price
			};

			return Ok(result);
		}

		[HttpPost("checkout")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> Checkout([FromBody] List<int> courseIds)
		{
			var user = await _userManager.GetUserAsync(User);

			var courses = await _context.Courses
				.Where(c => courseIds.Contains(c.Id))
				.Include(c => c.Instructor)
				.ToListAsync();

			if (!courses.Any()) return BadRequest("No valid courses selected.");

			foreach (var course in courses)
			{
				// Check if already enrolled
				var alreadyEnrolled = await _context.Enrollments
					.AnyAsync(e => e.CourseId == course.Id && e.StudentId == user.Id);

				if (!alreadyEnrolled)
				{
					_context.Enrollments.Add(new Enrollment
					{
						CourseId = course.Id,
						StudentId = user.Id,
						EnrolledAt = DateTime.UtcNow
					});
				}

				// السعر من الكورس نفسه
				var price = course.Price;
				var instructorShare = Math.Round(price * 0.8m, 2);
				var adminShare = price - instructorShare;

				// Add payment record
				_context.Payments.Add(new Payment
				{
					StudentId = user.Id,
					CourseId = course.Id,
					AmountPaid = price,
					InstructorShare = instructorShare,
					AdminShare = adminShare,
					PaidAt = DateTime.UtcNow
				});

				// Update instructor earnings
				var instructor = course.Instructor;
				instructor.TotalEarnings += instructorShare;
				var admin = _userManager.GetUsersInRoleAsync("Admin").Result.FirstOrDefault();
				if (admin != null)
				{
					admin.TotalEarnings += adminShare;
					_context.Users.Update(admin);
				}
				_context.Users.Update(instructor);
			}

			await _context.SaveChangesAsync();
			return Ok(new { message = "Payment successful. Courses have been added." });
		}

	}

}
