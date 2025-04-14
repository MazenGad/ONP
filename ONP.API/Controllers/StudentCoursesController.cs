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
	[Route("api/student-courses")]
	public class StudentCoursesController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public StudentCoursesController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		private async Task<ApplicationUser> GetCurrentUserAsync() =>
			await _userManager.GetUserAsync(User);

		// ---------------- FAVORITES ----------------

		[HttpPost("favorites/{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> AddToFavorites(int courseId)
		{
			var user = await GetCurrentUserAsync();

			var exists = await _context.FavoriteCourses
				.AnyAsync(f => f.CourseId == courseId && f.StudentId == user.Id);

			if (exists)
				return BadRequest("Course already in favorites.");

			_context.FavoriteCourses.Add(new FavoriteCourse
			{
				StudentId = user.Id,
				CourseId = courseId
			});

			await _context.SaveChangesAsync();
			return Ok("Added to favorites.");
		}

		[HttpDelete("favorites/{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> RemoveFromFavorites(int courseId)
		{
			var user = await GetCurrentUserAsync();

			var fav = await _context.FavoriteCourses
				.FirstOrDefaultAsync(f => f.CourseId == courseId && f.StudentId == user.Id);

			if (fav == null)
				return NotFound();

			_context.FavoriteCourses.Remove(fav);
			await _context.SaveChangesAsync();

			return Ok("Removed from favorites.");
		}

		[HttpGet("favorites")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetFavorites()
		{
			var user = await GetCurrentUserAsync();

			var courses = await _context.FavoriteCourses
				.Where(f => f.StudentId == user.Id)
				.Include(f => f.Course).ThenInclude(c => c.Instructor)
				.Include(f => f.Course).ThenInclude(c => c.Category)
				.Select(f => new SimpleCourseDto
				{
					CourseId = f.Course.Id,
					Title = f.Course.Title,
					InstructorName = f.Course.Instructor.FullName,
					CategoryName = f.Course.Category.Name
				})
				.ToListAsync();

			return Ok(courses);
		}

		// ---------------- CART ----------------

		[HttpPost("cart/{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> AddToCart(int courseId)
		{
			var user = await GetCurrentUserAsync();

			var exists = await _context.CartItems
				.AnyAsync(c => c.CourseId == courseId && c.StudentId == user.Id);

			if (exists)
				return BadRequest("Course already in cart.");

			_context.CartItems.Add(new CartItem
			{
				StudentId = user.Id,
				CourseId = courseId
			});

			await _context.SaveChangesAsync();
			return Ok("Added to cart.");
		}

		[HttpDelete("cart/{courseId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> RemoveFromCart(int courseId)
		{
			var user = await GetCurrentUserAsync();

			var item = await _context.CartItems
				.FirstOrDefaultAsync(c => c.CourseId == courseId && c.StudentId == user.Id);

			if (item == null)
				return NotFound();

			_context.CartItems.Remove(item);
			await _context.SaveChangesAsync();

			return Ok("Removed from cart.");
		}

		[HttpGet("cart")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetCart()
		{
			var user = await GetCurrentUserAsync();

			var courses = await _context.CartItems
				.Where(c => c.StudentId == user.Id)
				.Include(c => c.Course).ThenInclude(c => c.Instructor)
				.Include(c => c.Course).ThenInclude(c => c.Category)
				.Select(c => new SimpleCourseDto
				{
					CourseId = c.Course.Id,
					Title = c.Course.Title,
					InstructorName = c.Course.Instructor.FullName,
					CategoryName = c.Course.Category.Name
				})
				.ToListAsync();

			return Ok(courses);
		}

		[HttpPost("purchase")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> PurchaseCartCourses()
		{
			var user = await _userManager.GetUserAsync(User);

			// 1. هات الكورسات اللي في السلة
			var cartCourses = await _context.CartItems
				.Where(c => c.StudentId == user.Id)
				.ToListAsync();

			if (!cartCourses.Any())
				return BadRequest("Your cart is empty.");

			var courseIds = cartCourses.Select(c => c.CourseId).ToList();

			// 2. شوف إيه الكورسات اللي هو مسجل فيها فعلاً (عشان متتكررش)
			var alreadyEnrolled = await _context.Enrollments
				.Where(e => e.StudentId == user.Id && courseIds.Contains(e.CourseId))
				.Select(e => e.CourseId)
				.ToListAsync();

			// 3. فلتر الكورسات اللي مش متسجل فيها
			var toEnroll = cartCourses
				.Where(c => !alreadyEnrolled.Contains(c.CourseId))
				.Select(c => new Enrollment
				{
					StudentId = user.Id,
					CourseId = c.CourseId
				}).ToList();

			if (!toEnroll.Any())
				return BadRequest("You're already enrolled in all courses.");

			// 4. ضيفهم في الـ Enrollments
			_context.Enrollments.AddRange(toEnroll);

			// 5. احذفهم من الـ Cart
			_context.CartItems.RemoveRange(cartCourses);

			await _context.SaveChangesAsync();

			return Ok("Courses purchased and enrolled successfully.");
		}

		[HttpPost("purchase-selected")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> PurchaseSelectedCourses([FromBody] PurchaseSelectedDto dto)
		{
			var user = await _userManager.GetUserAsync(User);

			if (dto.CourseIds == null || !dto.CourseIds.Any())
				return BadRequest("No course IDs provided.");

			// 1. هات الكورسات اللي في السلة واللي اتبعتت في الطلب
			var cartCourses = await _context.CartItems
				.Where(c => c.StudentId == user.Id && dto.CourseIds.Contains(c.CourseId))
				.ToListAsync();

			if (!cartCourses.Any())
				return BadRequest("Selected courses not found in cart.");

			// 2. شوف المسجل فيهم قبل كده
			var alreadyEnrolled = await _context.Enrollments
				.Where(e => e.StudentId == user.Id && dto.CourseIds.Contains(e.CourseId))
				.Select(e => e.CourseId)
				.ToListAsync();

			// 3. ضيف الكورسات الجديدة بس
			var toEnroll = cartCourses
				.Where(c => !alreadyEnrolled.Contains(c.CourseId))
				.Select(c => new Enrollment
				{
					StudentId = user.Id,
					CourseId = c.CourseId
				}).ToList();

			if (!toEnroll.Any())
				return BadRequest("You're already enrolled in all selected courses.");

			_context.Enrollments.AddRange(toEnroll);
			_context.CartItems.RemoveRange(cartCourses);

			await _context.SaveChangesAsync();

			return Ok("Selected courses purchased successfully.");
		}


	}

}
