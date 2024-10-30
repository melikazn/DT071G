using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuizApp
{
    class Program
    {
        // Main-metoden är programmets startpunkt.
        static async Task Main(string[] args)
        {

            // Skapar databasen om den redan inte finns
            using (var db = new QuizDbContext())
            {
                db.Database.EnsureCreated();
            }

            Console.WriteLine("Välkommen till Svensk Grammatik Quiz!");

            string playerName;
            while (true)
            {
                // Loopa för att be anvöndarens namn (bara bokstäver är tillåtna)
                Console.Write("Ange ditt namn (endast bokstäver tillåtna): ");
                playerName = Console.ReadLine();

                // Kolla om namnet är tomt eller ogiltigt
                if (string.IsNullOrWhiteSpace(playerName) || !IsValidName(playerName))
                {
                    Console.WriteLine("Felaktigt namn. Vänligen ange ett giltigt namn.");
                }
                else
                {
                    break; // Ut ur loopen om namnet är giltigt
                }
            }

            Console.WriteLine("Du har 15 frågor att svara på. Låt oss börja!\n");
            // Listor med alla frågor och deras alternativ
            List<Question> questions = new List<Question>
            {
                // En fråga skapas- till varje fråga har vi 4 alternativ
                new Question("Vad är ett substantiv?", 
                    new List<string> { "Ett ord för en sak", "Ett ord för en handling", "Ett beskrivande ord", "Ett namn på en plats" }, 
                    0), // Index 0 anger att det är första svaret som är det korrekta svaret
                new Question("Vad är grundformen av ett verb?", 
                    new List<string> {  "Presens", "Preteritum", "Supinum", "Infinitiv", }, 
                    3),
                new Question("Vilket ord är ett pronomen?", 
                    new List<string> { "Snabbt", "Jag", "Vänskap", "Springa" }, 
                    1),
                new Question("Vilken är den korrekta böjningen av verbet 'att gå' i preteritum?",
                    new List<string> { "Gick", "Går", "Gått", "Gå" },
                    0),
                new Question("Vad är ett adjektiv?",
                    new List<string> { "Ett ord för en sak",  "Ett pronomen", "Ett verb" , "Ett beskrivande ord"},
                    3),
                new Question("Vilken av följande är en preposition?",
                    new List<string> { "Och", "Under", "Springa", "Sak" },
                    1),
                new Question("Vad är den korrekta böjningen 'att läsa' i infinitiv?",
                    new List<string> { "Läste", "Läser", "Att läsa", "Läsar" },
                    2),
                new Question("Vilket av följande är ett exempel på en pluralform?",
                    new List<string> { "Bok", "Böcker", "Boken", "Boks" },
                    1),
                new Question("Vad är skillnaden mellan 'sin' och 'hans'?",
                    new List<string> { "Sin är reflexiv, hans är possessiv", "Båda är samma", "Sin är singular, hans är plural", "Sin används för objekt" },
                    0),
                new Question("Vilket av följande ord är ett adverb?",
                    new List<string> { "Snabba", "Snabb", "Sak", "Snabbt" },
                    3),
                new Question("Vad är den korrekta formen av verbet 'att prata' i presens?",
                    new List<string> { "Pratade", "Pratar", "Att prata", "Prata" },
                    1),
                new Question("Vad är ett synonym?",
                    new List<string> { "Ord med samma betydelse", "Ord med motsatt betydelse", "Ord med liknande ljud", "Ord som är gamla" },
                    0),
                new Question("Vilket av följande är en korrekt böjning av 'att se' i preteritum?",
                    new List<string> { "Ser", "Sett", "Såg", "Sedd" },
                    2),
                new Question("Vilken av följande ord är ett substantiv?",
                    new List<string> { "Snabbt", "Långsam", "Äpple", "Att springa" },
                    2),
                new Question("Vad är en konjunktion?",
                    new List<string> { "Ett ord som binder ihop satser", "Ett ord som beskriver ett substantiv", "Ett ord som anger en handling", "Ett pronomen" },
                    0),
                new Question("Vilket ord är en antonym till 'stor'?",
                    new List<string> { "Klein", "Liten", "Mindre", "Tyngre" },
                    1)
            };

            int score = 0; // Användarens poäng
            TimeSpan totalResponseTime = TimeSpan.Zero; // Total tid användaren tog för att svara
 
            // Loopar genom alla frågor och hämtar svar från användaren
            foreach (var question in questions)
            {
                // Kör frågan och uppdaterar svarstid och poäng
                var (responseTime, questionScore) = await RunQuizQuestion(question);
                totalResponseTime += responseTime; // Lägg till svarstiden
                score += questionScore; // Lägg till poängen från frågan
            }

            // Granskar användarens nivå baserat på poäng
            string level = DetermineLevel(score);
            Console.WriteLine($"Din nivå är: {level}");

            // Spara eller uppdatera resultatet i databasen
            using (var db = new QuizDbContext())
            {
                var result = await db.Results.FirstOrDefaultAsync(r => r.PlayerName == playerName);

                if (result != null)
                {
                    // Om namnet redan finns, uppdatera poängen och svarstiden
                    result.Score = score;
                    result.ResponseTime = totalResponseTime; // Spara total svarstid
                    Console.WriteLine("Dina poäng har uppdaterats.");
                }
                else
                {
                    // Annars, skapa en ny post
                    result = new Result { PlayerName = playerName, Score = score, ResponseTime = totalResponseTime };
                    db.Results.Add(result);
                    Console.WriteLine("Ditt resultat har sparats.");
                }

                await db.SaveChangesAsync(); // Spara asynkront
            }

            // Visa resultat
            DisplayResults();
        }

        // Metod för att kontrollera om namnet är giltigt
        static bool IsValidName(string name)
        {
            // Kontrollera att namnet endast innehåller bokstäver och mellanslag
            return name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        // Metod som hanterar att ställa en fråga och mäta svarstid
        static async Task<(TimeSpan responseTime, int score)> RunQuizQuestion(Question question)
        {
            TimeSpan responseTime;
            int score = 0; // Poängen som ska returneras

            Console.WriteLine("\n" + question.Text); // Skriver ut frågan
            for (int i = 0; i < question.Options.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {question.Options[i]}"); // Skriver ut svarsalternativ
            }

            // Startar en tidsgräns på 10 sekunder för att besvara frågan
            using (var cts = new CancellationTokenSource())
            {
                Task delayTask = Task.Delay(10000, cts.Token);
                Console.Write("Ditt svar (ange nummer): ");
                Task<string> inputTask = Task.Run(() => Console.ReadLine());

                var startTime = DateTime.Now;

                 // Om användaren svarar inom 10 sekunder
                if (await Task.WhenAny(inputTask, delayTask) == inputTask)
                {
                    // Om användaren svarar inom tidsgränsen
                    cts.Cancel(); // Stoppa fördröjningen
                    string userAnswer = await inputTask;

                    // Felhantering för inmatning
                    while (true)
                    {
                         // Kontrollerar användarens svar och jämför med rätt svar
                        if (string.IsNullOrWhiteSpace(userAnswer))
                        {
                            Console.WriteLine("Du måste svara på frågan!");
                        }
                        else if (!int.TryParse(userAnswer, out int answerIndex) || answerIndex < 1 || answerIndex > question.Options.Count)
                        {
                            Console.WriteLine("Vänligen välj ett giltigt svar (1-{0}).", question.Options.Count);
                        }
                        else
                        {
                            // Uppdaterar poängen om svaret är korrekt
                            if (answerIndex - 1 == question.CorrectAnswer)
                            {
                                score++; // Öka poängen för rätt svar
                                Console.WriteLine("Rätt svar!");
                            }
                            else
                            {
                                Console.WriteLine("Fel svar.");
                            }
                            break; // Bryt ut ur loopen om svaret är giltigt
                        }
                        userAnswer = Console.ReadLine();
                    }
                }
                else
                {
                    // Om tiden går ut
                    Console.WriteLine("Tiden gick ut! Du har inte svarat på frågan.");
                }

                responseTime = DateTime.Now - startTime; // Beräkna svarstiden
            }

            return (responseTime, score); // Returnerar både svarstid och poäng
        }

        // Metod för att avgöra användarens språknivå baserat på poäng
        static string DetermineLevel(int score)
        {
            if (score >= 11)
                return "B2";
            else if (score >= 7)
                return "B1";
            else
                return "A1-A2"; // Ändra vid behov
        }
        
        // Visar alla spelares resultat från databasen i rangordning
        static void DisplayResults()
        {
            using (var db = new QuizDbContext())
            {
                var results = db.Results.OrderByDescending(r => r.Score).ToList();
                Console.WriteLine("\nResultat:");
                for (int i = 0; i < results.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {results[i].PlayerName} - {results[i].Score} poäng, Svarstid: {results[i].ResponseTime.TotalSeconds} sekunder");
                }
            }
        }
    }

// Frågeklass för att representera varje quizfråga
    class Question
    {
        public string Text { get; }
        public List<string> Options { get; }
        public int CorrectAnswer { get; }

        // Konstruktor för att skapa en ny fråga
        public Question(string text, List<string> options, int correctAnswer)
        {
            Text = text;
            Options = options;
            CorrectAnswer = correctAnswer;
        }
    }
}
