using EnglishWeb;
using EnglishWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnglishLearningSite.Controllers
{
    public class VocabularyController : Controller
    {
        EnglishLearningDataContext db = new EnglishLearningDataContext();
        private readonly AIService _apiService = new AIService();

        public ActionResult Index()
        {
            int vocabularyTypeId = 7;
            var vocabularyLessons = db.Lessons
                .Where(l => l.TypeId == vocabularyTypeId)
                .Select(l => new
                {
                    Lesson = l,
                    Image = db.Images.FirstOrDefault(i => i.LessonId == l.LessonId)
                })
                .ToList()
                .Select(x => new LessonViewModel
                {
                    LessonId = x.Lesson.LessonId,
                    Title = x.Lesson.Title,
                    Description = x.Lesson.Description,
                    ImagePath = x.Image != null ? x.Image.FilePath : "/Content/Images/default.jpg"
                }).ToList();

            return View(vocabularyLessons);
        }

        public ActionResult Detail(int id)
        {
            var lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null) return HttpNotFound();

            var vocabularies = db.Vocabularies.Where(v => v.LessonId == id).Take(20).ToList();
            ViewBag.Lesson = lesson;
            return View(vocabularies);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateVocabularyFromAI(int lessonId)
        {
            var lesson = db.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null) return HttpNotFound();

            try
            {
                var existingWords = db.Vocabularies
                    .Where(v => v.LessonId == lessonId)
                    .Select(v => v.Word.ToLower())
                    .ToHashSet();

                var wordsFromAI = await _apiService.GenerateVocabularyListAsync(lesson.Title, 30);

                int addedCount = 0;
                foreach (var word in wordsFromAI)
                {
                    string lowerWord = word.ToLower();

                    if (existingWords.Contains(lowerWord))
                        continue;

                    var defEx = await _apiService.GenerateDefinitionAndExampleAsync(word);

                    db.Vocabularies.InsertOnSubmit(new Vocabulary
                    {
                        LessonId = lesson.LessonId,
                        Word = word,
                        Definition = defEx?.Definition ?? $"Không có định nghĩa cho '{word}'",
                        Example = defEx?.Example ?? $"Không có ví dụ cho '{word}'",
                        PronunciationUrl = null
                    });

                    existingWords.Add(lowerWord); // ✅ Cập nhật vào danh sách đã có
                    addedCount++;

                    if (addedCount >= 10) break; // Thêm tối đa 10 từ mới
                }

                db.SubmitChanges();

                if (addedCount == 0)
                    TempData["Message"] = "⚠ Không có từ vựng mới nào được thêm vì tất cả đều đã tồn tại.";
                else
                    TempData["Message"] = $"✅ Đã thêm {addedCount} từ mới bằng AI!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Lỗi AI: " + ex.Message;
            }

            return RedirectToAction("Detail", new { id = lesson.LessonId });
        }

        public ActionResult Create()
        {
            var vocabularyLessons = db.Lessons.Where(l => l.TypeId == 7).ToList();
            ViewBag.Lessons = new SelectList(vocabularyLessons, "LessonId", "Title");
            return View(new Vocabulary());
        }

        [HttpPost]
        public ActionResult Create(Vocabulary vocab, HttpPostedFileBase pronunciationFile, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (pronunciationFile != null && pronunciationFile.ContentLength > 0)
                {
                    var audioDir = Server.MapPath("~/Uploads/Audio");
                    Directory.CreateDirectory(audioDir);
                    var audioFile = Path.GetFileName(pronunciationFile.FileName);
                    var audioPath = Path.Combine(audioDir, audioFile);
                    pronunciationFile.SaveAs(audioPath);
                    vocab.PronunciationUrl = "/Uploads/Audio/" + audioFile;
                }

                db.Vocabularies.InsertOnSubmit(vocab);
                db.SubmitChanges();

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var imageDir = Server.MapPath("~/Content/Images");
                    Directory.CreateDirectory(imageDir);
                    var imageFileName = Path.GetFileName(imageFile.FileName);
                    var imagePath = Path.Combine(imageDir, imageFileName);
                    imageFile.SaveAs(imagePath);

                    var img = new Image
                    {
                        FileName = imageFileName,
                        FilePath = "/Content/Images/" + imageFileName,
                        UploadDate = DateTime.Now,
                        UserId = 1,
                        WordId = vocab.WordId
                    };

                    db.Images.InsertOnSubmit(img);
                    db.SubmitChanges();
                }

                return RedirectToAction("Detail", new { id = vocab.LessonId });
            }

            var lessons = db.Lessons.Where(l => l.TypeId == 7).ToList();
            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title", vocab.LessonId);
            return View(vocab);
        }

        public ActionResult Edit(int id)
        {
            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == id);
            if (vocab == null) return HttpNotFound();

            var image = db.Images.FirstOrDefault(i => i.WordId == vocab.WordId);
            ViewBag.ImagePath = image?.FilePath;

            return View(vocab);
        }

        [HttpPost]
        public ActionResult Edit(Vocabulary model, HttpPostedFileBase pronunciationFile, HttpPostedFileBase imageFile)
        {
            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == model.WordId);
            if (vocab == null) return HttpNotFound();

            vocab.Word = model.Word;
            vocab.Definition = model.Definition;
            vocab.Example = model.Example;

            if (pronunciationFile != null && pronunciationFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(pronunciationFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads/Audio"), fileName);
                pronunciationFile.SaveAs(path);
                vocab.PronunciationUrl = "/Uploads/Audio/" + fileName;
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads/Images"), fileName);
                imageFile.SaveAs(path);

                var img = db.Images.FirstOrDefault(i => i.WordId == vocab.WordId);
                if (img != null)
                {
                    img.FileName = fileName;
                    img.FilePath = "/Uploads/Images/" + fileName;
                    img.UploadDate = DateTime.Now;
                }
                else
                {
                    db.Images.InsertOnSubmit(new Image
                    {
                        FileName = fileName,
                        FilePath = "/Uploads/Images/" + fileName,
                        UploadDate = DateTime.Now,
                        UserId = 1,
                        WordId = vocab.WordId,
                        LessonId = vocab.LessonId
                    });
                }
            }

            db.SubmitChanges();
            return RedirectToAction("Detail", new { id = vocab.LessonId });
        }

        public ActionResult Delete(int id)
        {
            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == id);
            if (vocab == null) return HttpNotFound();

            return View(vocab);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == id);
            if (vocab == null) return HttpNotFound();

            int lessonId = vocab.LessonId;
            db.Vocabularies.DeleteOnSubmit(vocab);
            db.SubmitChanges();
            return RedirectToAction("Detail", new { id = lessonId });
        }

        [HttpPost]
        public ActionResult AddToFavorites(int wordId)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để sử dụng chức năng yêu thích.";
                return RedirectToAction("Login", "User");
            }

            var existing = db.UserVocabularyHistories
                .FirstOrDefault(h => h.UserId == user.UserId && h.WordId == wordId);

            if (existing == null)
            {
                db.UserVocabularyHistories.InsertOnSubmit(new UserVocabularyHistory
                {
                    UserId = user.UserId,
                    WordId = wordId,
                    Score = 100,
                    TimesReviewed = 1,
                    LastReviewed = DateTime.Now
                });
                db.SubmitChanges();
                TempData["Message"] = "✅ Từ vựng đã được thêm vào danh sách yêu thích!";
            }
            else
            {
                TempData["Message"] = "⚠ Từ vựng này đã có trong danh sách yêu thích.";
            }

            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == wordId);
            return RedirectToAction("Detail", new { id = vocab?.LessonId });
        }

        [HttpPost]
        public ActionResult RemoveFromFavorites(int wordId)
        {
            var user = Session["User"] as User;
            if (user == null) return RedirectToAction("Login", "User");

            var favorite = db.UserVocabularyHistories
                .FirstOrDefault(h => h.UserId == user.UserId && h.WordId == wordId);

            if (favorite != null)
            {
                db.UserVocabularyHistories.DeleteOnSubmit(favorite);
                db.SubmitChanges();
            }

            return RedirectToAction("Favorites");
        }

        public ActionResult Favorites()
        {
            var user = Session["User"] as User;
            if (user == null) return RedirectToAction("Login", "User");

            var favorites = db.UserVocabularyHistories
                .Where(h => h.UserId == user.UserId && h.Score >= 80)
                .Select(h => h.Vocabulary)
                .ToList();

            return View(favorites);
        }
    }
}