using EnglishWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EnglishWeb.Controllers
{
    public class FlashcardsController : Controller
    {
        private dbEnglishDataContext db = new dbEnglishDataContext();

        private int GetCurrentUserId()
        {
            var user = Session["User"] as User;
            return user?.UserId ?? 0;
        }

        public ActionResult Index()
        {
            int userId = GetCurrentUserId();

            // Lấy danh sách các lịch sử ôn tập đã đến hạn của user
            var historiesToReview = db.UserVocabularyHistories
                .Where(h => h.UserId == userId && h.NextReview <= DateTime.Now)
                .OrderBy(h => h.NextReview)
                .ThenBy(h => h.Repetitions)
                .ToList();

            // Lấy danh sách từ đã từng học của user này
            var userLearnedWordIds = db.UserVocabularyHistories
                .Where(h => h.UserId == userId)
                .Select(h => h.WordId)
                .ToHashSet();

            // Lấy danh sách từ vựng chưa từng học
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
        public ActionResult ShowMeaning(int historyId, int wordId)
        {
            int userId = GetCurrentUserId();
            UserVocabularyHistory currentHistory = null;

            if (historyId != 0)
            {
                currentHistory = db.UserVocabularyHistories
                    .SingleOrDefault(h => h.HistoryId == historyId && h.UserId == userId);
            }
            else
            {
                var vocab = db.Vocabularies.SingleOrDefault(v => v.WordId == wordId);
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
            // 1. Kiểm tra đăng nhập qua Session
            var user = Session["User"] as User;
            if (user == null)
            {
                TempData["Message"] = "⚠️ Bạn cần đăng nhập để thêm từ.";
                return RedirectToAction("Login", "User");
            }

            int userId = user.UserId;

            // 2. Kiểm tra xem từ đã tồn tại trong flashcard chưa
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




        // GET: Flashcard/AddNewWord
        // GET: Flashcard/AddNewWord
        public ActionResult AddNewWord()
        {
            return View();
        }

        // POST: Flashcard/AddNewWord
        [HttpPost]
        public ActionResult AddNewWord(string word, string meaning)
        {
            // 1. Kiểm tra người dùng đã đăng nhập
            var user = Session["User"] as User;
            if (user == null)
            {
                TempData["Message"] = "⚠️ Bạn cần đăng nhập để thêm từ.";
                return RedirectToAction("Login", "User");
            }

            // 2. Kiểm tra từ đã tồn tại trong bảng Vocabulary chưa
            var existing = db.Vocabularies.FirstOrDefault(v => v.Word == word);
            if (existing == null)
            {
                // 2.1. Lấy một bài học mặc định
                var defaultLesson = db.Lessons.FirstOrDefault();
                if (defaultLesson == null)
                {
                    TempData["Message"] = "⚠️ Không có bài học nào tồn tại trong hệ thống. Vui lòng tạo ít nhất một Lesson trước.";
                    return RedirectToAction("Index");
                }

                // 2.2. Thêm từ mới vào bảng Vocabulary
                existing = new Vocabulary
                {
                    Word = word.Trim(),
                    Definition = meaning.Trim(),
                    LessonId = defaultLesson.LessonId
                };

                db.Vocabularies.InsertOnSubmit(existing);
                db.SubmitChanges(); // Để lấy WordId
            }

            // 3. Kiểm tra từ đã nằm trong flashcard của user chưa
            bool inFlashcard = db.UserVocabularyHistories
                .Any(h => h.UserId == user.UserId && h.WordId == existing.WordId);

            if (!inFlashcard)
            {
                var history = new UserVocabularyHistory
                {
                    UserId = user.UserId,
                    WordId = existing.WordId,
                    LastReviewed = DateTime.Now,
                    NextReview = DateTime.Now.AddDays(1), // Bắt đầu ôn từ ngày mai
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

            return RedirectToAction("Index"); // Trở về trang flashcard
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Đảm bảo đóng kết nối DB khi controller bị hủy
            }
            base.Dispose(disposing);
        }
    }
}
