using EnglishLearningSite;
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
        private dbEnglishDataContext db = new dbEnglishDataContext();

        // GET: Lesson
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
            // Load LessonType và Images cùng lúc (eager load)
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
                // Xóa ảnh liên kết
                var images = db.Images.Where(i => i.LessonId == id).ToList();
                foreach (var img in images)
                {
                    db.Images.DeleteOnSubmit(img);
                }

                // Xóa từ vựng liên kết
                var vocabularies = db.Vocabularies.Where(v => v.LessonId == id).ToList();
                foreach (var vocab in vocabularies)
                {
                    db.Vocabularies.DeleteOnSubmit(vocab);
                }

                // Sau đó xóa bài học
                db.Lessons.DeleteOnSubmit(lesson);
                db.SubmitChanges();
            }

            return RedirectToAction("Index", "Vocabulary");
        }

    }
}
