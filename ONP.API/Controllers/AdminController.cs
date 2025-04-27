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
	public class AdminController : Controller
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly AppDbContext _context;

		public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context)
		{
			_userManager = userManager;
			_context = context;
		}


		[HttpGet("DashboardOverview")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DashboardOverview()
		{
			var totalCourses = await _context.Courses.CountAsync();

			var totalStudents = await _context.Roles.CountAsync(u => u.Name == "Student");

			var totalInstructors = await _context.Roles.CountAsync(u => u.Name == "Instructor");

			var totalJobs = await _context.Jobs.CountAsync();
			var admin = _userManager.GetUsersInRoleAsync("Admin").Result.FirstOrDefault();

			var totalRevenue = admin.TotalEarnings;
			var recentCourses = await _context.Courses.Include(c=>c.Instructor)
				.OrderByDescending(c => c.CreatedAt)
				.Take(2)
				.ToListAsync();
			var recentJobs = await _context.Jobs
				.OrderByDescending(j => j.PostedAt)
				.Take(5)
				.ToListAsync();
			var overview = new DashboardOverview
			{
				TotalCourses = totalCourses,
				TotalStudents = totalStudents,
				TotalInstructors = totalInstructors,
				TotalJobs = totalJobs,
				TotalRevenue = totalRevenue,
				RecentCourses = recentCourses.Select(c => new CourseResponseDto
				{
					Id = c.Id,
					Title = c.Title,
					Price = c.Price,
					ImageUrl = c.ImageUrl,
					InstructorName = c.Instructor.FullName,
				}).ToList(),
				RecentJobs = recentJobs
			};
			return Ok(overview);
		}

		[HttpGet("GetUsers")]
		[Authorize (Roles = "Admin")]
		public async Task<IActionResult> GetUsers()
        {
			var users = _userManager.Users.ToList();

			var userRoles = new List<UserRoleDto>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				userRoles.Add(new UserRoleDto
				{
					Id = user.Id,
					UserName = user.UserName,
					FullName = user.FullName,
					UserRoles = roles.ToList()
				});
			}

			return Ok(userRoles);


		}

		[HttpPost("AddRole")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddRole(string userId, string role)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound("User not found");
			var result = await _userManager.AddToRoleAsync(user, role);
			if (result.Succeeded)
				return Ok("Role added successfully");
			return BadRequest("Failed to add role");
		}

		[HttpGet ("GetRoles")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetRoles()
		{
			var roles = await _context.Roles.ToListAsync();
			return Ok(roles);
		}

		[HttpPost("RemoveRole")]
		[Authorize(Roles = "Admin")]

		public async Task<IActionResult> RemoveRole(string userId, string role)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound("User not found");
			var result = await _userManager.RemoveFromRoleAsync(user, role);
			if (result.Succeeded)
				return Ok("Role removed successfully");
			return BadRequest("Failed to remove role");
		}



		[HttpDelete("DeleteUser")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteUser(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound("User not found");
			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
				return Ok("User deleted successfully");
			return BadRequest("Failed to delete user");
		}

		[HttpGet("GetAllCourses")]

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllCourses()
		{
			var courses = await _context.Courses
				.Include(c => c.Instructor)
				.Include(c => c.Category)
				.Select(c=> new GetCourseAdminDto {
					Id = c.Id,
					Title = c.Title,
					Price = c.Price,
					CategoryName = c.Category.Name,
					InstructorName = c.Instructor.FullName

				}).ToListAsync();
			return Ok(courses);
		}

		[HttpDelete("DeleteCourse")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteCourse(int courseId)
		{
			var course = await _context.Courses.FindAsync(courseId);
			if (course == null)
				return NotFound("Course not found");
			_context.Courses.Remove(course);
			await _context.SaveChangesAsync();
			return Ok("Course deleted successfully");
		}

		[HttpPut("UpdateCourse")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateCourse(int courseId, CreateCourseDto dto)
		{
			var course = await _context.Courses.FindAsync(courseId);
			if (course == null)
				return NotFound("Course not found");
			course.Title = dto.Title;
			course.Description = dto.Description;
			course.CategoryId = dto.CategoryId;
			course.Price = dto.Price;
			await _context.SaveChangesAsync();
			return Ok("Course updated successfully");
		}

		[HttpGet("GetSnippets")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetSnippets()
		{
			var snippets = await _context.CodeSnippets
				.Include(s => s.Author)
				.Select(s => new SnippetDto
				{
					Id = s.Id,
					Title = s.Title,
					Description = s.Description,
					Language = s.Language,
					Code = s.Code,
					AuthorName = s.Author.FullName
				}).ToListAsync();
			return Ok(snippets);
		}

		[HttpDelete("DeleteSnippet")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteSnippet(int snippetId)
		{
			var snippet = await _context.CodeSnippets.FindAsync(snippetId);
			if (snippet == null)
				return NotFound("Snippet not found");
			_context.CodeSnippets.Remove(snippet);
			await _context.SaveChangesAsync();
			return Ok("Snippet deleted successfully");
		}



	}
}
