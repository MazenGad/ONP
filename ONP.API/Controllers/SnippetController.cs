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
	public class SnippetController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public SnippetController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		//  إضافة snippet
		[HttpPost]
		[Authorize(Roles = "Instructor,Student,Admin")]
		public async Task<IActionResult> CreateSnippet(CreateSnippetDto dto)
		{
			var user = await _userManager.GetUserAsync(User);

			var snippet = new CodeSnippet
			{
				Title = dto.Title,
				Description = dto.Description,
				Language = dto.Language,
				Code = dto.Code,
				AuthorId = user.Id
			};

			_context.CodeSnippets.Add(snippet);
			await _context.SaveChangesAsync();

			return Ok(new { snippet.Id });
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAllSnippets(
		[FromQuery] string? language = null,
		[FromQuery] string? search = null)
		{
			var query = _context.CodeSnippets
				.Include(s => s.Author)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(s =>
					s.Title.Contains(search) ||
					(s.Description != null && s.Description.Contains(search)));
			}

			if (!string.IsNullOrWhiteSpace(language))
			{
				query = query.Where(s => s.Language.ToLower() == language.ToLower());
			}

			var snippets = await query
				.OrderByDescending(s => s.CreatedAt)
				.Select(s => new SnippetDto
				{
					Id = s.Id,
					Title = s.Title,
					Description = s.Description,
					Language = s.Language,
					Code = s.Code,
					AuthorName = s.Author.FullName,
					CreatedAt = s.CreatedAt
				})
				.ToListAsync();

			return Ok(snippets);
		}


		//  عرض Snippet واحد
		[HttpGet("{id}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetSnippet(int id)
		{
			var s = await _context.CodeSnippets
				.Include(s => s.Author)
				.FirstOrDefaultAsync(s => s.Id == id);

			if (s == null)
				return NotFound("Snippet not found");

			return Ok(new SnippetDto
			{
				Id = s.Id,
				Title = s.Title,
				Description = s.Description,
				Language = s.Language,
				Code = s.Code,
				AuthorName = s.Author.FullName,
				CreatedAt = s.CreatedAt
			});
		}

		//  حذف Snippet (صاحبه فقط)
		[HttpDelete("{id}")]
		[Authorize(Roles = "Instructor,Student,Admin")]
		public async Task<IActionResult> DeleteSnippet(int id)
		{
			var user = await _userManager.GetUserAsync(User);

			var snippet = await _context.CodeSnippets
				.FirstOrDefaultAsync(s => s.Id == id && s.AuthorId == user.Id);

			if (snippet == null)
				return Forbid("You can only delete your own snippets.");

			_context.CodeSnippets.Remove(snippet);
			await _context.SaveChangesAsync();

			return Ok("Snippet deleted successfully.");
		}


		[HttpPut("{id}")]
		[Authorize(Roles = "Instructor,Student,Admin")]
		public async Task<IActionResult> UpdateSnippet(int id, CreateSnippetDto dto)
		{
			var user = await _userManager.GetUserAsync(User);

			var snippet = await _context.CodeSnippets
				.FirstOrDefaultAsync(s => s.Id == id && s.AuthorId == user.Id);

			if (snippet == null)
				return Forbid("You can only update your own snippets.");

			snippet.Title = dto.Title;
			snippet.Description = dto.Description;
			snippet.Language = dto.Language;
			snippet.Code = dto.Code;

			await _context.SaveChangesAsync();

			return Ok("Snippet updated successfully.");
		}

	}

}
