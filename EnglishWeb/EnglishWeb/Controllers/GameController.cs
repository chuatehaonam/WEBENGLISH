using EnglishWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EnglishWeb.Controllers
{
    public class GameController : Controller
    {
        dbEnglishDataContext db = new dbEnglishDataContext();

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

        public ActionResult SelectMode(int lessonId)
        {
            ViewBag.LessonId = lessonId;
            return View();
        }

        public ActionResult Play(int lessonId, string mode = "easy")
        {
            var words = db.Vocabularies
                .Where(v => v.LessonId == lessonId)
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            if (!words.Any()) return RedirectToAction("SelectLesson");

            Session["GameWords"] = words;
            Session["LessonId"] = lessonId;
            Session["Score"] = 0;
            Session["CurrentIndex"] = 0;
            Session["Mode"] = mode.ToLower();
            Session["LastChoices"] = null;

            return RedirectToAction("Next");
        }

        public ActionResult Next()
        {
            if (Session["GameWords"] == null || Session["CurrentIndex"] == null)
                return RedirectToAction("SelectLesson");

            int index = (int)Session["CurrentIndex"];
            var words = (List<Vocabulary>)Session["GameWords"];

            if (index >= words.Count)
                return RedirectToAction("Result");

            var currentWord = words[index];
            var correctImage = db.Images.FirstOrDefault(i => i.WordId == currentWord.WordId);
            if (correctImage == null)
            {
                Session["CurrentIndex"] = index + 1;
                return RedirectToAction("Next");
            }

            var wrongImages = db.Images
                .Where(i => i.WordId != currentWord.WordId && i.WordId != null)
                .GroupBy(i => i.WordId)
                .Select(g => g.FirstOrDefault())
                .OrderBy(x => Guid.NewGuid())
                .Take(9)
                .ToList();

            if (wrongImages.Count < 9)
            {
                Session["CurrentIndex"] = index + 1;
                return RedirectToAction("Next");
            }

            var allChoices = wrongImages.Concat(new[] { correctImage })
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            Session["LastChoices"] = allChoices;
            ViewBag.CorrectImageId = correctImage.ImageId;
            ViewBag.TimeLeft = 7.0;
            ViewBag.Mode = Session["Mode"];

            var model = new GameViewModel
            {
                LessonId = (int)Session["LessonId"],
                CurrentWord = currentWord,
                Choices = allChoices,
                Score = (int)Session["Score"]
            };

            return View("Play", model);
        }

        public ActionResult Answer(int lessonId, int wordId, int imageId, string timeLeft)
        {
            var image = db.Images.FirstOrDefault(i => i.ImageId == imageId);
            var word = db.Vocabularies.FirstOrDefault(w => w.WordId == wordId);
            if (image == null || word == null)
                return RedirectToAction("Next");

            int score = (int)Session["Score"];
            var currentChoices = (List<Image>)Session["LastChoices"];
            var mode = (string)Session["Mode"];

            if (image.WordId == wordId)
            {
                if (mode == "hard")
                {
                    double.TryParse(timeLeft, out double timeRemaining);
                    int points = Math.Min(5, (int)Math.Ceiling(timeRemaining * (10.0 / 7.0)));
                    score += points;
                }
                else
                {
                    score += 5;
                }

                Session["Score"] = score;
                Session["CurrentIndex"] = (int)Session["CurrentIndex"] + 1;
                return RedirectToAction("Next");
            }
            else
            {
                score -= 1;
                Session["Score"] = score;

                var wrongs = currentChoices
                    .Where(i => i.WordId != wordId)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                foreach (var w in wrongs)
                    currentChoices.Remove(w);

                Session["LastChoices"] = currentChoices;

                ViewBag.CorrectImageId = currentChoices.FirstOrDefault(i => i.WordId == wordId)?.ImageId ?? 0;
                double.TryParse(timeLeft, out double timeLeftValue);
                ViewBag.TimeLeft = timeLeftValue;
                ViewBag.Mode = mode;

                var model = new GameViewModel
                {
                    LessonId = lessonId,
                    CurrentWord = word,
                    Choices = currentChoices,
                    Score = score
                };

                return View("Play", model);
            }
        }

        [HttpPost]
        public JsonResult AnswerAjax(int lessonId, int wordId, int imageId, string timeLeft)
        {
            var mode = (string)Session["Mode"];
            int score = (int)Session["Score"];
            var currentChoices = (List<Image>)Session["LastChoices"];

            // ❗ Nếu không chọn ảnh (imageId == 0): hết giờ => trừ 5 điểm
            if (imageId == 0)
            {
                score -= 5;
                Session["Score"] = score;
                Session["CurrentIndex"] = (int)Session["CurrentIndex"] + 1;

                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Next", "Game")
                });
            }

            var image = db.Images.FirstOrDefault(i => i.ImageId == imageId);
            var word = db.Vocabularies.FirstOrDefault(w => w.WordId == wordId);
            if (image == null || word == null)
            {
                return Json(new { success = false });
            }

            if (image.WordId == wordId)
            {
                double.TryParse(timeLeft, out double timeRemaining);
                int points = (mode == "hard")
                    ? Math.Min(5, (int)Math.Ceiling(timeRemaining * (10.0 / 7.0)))
                    : 5;

                score += points;
                Session["Score"] = score;
                Session["CurrentIndex"] = (int)Session["CurrentIndex"] + 1;

                return Json(new { success = true, redirect = Url.Action("Next", "Game") });
            }
            else
            {
                score -= 1;
                Session["Score"] = score;

                var wrongs = currentChoices
                    .Where(i => i.WordId != wordId)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                foreach (var w in wrongs)
                    currentChoices.Remove(w);

                Session["LastChoices"] = currentChoices;
                int correctId = currentChoices.FirstOrDefault(i => i.WordId == wordId)?.ImageId ?? 0;

                return Json(new
                {
                    success = true,
                    isCorrect = false,
                    score = score,
                    correctImageId = correctId,
                    choices = currentChoices.Select(i => new { i.ImageId, i.FilePath }).ToList()
                });
            }
        }

        public ActionResult Result()
        {
            ViewBag.Score = Session["Score"] ?? 0;
            return View();
        }
    }
}
