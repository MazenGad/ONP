using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ONP.API.Data;
using ONP.API.DTO;
using ONP.API.Entity;

namespace ONP.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoryController : ControllerBase
	{
		private readonly AppDbContext _context;

		public CategoryController(AppDbContext context)
		{
			_context = context;
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
		{
			var category = new Category { Name = dto.Name };
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();
			return Ok(category);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAllCategories()
		{
			var categories = await _context.Categories.ToListAsync();
			return Ok(categories);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateCategory(int id, CreateCategoryDto dto)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category == null)
				return NotFound("Category not found.");

			category.Name = dto.Name;
			await _context.SaveChangesAsync();

			return Ok("Category updated.");
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category == null)
				return NotFound("Category not found.");

			_context.Categories.Remove(category);
			await _context.SaveChangesAsync();

			return Ok("Category deleted.");
		}
	}
}
