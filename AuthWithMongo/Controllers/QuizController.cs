using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AuthWithMongo.Models;
using AuthWithMongo.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

public class QuizController : Controller
{
    private readonly MongoDbContext _context;

    public QuizController(MongoDbContext context)
    {
        _context = context;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_AdminLayout.cshtml";
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_UserLayout.cshtml";
            }
        }
        else
        {
            ViewData["Layout"] = "~/Views/Shared/_Layout.cshtml"; // fallback for unauthenticated access
        }

        base.OnActionExecuting(context);
    }

    [HttpGet]
    public async Task<IActionResult> Start(string id)
    {
        var quiz = await _context.Quizzes.Find(q => q.Id == id).FirstOrDefaultAsync();
        var shuffled = quiz.Questions.OrderBy(q => Guid.NewGuid()).ToList();
        if (quiz == null) return NotFound();

        var viewModel = new QuizAttemptViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            TimeLimitSeconds = quiz.TimeLimitSeconds,
            Questions = quiz.Questions.Select((q, i) => new QuestionViewModel
            {
                Text = q.Text,
                Options = q.Options,
                Index = i
            }).ToList(),
            Answers = new List<int>(new int[quiz.Questions.Count]) // initializes with 0s
        };

        return View("QuizPage", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitQuiz(QuizAttemptViewModel attempt)
    {
        var quiz = await _context.Quizzes.Find(q => q.Id == attempt.QuizId).FirstOrDefaultAsync();
        if (quiz == null) return NotFound();

        var answers = attempt.Answers ?? new List<int>();
        int correct = 0;

        for (int i = 0; i < quiz.Questions.Count; i++)
        {
            int selected = (i < answers.Count) ? answers[i] : -1; // -1 means no answer chosen

            if (selected >= 0 && selected == quiz.Questions[i].CorrectIndex)
                correct++;
        }

        var attemptRecord = new QuizAttempt
        {
            UserEmail = User.Identity.Name,
            QuizId = quiz.Id,
            Score = correct,
            Total = quiz.Questions.Count,
            AttemptedAt = DateTime.UtcNow
        };

        await _context.QuizAttempts.InsertOneAsync(attemptRecord);

        ViewBag.Score = correct;
        ViewBag.Total = quiz.Questions.Count;

        return View("QuizResult");
    }

    [HttpGet]
    public async Task<IActionResult> AllQuizzes()
    {
        var quizzes = await _context.Quizzes.Find(_ => true).ToListAsync();
        return View(quizzes);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyAttempts()
    {
        var userEmail = User.Identity.Name;
        var attempts = await _context.QuizAttempts
            .Find(a => a.UserEmail == userEmail)
            .SortByDescending(a => a.AttemptedAt)
            .ToListAsync();

        var quizIds = attempts.Select(a => a.QuizId).Distinct().ToList();

        var quizzes = await _context.Quizzes
            .Find(q => quizIds.Contains(q.Id))
            .ToListAsync();

        var groupedAttempts = quizIds.Select(id => new QuizAttemptStatsViewModel
        {
            QuizTitle = quizzes.FirstOrDefault(q => q.Id == id)?.Title ?? "Unknown Quiz",
            Attempts = attempts.Where(a => a.QuizId == id).ToList()
        }).ToList();

        return View(groupedAttempts);
    }


    //This displays all the users
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageQuiz()
    {
        var quizzes = await _context.Quizzes.Find(_ => true).ToListAsync();
        return View(quizzes);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateQuiz(IFormFile quizImage, string title, int timeLimit, List<Question> questions)
    {
        if (quizImage == null || quizImage.Length == 0)
        {
            ModelState.AddModelError("", "Please upload an image.");
            return View(); // Or return to ManageQuiz with error
        }

        // Save the uploaded image to wwwroot/images
        var imageName = Path.GetFileName(quizImage.FileName);
        var imagePath = Path.Combine("wwwroot/images", imageName);
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await quizImage.CopyToAsync(stream);
        }

        var quiz = new Quiz
        {
            Title = title,
            TimeLimitSeconds = timeLimit,
            ImageUrl = "~/images/" + imageName,
            Questions = questions.Select(q => new Question
            {
                Text = q.Text,
                Options = q.Options,
                CorrectIndex = q.CorrectIndex
            }).ToList()
        };

        await _context.Quizzes.InsertOneAsync(quiz);
        return RedirectToAction("ManageQuiz");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteQuiz(string id)
    {
        await _context.Quizzes.DeleteOneAsync(q => q.Id == id);
        return RedirectToAction("ManageQuiz");
    }
}