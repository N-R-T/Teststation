using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Teststation.Models
{
    public class Database : IdentityDbContext
    {
        public Database(DbContextOptions<Database> options) : base(options)
        { }

        public DbSet<Test> Tests { get; set; }
        public DbSet<UserInformation> UserInformation { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public DbSet<Question> Questions { get; set; }
        public DbSet<MathQuestion> MathQuestions { get; set; }
        public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; }
        public DbSet<Choice> Choices { get; set; }

        public DbSet<Answer> Answers { get; set; }
        public DbSet<MathAnswer> MathAnswers { get; set; }
        public DbSet<MultipleChoiceAnswer> MultipleChoiceAnswers { get; set; }
    }
}
