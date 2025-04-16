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
	public class JobController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public JobController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		//  Admin يضيف وظيفة
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateJob(CreateJobDto dto)
		{
			var job = new Job
			{
				Title = dto.Title,
				Content = dto.Content,
				ContactEmail = dto.ContactEmail
			};

			_context.Jobs.Add(job);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Job created", job.Id });
		}

		// كل الناس تقدر تشوف الوظائف
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAllJobs(
			[FromQuery] string? search = null,
			[FromQuery] int? lastDays = null)
		{
			var query = _context.Jobs.AsQueryable();

			// 🔍 فلترة بالكلمة المفتاحية
			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(j =>
					j.Title.Contains(search) || j.Content.Contains(search));
			}

			// 📆 فلترة حسب عدد الأيام الأخيرة
			if (lastDays.HasValue)
			{
				var sinceDate = DateTime.UtcNow.AddDays(-lastDays.Value);
				query = query.Where(j => j.PostedAt >= sinceDate);
			}

			var jobs = await query
				.OrderByDescending(j => j.PostedAt)
				.Select(j => new JobDto
				{
					Id = j.Id,
					Title = j.Title,
					Content = j.Content,
					ContactEmail = j.ContactEmail,
					PostedAt = j.PostedAt
				})
				.ToListAsync();

			return Ok(jobs);
		}


		//  الطالب يعمل Track لو قدم
		[HttpPost("{jobId}/track")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> TrackJob(int jobId)
		{
			var user = await _userManager.GetUserAsync(User);

			var exists = await _context.TrackedJobs
				.AnyAsync(t => t.JobId == jobId && t.StudentId == user.Id);

			if (exists)
				return BadRequest("Already tracked this job.");

			_context.TrackedJobs.Add(new TrackedJob
			{
				JobId = jobId,
				StudentId = user.Id
			});

			await _context.SaveChangesAsync();
			return Ok("Job tracked successfully.");
		}

		//  الطالب يشوف الوظايف اللي تتبعها
		[HttpGet("my-tracked")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetTrackedJobs()
		{
			var user = await _userManager.GetUserAsync(User);

			var tracked = await _context.TrackedJobs
				.Where(t => t.StudentId == user.Id)
				.Include(t => t.Job)
				.Select(t => new JobDto
				{
					Id = t.Job.Id,
					Title = t.Job.Title,
					Content = t.Job.Content,
					ContactEmail = t.Job.ContactEmail,
					PostedAt = t.Job.PostedAt
				})
				.ToListAsync();

			return Ok(tracked);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteJob(int id)
		{
			var job = await _context.Jobs
				.Include(j => j.TrackedBy) // لو فيه طلبة متبعينها نمسح معاها
				.FirstOrDefaultAsync(j => j.Id == id);

			if (job == null)
				return NotFound("Job not found.");

			_context.TrackedJobs.RemoveRange(job.TrackedBy); // نمسح التتبع الأول
			_context.Jobs.Remove(job); // وبعدين الوظيفة نفسها

			await _context.SaveChangesAsync();
			return Ok("Job deleted successfully.");
		}

	}

}
