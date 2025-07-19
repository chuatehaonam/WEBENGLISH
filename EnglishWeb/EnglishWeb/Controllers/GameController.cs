using EnglishWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EnglishWeb.Controllers
{
    public class GameController : Controller
    {
        EnglishLearningDataContext db = new EnglishLearningDataContext();

        public ActionResult SelectLesson()
        {
            int vocabularyTypeId = 7;

            var lessons = db.Lessons
                .Where(l => l.TypeId == vocabularyTypeId)
                .Select(l => new LessonViewModel
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    ImagePath = db.Images.FirstOrDefault(i => i.LessonId == l.LessonId) != null
                        ? db.Images.FirstOrDefault(i => i.LessonId == l.LessonId).FilePath
                        : "/Content/Images/default.jpg"
                }).ToList();

            return View(lessons);
        }

        public ActionResult PlayReorder(int lessonId)
        {
            var words = db.Vocabularies
                .Where(v => v.LessonId == lessonId)
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            if (!words.Any()) return RedirectToAction("SelectLesson");

            Session["ReorderWords"] = words;
            Session["ReorderIndex"] = 0;
            Session["ReorderScore"] = 0;

            return RedirectToAction("NextReorder");
        }

        public ActionResult NextReorder()
        {
            var words = Session["ReorderWords"] as List<Vocabulary>;
            int index = (int)Session["ReorderIndex"];

            if (index >= words.Count)
                return RedirectToAction("ResultReorder");

            var currentWord = words[index];
            var cleanWord = new string(currentWord.Word.Where(char.IsLetter).ToArray()).ToUpper();
            var shuffled = cleanWord.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray();

            var model = new GameReorderViewModel
            {
                LessonId = currentWord.LessonId,
                WordId = currentWord.WordId,
                OriginalWord = cleanWord,
                ShuffledWord = string.Join(" / ", shuffled),
                Score = (int)Session["ReorderScore"]
            };

            if (TempData["AnswerResult"] != null)
            {
                ViewBag.IsWrong = true;
                ViewBag.CorrectAnswer = TempData["AnswerResult"];
            }

            return View("PlayReorder", model);
        }


        [HttpPost]
        public ActionResult AnswerReorder(int wordId, string userAnswer)
        {
            var words = Session["ReorderWords"] as List<Vocabulary>;
            int index = (int)Session["ReorderIndex"];
            int score = (int)Session["ReorderScore"];

            var currentWord = words[index];
            var correctAnswer = new string(currentWord.Word.Where(char.IsLetter).ToArray()).ToUpper();
            var userInput = new string((userAnswer ?? "").Where(char.IsLetter).ToArray()).ToUpper();

            bool isCorrect = !string.IsNullOrWhiteSpace(userInput) &&
                             string.Equals(userInput, correctAnswer, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                score += 5;
                Session["ReorderScore"] = score;
                Session["ReorderIndex"] = index + 1;
                return RedirectToAction("NextReorder");
            }
            else
            {
                if (index > 0) score = Math.Max(0, score - 3);

                Session["ReorderScore"] = score;
                Session["ReorderIndex"] = index + 1;

                TempData["AnswerResult"] = $"❌ Sai rồi! Đáp án đúng là: <strong>{correctAnswer}</strong>";
                return RedirectToAction("NextReorder");
            }
        }


        public ActionResult ResultReorder()
        {
            ViewBag.Score = Session["ReorderScore"] ?? 0;
            return View();
        }
    }
}
