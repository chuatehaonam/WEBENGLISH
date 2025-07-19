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
                return 0; // Return 0 instead of redirect to avoid issues with AJAX
            }
            return user.UserId;
        }

        public ActionResult Index()
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["Message"] = "⚠️ Vui lòng đăng nhập để sử dụng flashcard.";
                    return RedirectToAction("Login", "User");
                }

                // Load Vocabulary cùng lúc khi lấy UserVocabularyHistory
                DataLoadOptions dlo = new DataLoadOptions();
                dlo.LoadWith<UserVocabularyHistory>(h => h.Vocabulary);
                dlo.LoadWith<Vocabulary>(v => v.Images);
                db.LoadOptions = dlo;

                var historiesToReview = db.UserVocabularyHistories
                    .Where(h => h.UserId == userId && h.NextReview <= DateTime.Now)
                    .OrderBy(h => h.NextReview)
                    .ThenBy(h => h.Repetitions)
                    .ToList();

                // Lấy danh sách từ mới chưa học (dùng subquery thay vì HashSet)
                var newVocabularies = db.Vocabularies
                    .Where(v => !db.UserVocabularyHistories.Any(h => h.UserId == userId && h.WordId == v.WordId))
                    .Take(5)
                    .ToList();

                foreach (var vocab in newVocabularies)
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

                var finalDueWords = historiesToReview
                    .OrderBy(h => h.NextReview)
                    .ThenBy(h => h.Repetitions)
                    .Take(10) // Hiển thị tối đa 10 từ cùng lúc
                    .ToList();

                ViewBag.FlashcardList = finalDueWords;
                ViewBag.CurrentFlashcard = finalDueWords.FirstOrDefault(); // Giữ để tương thích
                ViewBag.WordsToReviewCount = historiesToReview.Count; // Tổng số từ cần ôn
                ViewBag.ShowMeaning = false;
                ViewBag.Message = finalDueWords.Count == 0 ? "🎉 Bạn đã học xong toàn bộ flashcard!" : null;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "❌ Có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> ShowMeaning(int historyId, int wordId)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "⚠️ Vui lòng đăng nhập để tiếp tục." });
                    }
                    TempData["Message"] = "⚠️ Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Login", "User");
                }

                // Load Vocabulary cùng lúc
                DataLoadOptions dlo = new DataLoadOptions();
                dlo.LoadWith<UserVocabularyHistory>(h => h.Vocabulary);
                dlo.LoadWith<Vocabulary>(v => v.Images);
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
                            if (defEx != null)
                            {
                                vocab.Definition = defEx.Definition ?? "Không có định nghĩa";
                                vocab.Example = defEx.Example ?? "Không có ví dụ";
                                db.SubmitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue
                            if (Request.IsAjaxRequest())
                            {
                                return Json(new { success = false, message = "⚠️ Không thể tải định nghĩa từ AI: " + ex.Message });
                            }
                            ViewBag.Message = "⚠️ Không thể tải định nghĩa từ AI: " + ex.Message;
                        }
                    }

                    // Return JSON for AJAX requests
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new 
                        { 
                            success = true,
                            definition = vocab.Definition ?? "Chưa có định nghĩa",
                            example = vocab.Example ?? "Chưa có ví dụ"
                        });
                    }

                    ViewBag.CurrentFlashcard = currentHistory;
                    ViewBag.ShowMeaning = true;
                }
                else
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "❌ Không tìm thấy từ này." });
                    }
                    TempData["Message"] = "❌ Không tìm thấy từ này.";
                    return RedirectToAction("Index");
                }

                var wordsToReviewCount = db.UserVocabularyHistories
                    .Where(h => h.UserId == userId && h.NextReview <= DateTime.Now)
                    .Count();
                wordsToReviewCount += Math.Min(5, db.Vocabularies
                    .Count(v => !db.UserVocabularyHistories.Any(h => h.UserId == userId && h.WordId == v.WordId)));

                ViewBag.WordsToReviewCount = wordsToReviewCount;
                return View("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "❌ Có lỗi xảy ra: " + ex.Message });
                }
                ViewBag.Message = "❌ Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult RateWord(int historyId, int wordId, int? rating)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["Message"] = "⚠️ Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Login", "User");
                }

                // Validate rating parameter
                if (!rating.HasValue)
                {
                    TempData["Message"] = "❌ Vui lòng chọn một mức đánh giá.";
                    return RedirectToAction("Index");
                }

                int ratingValue = rating.Value;
                if (ratingValue < 0 || ratingValue > 5)
                {
                    TempData["Message"] = "⚠️ Điểm đánh giá không hợp lệ (0-5).";
                    return RedirectToAction("Index");
                }

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
                            Score = 0,
                            TimesReviewed = 0
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
                    UpdateSM2Algorithm(history, ratingValue);
                    history.TimesReviewed = history.TimesReviewed + 1;
                    db.SubmitChanges();
                    
                    // Thông báo thành công với emoji phù hợp
                    string ratingMessage;
                    switch (ratingValue)
                    {
                        case 0:
                            ratingMessage = "😅 Không sao, lần sau sẽ nhớ tốt hơn!";
                            break;
                        case 1:
                            ratingMessage = "🤔 Cần ôn tập thêm nhé!";
                            break;
                        case 2:
                            ratingMessage = "😊 Tạm được, tiếp tục cố gắng!";
                            break;
                        case 3:
                            ratingMessage = "👍 Ổn đấy!";
                            break;
                        case 4:
                            ratingMessage = "😃 Tốt lắm!";
                            break;
                        case 5:
                            ratingMessage = "🎉 Xuất sắc!";
                            break;
                        default:
                            ratingMessage = "✅ Đã lưu đánh giá!";
                            break;
                    }
                    TempData["Message"] = ratingMessage;
                }
                else
                {
                    TempData["Message"] = "❌ Không tìm thấy từ để đánh giá.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Có lỗi xảy ra khi lưu đánh giá: " + ex.Message;
                return RedirectToAction("Index");
            }
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
            try
            {
                var user = Session["User"] as User;
                if (user == null)
                {
                    TempData["Message"] = "⚠️ Bạn cần đăng nhập để thêm từ.";
                    return RedirectToAction("Login", "User");
                }

                // Validation
                if (string.IsNullOrWhiteSpace(word))
                {
                    TempData["Message"] = "❌ Vui lòng nhập từ tiếng Anh.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(meaning))
                {
                    TempData["Message"] = "❌ Vui lòng nhập nghĩa của từ.";
                    return View();
                }

                word = word.Trim().ToLower();
                meaning = meaning.Trim();

                // Kiểm tra từ đã tồn tại
                var existing = db.Vocabularies.FirstOrDefault(v => v.Word.ToLower() == word);
                if (existing == null)
                {
                    var defaultLesson = db.Lessons.FirstOrDefault();
                    if (defaultLesson == null)
                    {
                        TempData["Message"] = "⚠️ Không có bài học nào tồn tại trong hệ thống.";
                        return View();
                    }

                    existing = new Vocabulary
                    {
                        Word = word,
                        Definition = meaning,
                        LessonId = defaultLesson.LessonId,
                        Example = $"Example: {word} is used in sentences."
                    };

                    db.Vocabularies.InsertOnSubmit(existing);
                    db.SubmitChanges();
                }

                // Kiểm tra từ đã có trong flashcard của user chưa
                bool inFlashcard = db.UserVocabularyHistories
                    .Any(h => h.UserId == user.UserId && h.WordId == existing.WordId);

                if (!inFlashcard)
                {
                    var history = new UserVocabularyHistory
                    {
                        UserId = user.UserId,
                        WordId = existing.WordId,
                        LastReviewed = DateTime.Now,
                        NextReview = DateTime.Now,
                        Interval = 1,
                        Repetitions = 0,
                        EasinessFactor = 2.5,
                        Score = 0,
                        TimesReviewed = 0
                    };

                    db.UserVocabularyHistories.InsertOnSubmit(history);
                    db.SubmitChanges();

                    TempData["Message"] = $"✅ Đã thêm từ '{existing.Word}' vào flashcard thành công!";
                }
                else
                {
                    TempData["Message"] = $"⚠️ Từ '{existing.Word}' đã có trong flashcard của bạn.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Có lỗi xảy ra khi thêm từ: " + ex.Message;
                return View();
            }
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
