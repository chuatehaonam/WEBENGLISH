//using EnglishWeb.Models;
//using System;
//using System.Linq;
//using System.Net;
//using System.Web.Mvc;

//namespace EnglishLearningSite.Controllers
//{
//    public class VocabularyController : Controller
//    {
//        private dbEnglishDataContext db = new dbEnglishDataContext();

//        // GET: Vocabulary/Edit/5
//        public ActionResult Edit(int? id)
//        {
//            if (id == null)
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

//            Vocabulary vocabulary = db.Vocabularies.FirstOrDefault(v => v.WordId == id);
//            if (vocabulary == null)
//                return HttpNotFound();

//            // Lấy TypeId của bài học hiện tại để chọn LessonType đúng
//            int? currentTypeId = null;
//            if (vocabulary.LessonId != null)
//            {
//                var lesson = db.Lessons.FirstOrDefault(l => l.LessonId == vocabulary.LessonId);
//                if (lesson != null)
//                    currentTypeId = lesson.TypeId;
//            }

//            ViewBag.LessonTypes = new SelectList(db.LessonTypes, "TypeId", "TypeName", currentTypeId);

//            if (currentTypeId != null)
//            {
//                ViewBag.Lessons = new SelectList(db.Lessons.Where(l => l.TypeId == currentTypeId), "LessonId", "Title", vocabulary.LessonId);
//            }
//            else
//            {
//                ViewBag.Lessons = new SelectList(Enumerable.Empty<SelectListItem>());
//            }

//            return View(vocabulary);
//        }

//        // POST: Vocabulary/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit([Bind(Include = "WordId,Word,Definition,Example,PronunciationUrl,LessonId")] Vocabulary vocabulary)
//        {
//            if (ModelState.IsValid)
//            {
//                var existing = db.Vocabularies.FirstOrDefault(v => v.WordId == vocabulary.WordId);
//                if (existing != null)
//                {
//                    existing.Word = vocabulary.Word;
//                    existing.Definition = vocabulary.Definition;
//                    existing.Example = vocabulary.Example;
//                    existing.PronunciationUrl = vocabulary.PronunciationUrl;
//                    existing.LessonId = vocabulary.LessonId;

//                    db.SubmitChanges();
//                    return RedirectToAction("Index");
//                }
//                return HttpNotFound();
//            }

//            // Nếu lỗi, load lại dropdown
//            int? currentTypeId = null;
//            if (vocabulary.LessonId != null)
//            {
//                var lesson = db.Lessons.FirstOrDefault(l => l.LessonId == vocabulary.LessonId);
//                if (lesson != null)
//                    currentTypeId = lesson.TypeId;
//            }

//            ViewBag.LessonTypes = new SelectList(db.LessonTypes, "TypeId", "TypeName", currentTypeId);

//            if (currentTypeId != null)
//            {
//                ViewBag.Lessons = new SelectList(db.Lessons.Where(l => l.TypeId == currentTypeId), "LessonId", "Title", vocabulary.LessonId);
//            }
//            else
//            {
//                ViewBag.Lessons = new SelectList(Enumerable.Empty<SelectListItem>());
//            }

//            return View(vocabulary);
//        }

//        // Ajax call để lấy danh sách Lesson theo TypeId
//        public JsonResult GetLessonsByType(int typeId)
//        {
//            var lessons = db.Lessons
//                .Where(l => l.TypeId == typeId)
//                .Select(l => new { l.LessonId, l.Title })
//                .ToList();
//            return Json(lessons, JsonRequestBehavior.AllowGet);
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                db.Dispose();
//            }
//            base.Dispose(disposing);
//        }
//    }
//}
