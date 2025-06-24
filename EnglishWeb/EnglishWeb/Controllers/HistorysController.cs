using EnglishWeb.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EnglishWeb.Controllers
{
    public class HistorysController : Controller
    {
        dbEnglishDataContext db = new dbEnglishDataContext();

        // GET: History
        public ActionResult Index()
        {
            // Kiểm tra nếu chưa đăng nhập
            if (Session["User"] == null)
            {
                TempData["Message"] = "⚠️ Please login to view your flashcard history.";
                return RedirectToAction("Login", "User");
            }

            // Lấy thông tin người dùng từ session
            var user = (User)Session["User"];
            int userId = user.UserId;

            // Ngày hợp lệ tối thiểu cho SQL Server (không dùng DateTime.MinValue vì sẽ gây lỗi SqlDateTime overflow)
            var minValidSqlDate = new DateTime(1753, 1, 1);

            // Truy vấn các từ mà user đã học, có lịch ôn hợp lệ
            var history = db.UserVocabularyHistories
                            .Where(h => h.UserId == userId && h.NextReview >= minValidSqlDate)
                            .OrderByDescending(h => h.NextReview)
                            .ToList();

            return View(history);
        }
    }
}
