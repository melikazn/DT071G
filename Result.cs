using System.ComponentModel.DataAnnotations;

namespace QuizApp
{
    public class Result
    {
        [Key]
        public int Id { get; set; }          // Unik identifierare för varje resultat
        public string PlayerName { get; set; } // Namnet på spelaren
        public int Score { get; set; }      // Poängen som spelaren fick
        public TimeSpan ResponseTime { get; set; } // Total svarstid
    }
}
