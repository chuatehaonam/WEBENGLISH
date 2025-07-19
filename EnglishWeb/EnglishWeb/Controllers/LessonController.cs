using EnglishLearningSite;
using EnglishWeb;
using EnglishWeb.Models;
using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EnglishLearningSite.Controllers
{
    public class LessonController : Controller
    {
        private EnglishLearningDataContext db = new EnglishLearningDataContext();

   
        public ActionResult About()
        {
            ViewBag.Message = " ";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "My contact page.";
            return View();
        }
        public ActionResult Index(int? typeId)
        {
      
            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<Lesson>(l => l.LessonType);
            dlo.LoadWith<Lesson>(l => l.Images);
            db.LoadOptions = dlo;

            var lessons = db.Lessons.AsQueryable();

            if (typeId.HasValue)
            {
                lessons = lessons.Where(l => l.TypeId == typeId.Value);
            }

            ViewBag.TypeId = new SelectList(db.LessonTypes, "TypeId", "TypeName", typeId);

            return View(lessons.ToList());
        }

        // GET: Lesson/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Lesson lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null)
                return HttpNotFound();

            var image = db.Images.FirstOrDefault(i => i.LessonId == id);
            ViewBag.ImagePath = image?.FilePath;

            return View(lesson);
        }

        public ActionResult Create()
        {
            ViewBag.TypeId = new SelectList(db.LessonTypes, "TypeId", "TypeName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,TypeId")] Lesson lesson, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                lesson.CreateDate = DateTime.Now;
                db.Lessons.InsertOnSubmit(lesson);
                db.SubmitChanges();

                // Lưu ảnh nếu có
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    imageFile.SaveAs(path);

                    Image img = new Image
                    {
                        FileName = fileName,
                        FilePath = "/Uploads/" + fileName,
                        LessonId = lesson.LessonId,
                        UserId = 1, // Giả sử UserId tạm thời là 1, bạn có thể lấy User hiện tại
                        UploadDate = DateTime.Now
                    };

                    db.Images.InsertOnSubmit(img);
                    db.SubmitChanges();
                }

                return RedirectToAction("Index", "Vocabulary");
            }

            ViewBag.TypeId = new SelectList(db.LessonTypes, "TypeId", "TypeName", lesson.TypeId);
            return View(lesson);
        }

        // GET: Lesson/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Lesson lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null)
                return HttpNotFound();

            ViewBag.TypeId = new SelectList(db.LessonTypes, "TypeId", "TypeName", lesson.TypeId);

            var currentImg = db.Images.FirstOrDefault(i => i.LessonId == lesson.LessonId);
            ViewBag.ImagePath = currentImg != null ? currentImg.FilePath : null;

            return View(lesson);
        }

        // POST: Lesson/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LessonId,Title,Description,TypeId")] Lesson lesson, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Lessons.FirstOrDefault(l => l.LessonId == lesson.LessonId);
                if (existing != null)
                {
                    existing.Title = lesson.Title;
                    existing.Description = lesson.Description;
                    existing.TypeId = lesson.TypeId;
                    db.SubmitChanges();

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                        imageFile.SaveAs(path);

                        Image img = db.Images.FirstOrDefault(i => i.LessonId == lesson.LessonId);
                        if (img != null)
                        {
                            img.FileName = fileName;
                            img.FilePath = "/Uploads/" + fileName;
                            img.UploadDate = DateTime.Now;
                        }
                        else
                        {
                            db.Images.InsertOnSubmit(new Image
                            {
                                FileName = fileName,
                                FilePath = "/Uploads/" + fileName,
                                UploadDate = DateTime.Now,
                                UserId = 1, // hoặc userId hiện tại
                                LessonId = lesson.LessonId
                            });
                        }
                        db.SubmitChanges();
                    }

                    return RedirectToAction("Index");
                }
            }

            // Nếu có lỗi thì giữ lại dropdown và ảnh
            ViewBag.TypeId = new SelectList(db.LessonTypes, "TypeId", "TypeName", lesson.TypeId);
            var currentImg = db.Images.FirstOrDefault(i => i.LessonId == lesson.LessonId);
            ViewBag.ImagePath = currentImg != null ? currentImg.FilePath : null;

            return View(lesson);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Lesson lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null)
                return HttpNotFound();

            ViewBag.TypeName = lesson.LessonType?.TypeName;
            return View(lesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson lesson = db.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson != null)
            {
                // ❌ 1. Xóa ảnh liên kết với Lesson
                var lessonImages = db.Images.Where(i => i.LessonId == id).ToList();
                db.Images.DeleteAllOnSubmit(lessonImages);

                // ✅ 2. Xóa từ vựng & ảnh liên quan tới từ
                var vocabularies = db.Vocabularies.Where(v => v.LessonId == id).ToList();

                foreach (var vocab in vocabularies)
                {
                    // Xóa ảnh của từ vựng
                    var wordImages = db.Images.Where(i => i.WordId == vocab.WordId).ToList();
                    db.Images.DeleteAllOnSubmit(wordImages);

                    // Xóa UserVocabularyHistory
                    var histories = db.UserVocabularyHistories.Where(h => h.WordId == vocab.WordId).ToList();
                    db.UserVocabularyHistories.DeleteAllOnSubmit(histories);

                    // Xóa từ vựng
                    db.Vocabularies.DeleteOnSubmit(vocab);
                }

                // ✅ 3. Xóa bài học
                db.Lessons.DeleteOnSubmit(lesson);
                db.SubmitChanges();
            }

            return RedirectToAction("Index", "Vocabulary");
        }


    }
}