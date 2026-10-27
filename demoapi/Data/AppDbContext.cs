using demoapi.Models;
using Microsoft.EntityFrameworkCore;

namespace demoapi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<StudentEntity> Students => Set<StudentEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<EnrollmentEntity> Enrollments => Set<EnrollmentEntity>();
    public DbSet<AssessmentEntity> Assessments => Set<AssessmentEntity>();
    public DbSet<ScoreEntity> Scores => Set<ScoreEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnrollmentEntity>(e =>
        {
            // DB column is ENUM('ACTIVE','COMPLETED','DROPPED')
            e.Property(x => x.Status).HasConversion(
                v => v.ToString().ToUpperInvariant(),
                s => Enum.Parse<EnrollmentStatus>(s, true));

            e.HasOne(x => x.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssessmentEntity>(a =>
        {
            a.HasOne(x => x.Course)
                .WithMany(c => c.Assessments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScoreEntity>(s =>
        {
            s.HasOne(x => x.Enrollment)
                .WithMany(e => e.Scores)
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            s.HasOne(x => x.Assessment)
                .WithMany(a => a.Scores)
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}