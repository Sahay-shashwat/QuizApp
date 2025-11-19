using Microsoft.EntityFrameworkCore;
using Core.Entities;

namespace Infrastructure.Persistence;

public class QuizDbContext : DbContext
{
  public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) {}
  public DbSet<Quiz> Quizzes { get; set; }
  public DbSet<Question> Questions { get; set; }
  public DbSet<Option> Options { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<Submission> Submissions { get; set; }
  //public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>()
      .HasKey(x => x.Id);

    modelBuilder.Entity<Quiz>()
            .HasKey(q => q.ID);

    modelBuilder.Entity<Option>()
            .HasKey(q => q.ID);

    modelBuilder.Entity<Question>()
            .HasKey(q => q.ID);

    modelBuilder.Entity<Quiz>()
      .HasOne(o => o.User)
      .WithMany(q => q.Quizs)
      .HasForeignKey(q => q.UserId)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Question>()
        .HasOne(o => o.Quiz)
        .WithMany(q => q.Questions)
        .HasForeignKey(q => q.QuizId);

    modelBuilder.Entity<Option>()
        .HasOne(o => o.Question)
        .WithMany(q => q.Options)
        .HasForeignKey(q => q.QuestionId);

    modelBuilder.Entity<Submission>()
        .HasOne(q => q.Question)
        .WithMany(q => q.Submissions)
        .HasForeignKey(q => q.QuestionID);

    modelBuilder.Entity<Submission>()
        .HasOne<User>()
        .WithMany(q => q.Submissions)
        .HasForeignKey(q => q.UserId);

    modelBuilder.Entity<Submission>()
        .HasOne<Option>()
        .WithMany(q => q.Submissions)
        .HasForeignKey(q => q.SelectedOptionId)
        .OnDelete(DeleteBehavior.Restrict);

  }
}

