using EnglishWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnglishWeb.Controllers
{
    public class FlashcardsController : Controller
    {
        private EnglishLearningDataContext db = new EnglishLearningDataContext();
        private readonly AIService _apiService = new AIService();

        private int GetCurrentUserId()
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                Response.Redirect("~/User/Login");
                return 0; // tránh lỗi compile
            }
            return user.UserId;
        }

        public ActionResult Index()
        {
            int userId = GetCurrentUserId();

            // Load Vocabulary cùng lúc khi lấy UserVocabularyHistory
            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<UserVocabularyHistory>(h => h.Vocabulary);
            db.LoadOptions = dlo;

            var historiesToReview = db.UserVocabularyHistories
                .Where(h => h.UserId == userId && h.NextReview <= DateTime.Now)
                .OrderBy(h => h.NextReview)
                .ThenBy(h => h.Repetitions)
                .ToList();

            var userLearnedWordIds = db.UserVocabularyHistories
                .Where(h => h.UserId == userId)
                .Select(h => h.WordId)
                .ToHashSet();

            var allVocabularies = db.Vocabularies.ToList();
            foreach (var vocab in allVocabularies)
            {
                if (!userLearnedWordIds.Contains(vocab.WordId))
                {
                    historiesToReview.Add(new UserVocabularyHistory
                    {
                        UserId = userId,
                        WordId = vocab.WordId,
                        Vocabulary = vocab,
                        LastReviewed = DateTime.MinValue,
                        NextReview = DateTime.Now,
                        Interval = 0,
                        Repetitions = 0,
                        EasinessFactor = 2.5,
                        Score = 0
                    });
                }
            }

            var finalDueWords = historiesToReview
                .OrderBy(h => h.NextReview)
                .ThenBy(h => h.Repetitions)
                .ToList();

            ViewBag.CurrentFlashcard = finalDueWords.FirstOrDefault();
            ViewBag.WordsToReviewCount = finalDueWords.Count;
            ViewBag.ShowMeaning = false;
            ViewBag.Message = finalDueWords.Count == 0 ? "🎉 Bạn đã học xong toàn bộ flashcard!" : null;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ShowMeaning(int historyId, int wordId)
        {
            int userId = GetCurrentUserId();

            // Load Vocabulary cùng lúc
            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<UserVocabularyHistory>(h => h.Vocabulary);
            db.LoadOptions = dlo;

            UserVocabularyHistory currentHistory = null;

            if (historyId != 0)
            {
                currentHistory = db.UserVocabularyHistories
                    .FirstOrDefault(h => h.HistoryId == historyId && h.UserId == userId);
            }
            else
            {
                var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == wordId);
                if (vocab != null)
                {
                    currentHistory = new UserVocabularyHistory
                    {
                        UserId = userId,
                        WordId = vocab.WordId,
                        Vocabulary = vocab,
                        LastReviewed = DateTime.MinValue,
                        NextReview = DateTime.Now,
                        Interval = 0,
                        Repetitions = 0,
                        EasinessFactor = 2.5,
                        Score = 0
                    };
                }
            }

            if (currentHistory != null)
            {
                // 🧠 Gọi AI nếu định nghĩa hoặc ví dụ chưa đúng
                var vocab = currentHistory.Vocabulary;
                if (string.IsNullOrWhiteSpace(vocab.Definition) ||
                    vocab.Definition.StartsWith("Definition of") ||
                    string.IsNullOrWhiteSpace(vocab.Example) ||
                    vocab.Example.StartsWith("Example using"))
                {
                    try
                    {
                        var defEx = await _apiService.GenerateDefinitionAndExampleAsync(vocab.Word);
                        vocab.Definition = defEx?.Definition ?? "Không có định nghĩa";
                        vocab.Example = defEx?.Example ?? "Không có ví dụ";
                        db.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "❌ AI Error: " + ex.Message;
                    }
                }

                ViewBag.CurrentFlashcard = currentHistory;
                ViewBag.ShowMeaning = true;
            }

            var wordsToReviewCount = db.UserVocabularyHistories
                .Where(h => h.UserId == userId && h.NextReview <= DateTime.Now)
                .Count();
            wordsToReviewCount += db.Vocabularies
                .Count(v => !db.UserVocabularyHistories.Any(h => h.UserId == userId && h.WordId == v.WordId));

            ViewBag.WordsToReviewCount = wordsToReviewCount;
            return View("Index");
        }


        [HttpPost]
        public ActionResult RateWord(int historyId, int wordId, int rating)
        {
            int userId = GetCurrentUserId();
            UserVocabularyHistory history = null;

            if (historyId == 0)
            {
                var vocab = db.Vocabularies.SingleOrDefault(v => v.WordId == wordId);
                if (vocab != null)
                {
                    history = new UserVocabularyHistory
                    {
                        UserId = userId,
                        WordId = wordId,
                        LastReviewed = DateTime.MinValue,
                        NextReview = DateTime.Now,
                        Interval = 0,
                        Repetitions = 0,
                        EasinessFactor = 2.5,
                        Score = 0
                    };
                    db.UserVocabularyHistories.InsertOnSubmit(history);
                }
            }
            else
            {
                history = db.UserVocabularyHistories
                    .SingleOrDefault(h => h.HistoryId == historyId && h.UserId == userId);
            }

            if (history != null)
            {
                UpdateSM2Algorithm(history, rating);
                db.SubmitChanges();
            }

            return RedirectToAction("NextFlashcard");
        }

        public ActionResult NextFlashcard()
        {
            return RedirectToAction("Index");
        }

        private void UpdateSM2Algorithm(UserVocabularyHistory history, int quality)
        {
            double ef = history.EasinessFactor ?? 2.5;
            int repetitions = history.Repetitions ?? 0;
            int previousInterval = history.Interval ?? 1;

            if (quality < 3)
            {
                repetitions = 0;
                ef = Math.Max(1.3, ef - 0.2);
                history.Interval = 1;
            }
            else
            {
                repetitions++;
                if (repetitions == 1)
                    history.Interval = 1;
                else if (repetitions == 2)
                    history.Interval = 6;
                else
                    history.Interval = (int)Math.Round(previousInterval * ef);

                ef += (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
                ef = Math.Max(1.3, Math.Min(2.5, ef));
            }

            history.Repetitions = repetitions;
            history.EasinessFactor = ef;
            history.LastReviewed = DateTime.Now;
            history.NextReview = history.LastReviewed.AddDays(history.Interval.Value);
            history.Score = quality;
        }

        [HttpPost]
        public ActionResult AddToFlashcard(int wordId)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                TempData["Message"] = "⚠️ Bạn cần đăng nhập để thêm từ.";
                return RedirectToAction("Login", "User");
            }

            int userId = user.UserId;

            bool exists = db.UserVocabularyHistories
                             .Any(x => x.UserId == userId && x.WordId == wordId);

            if (!exists)
            {
                var history = new UserVocabularyHistory
                {
                    UserId = userId,
                    WordId = wordId,
                    LastReviewed = DateTime.Now,
                    Score = 0,
                    TimesReviewed = 0,
                    EasinessFactor = 2.5,
                    Interval = 1,
                    NextReview = DateTime.Now.AddDays(1),
                    Repetitions = 0
                };

                db.UserVocabularyHistories.InsertOnSubmit(history);
                db.SubmitChanges();

                TempData["Message"] = "✅ Đã thêm vào flashcard!";
            }
            else
            {
                TempData["Message"] = "⚠️ Từ này đã tồn tại trong flashcard.";
            }

            return RedirectToAction("Index", "History");
        }

        public ActionResult AddNewWord()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddNewWord(string word, string meaning)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                TempData["Message"] = "⚠️ Bạn cần đăng nhập để thêm từ.";
                return RedirectToAction("Login", "User");
            }

            var existing = db.Vocabularies.FirstOrDefault(v => v.Word == word);
            if (existing == null)
            {
                var defaultLesson = db.Lessons.FirstOrDefault();
                if (defaultLesson == null)
                {
                    TempData["Message"] = "⚠️ Không có bài học nào tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                existing = new Vocabulary
                {
                    Word = word.Trim(),
                    Definition = meaning.Trim(),
                    LessonId = defaultLesson.LessonId
                };

                db.Vocabularies.InsertOnSubmit(existing);
                db.SubmitChanges();
            }

            bool inFlashcard = db.UserVocabularyHistories
                .Any(h => h.UserId == user.UserId && h.WordId == existing.WordId);

            if (!inFlashcard)
            {
                var history = new UserVocabularyHistory
                {
                    UserId = user.UserId,
                    WordId = existing.WordId,
                    LastReviewed = DateTime.Now,
                    NextReview = DateTime.Now.AddDays(1),
                    Interval = 1,
                    Repetitions = 0,
                    EasinessFactor = 2.5,
                    Score = 0,
                    TimesReviewed = 0
                };

                db.UserVocabularyHistories.InsertOnSubmit(history);
                db.SubmitChanges();

                TempData["Message"] = "✅ Đã thêm từ vào flashcard!";
            }
            else
            {
                TempData["Message"] = "⚠️ Từ này đã có trong flashcard của bạn.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
