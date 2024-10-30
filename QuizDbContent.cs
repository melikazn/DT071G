using Microsoft.EntityFrameworkCore;

namespace QuizApp
{
    public class QuizDbContext : DbContext
    {
        public DbSet<Result> Results { get; set; } // Tabell för att lagra resultat

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=quiz.db"); // Anger databasens filnamn
        }
    }
}
