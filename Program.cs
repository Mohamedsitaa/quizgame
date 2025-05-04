using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

class QuizGame
{
    static int score = 0;
    static bool answered = false;
    static string userAnswer = "";
    static System.Timers.Timer questionTimer;

    static int currentLevel = 1;
    static int correctStreak = 0;
    static int wrongStreak = 0;
    static Dictionary<string, int> categoryStats = new();

    static void Main()
    {
        Console.WriteLine("🌟 Welcome to the AI Quiz Game!");
        Console.WriteLine("The difficulty level will be adjusted and questions suggested based on your performance.\n");

        var allQuestions = GetAllQuestions();

        Console.WriteLine("🧠 You have 10 seconds to answer each question. Let's begin...\n");

        for (int i = 0; i < 10; i++)
        {
            var question = PickQuestion(allQuestions, currentLevel);
            Console.WriteLine($"[Level: {GetLevelName(currentLevel)}] ({question.Category})");
            Console.WriteLine("Question: " + question.Text);
            answered = false;
            userAnswer = "";

            questionTimer = new System.Timers.Timer(10000);
            questionTimer.Elapsed += TimeUp;
            questionTimer.AutoReset = false;
            questionTimer.Enabled = true;

            Thread inputThread = new Thread(() =>
            {
                Console.Write("Your answer: ");
                userAnswer = Console.ReadLine()?.Trim();
                answered = true;
            });

            inputThread.Start();

            while (!answered && questionTimer.Enabled) { }

            questionTimer.Stop();

            if (!answered)
            {
                Console.WriteLine("⏰ Time's up! 😢");
                wrongStreak++;
                correctStreak = 0;
            }
            else if (userAnswer.Equals(question.Answer, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("✔️ Correct answer!\n");
                score++;
                correctStreak++;
                wrongStreak = 0;

                if (!categoryStats.ContainsKey(question.Category))
                    categoryStats[question.Category] = 0;
                categoryStats[question.Category]++;
            }
            else
            {
                Console.WriteLine($"❌ Incorrect. The correct answer is: {question.Answer}\n");
                wrongStreak++;
                correctStreak = 0;
            }

            if (correctStreak >= 2 && currentLevel < 3)
            {
                currentLevel++;
                Console.WriteLine("⬆️ Difficulty level increased!\n");
                correctStreak = 0;
            }
            else if (wrongStreak >= 2 && currentLevel > 1)
            {
                currentLevel--;
                Console.WriteLine("⬇️ Difficulty level decreased.\n");
                wrongStreak = 0;
            }
        }

        Console.WriteLine($"✅ Game Over! You scored {score} out of 10.");
        Console.WriteLine("📊 Performance by categories:");
        foreach (var cat in categoryStats)
            Console.WriteLine($" - {cat.Key}: {cat.Value} correct answers");
    }

    static void TimeUp(Object source, ElapsedEventArgs e)
    {
        questionTimer.Stop();
    }

    static Question PickQuestion(List<Question> allQuestions, int level)
    {
        string favoriteCategory = categoryStats.OrderByDescending(k => k.Value).Select(k => k.Key).FirstOrDefault();

        var filtered = allQuestions.Where(q => q.Level == level).ToList();

        if (!string.IsNullOrEmpty(favoriteCategory))
        {
            var preferred = filtered.Where(q => q.Category == favoriteCategory).ToList();
            if (preferred.Any())
                return preferred[new Random().Next(preferred.Count)];
        }

        return filtered[new Random().Next(filtered.Count)];
    }

    static string GetLevelName(int level)
    {
        return level switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => "Unknown"
        };
    }

    static List<Question> GetAllQuestions()
    {
        return new List<Question>
        {
            new Question("How many sides does a triangle have?", "3", 1, "Mathematics"),
            new Question("What is the color of the sky?", "Blue", 1, "Science"),
            new Question("What is the capital of Egypt?", "Cairo", 1, "Geography"),
            new Question("Who was the first person to land on the moon?", "Neil Armstrong", 2, "History"),
            new Question("What is the longest river in the world?", "Nile", 2, "Geography"),
            new Question("How many continents are there in the world?", "7", 2, "Geography"),
            new Question("Who invented electricity?", "Thomas Edison", 3, "Science"),
            new Question("When did World War II begin?", "1939", 3, "History"),
            new Question("What is the chemical symbol for iron?", "Fe", 3, "Science")
        };
    }
}

class Question
{
    public string Text { get; }
    public string Answer { get; }
    public int Level { get; }
    public string Category { get; }

    public Question(string text, string answer, int level, string category)
    {
        Text = text;
        Answer = answer;
        Level = level;
        Category = category;
    }
}

