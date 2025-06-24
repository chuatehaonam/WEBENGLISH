using EnglishWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnglishLearningSite.Controllers
{
    public class VocabularyController : Controller
    {
        dbEnglishDataContext db = new dbEnglishDataContext();

        // Danh sách các bài học Vocabulary
        public ActionResult Index()
        {
            int vocabularyTypeId = 7; // ID của 'Vocabulary' trong LessonType

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


        // Hiển thị chi tiết 12 từ vựng trong bài học
        public ActionResult Detail(int id)
        {
            var lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null) return HttpNotFound();

            var vocabularies = db.Vocabularies
                .Where(v => v.LessonId == id)
                .Take(12)
                .ToList();

            ViewBag.Lesson = lesson;
            return View(vocabularies);
        }

        // Thêm từ vựng
        public ActionResult Create()
        {
            var vocabularyLessons = db.Lessons
                .Where(l => l.TypeId == 7) // 7 là Vocabulary LessonType
                .ToList();

            ViewBag.Lessons = new SelectList(vocabularyLessons, "LessonId", "Title");

            return View(new Vocabulary());
        }


        [HttpPost]
        public ActionResult Create(Vocabulary vocab, HttpPostedFileBase pronunciationFile, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Lưu file âm thanh nếu có
                if (pronunciationFile != null && pronunciationFile.ContentLength > 0)
                {
                    var audioDir = Server.MapPath("~/Uploads/Audio");
                    Directory.CreateDirectory(audioDir);
                    var audioFile = Path.GetFileName(pronunciationFile.FileName);
                    var audioPath = Path.Combine(audioDir, audioFile);
                    pronunciationFile.SaveAs(audioPath);
                    vocab.PronunciationUrl = "/Uploads/Audio/" + audioFile;
                }

                // Thêm từ vựng
                db.Vocabularies.InsertOnSubmit(vocab);
                db.SubmitChanges();

                // Lưu ảnh nếu có
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
                        UserId = 1, // Hoặc lấy user đăng nhập từ Session
                        WordId = vocab.WordId
                    };

                    db.Images.InsertOnSubmit(img);
                    db.SubmitChanges();
                }

                return RedirectToAction("Detail", new { id = vocab.LessonId });
            }

            // Reload dropdown nếu có lỗi
            var lessons = db.Lessons.Where(l => l.TypeId == 7).ToList();
            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title", vocab.LessonId);

            return View(vocab);
        }


        // Sửa từ vựng
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

            // Cập nhật pronunciation
            if (pronunciationFile != null && pronunciationFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(pronunciationFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads/Audio"), fileName);
                pronunciationFile.SaveAs(path);
                vocab.PronunciationUrl = "/Uploads/Audio/" + fileName;
            }

            // Cập nhật image
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
                        UserId = 1, // hoặc lấy từ Session
                        WordId = vocab.WordId,
                        LessonId = vocab.LessonId
                    });
                }
            }

            db.SubmitChanges();
            return RedirectToAction("Detail", new { id = vocab.LessonId });
        }


        // Xóa từ vựng
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

        // Thêm từ vựng vào danh sách yêu thích
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
                UserVocabularyHistory history = new UserVocabularyHistory
                {
                    UserId = user.UserId,
                    WordId = wordId,
                    Score = 100,
                    TimesReviewed = 1,
                    LastReviewed = DateTime.Now
                };

                db.UserVocabularyHistories.InsertOnSubmit(history);
                db.SubmitChanges();

                TempData["Message"] = "✅ Từ vựng đã được thêm vào danh sách yêu thích!";
            }
            else
            {
                TempData["Message"] = "ℹ Từ vựng này đã có trong danh sách yêu thích.";
            }

            var vocab = db.Vocabularies.FirstOrDefault(v => v.WordId == wordId);
            return RedirectToAction("Detail", new { id = vocab?.LessonId });
        }

        [HttpPost]
        public ActionResult RemoveFromFavorites(int wordId)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var favorite = db.UserVocabularyHistories
                .FirstOrDefault(h => h.UserId == user.UserId && h.WordId == wordId);

            if (favorite != null)
            {
                db.UserVocabularyHistories.DeleteOnSubmit(favorite);
                db.SubmitChanges();
            }

            return RedirectToAction("Favorites");
        }


        // Hiển thị danh sách từ vựng yêu thích của user
        public ActionResult Favorites()
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var favorites = db.UserVocabularyHistories
                .Where(h => h.UserId == user.UserId && h.Score >= 80) // Ví dụ lấy các từ người học tốt
                .Select(h => h.Vocabulary)
                .ToList();

            return View(favorites);
        }

    }

    //public class LessonViewModel
    //{
    //    public int LessonId { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //    public string ImagePath { get; set; }
    //}
}
