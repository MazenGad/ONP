using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ONP.API.Entity;

namespace ONP.API.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
		{
		}
		public DbSet<Category> Categories { get; set; }
		public DbSet<Course> Courses { get; set; }
		public DbSet<CourseContent> CourseContents { get; set; }
		public DbSet<Enrollment> Enrollments { get; set; }
		public DbSet<CourseProgress> CourseProgress { get; set; }
		public DbSet<CourseRating> CourseRatings { get; set; }

		public DbSet<FavoriteCourse> FavoriteCourses { get; set; }
		public DbSet<CartItem> CartItems { get; set; }

		public DbSet<Job> Jobs { get; set; }
		public DbSet<TrackedJob> TrackedJobs { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Course>()
				   .HasOne(c => c.Category)
				   .WithMany(cat => cat.Courses)
				   .HasForeignKey(c => c.CategoryId);

			builder.Entity<Course>()
				   .HasOne(c => c.Instructor)
				   .WithMany()
				   .HasForeignKey(c => c.InstructorId);

			builder.Entity<Course>()
					.HasMany(c => c.Contents)
					.WithOne(cc => cc.Course)
					.HasForeignKey(cc => cc.CourseId)
					.OnDelete(DeleteBehavior.Cascade); // مسموح لأن التانية Restrict


			builder.Entity<Enrollment>()
				   .HasIndex(e => new { e.StudentId, e.CourseId })
				   .IsUnique();

			builder.Entity<Enrollment>()
				   .HasOne(e => e.Student)
				   .WithMany()
				   .HasForeignKey(e => e.StudentId)
				   .OnDelete(DeleteBehavior.Cascade); 

			builder.Entity<Enrollment>()
				  .HasOne(e => e.Course)
				  .WithMany()
				  .HasForeignKey(e => e.CourseId)
				  .OnDelete(DeleteBehavior.Restrict); // ده مهم

			builder.Entity<CourseProgress>()
			   .HasIndex(p => new { p.StudentId, p.ContentId })
			   .IsUnique(); //

			builder.Entity<CourseProgress>()
				.HasOne(p => p.Content)
				.WithMany()
				.HasForeignKey(p => p.ContentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<CourseProgress>()
				.HasOne(p => p.Course)
				.WithMany()
				.HasForeignKey(p => p.CourseId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<CourseRating>()
			   .HasIndex(r => new { r.CourseId, r.StudentId })
			   .IsUnique(); // الطالب يقيّم مرة واحدة فقط

			builder.Entity<CourseRating>()
				.HasOne(r => r.Course)
				.WithMany()
				.HasForeignKey(r => r.CourseId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<CourseRating>()
				.HasOne(r => r.Student)
				.WithMany()
				.HasForeignKey(r => r.StudentId)
				.OnDelete(DeleteBehavior.Cascade); // ده مفيهوش مشكلة

			builder.Entity<FavoriteCourse>()
			  .HasIndex(f => new { f.StudentId, f.CourseId })
			  .IsUnique();

			builder.Entity<CartItem>()
				.HasIndex(c => new { c.StudentId, c.CourseId })
				.IsUnique();


			builder.Entity<CartItem>()
				.HasOne(c => c.Course)
				.WithMany()
				.HasForeignKey(c => c.CourseId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<CartItem>()
				.HasOne(c => c.Student)
				.WithMany()
				.HasForeignKey(c => c.StudentId)
				.OnDelete(DeleteBehavior.Cascade); // ده عادي

			builder.Entity<FavoriteCourse>()
				.HasOne(f => f.Course)
				.WithMany()
				.HasForeignKey(f => f.CourseId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<FavoriteCourse>()
				.HasOne(f => f.Student)
				.WithMany()
				.HasForeignKey(f => f.StudentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<TrackedJob>()
				.HasIndex(t => new { t.StudentId, t.JobId })
				.IsUnique(); // يمنع الطالب يضيف نفس الوظيفة مرتين
		}

	}
}
