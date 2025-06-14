using EnglishWeb.Models; // namespace đúng theo project của bạn
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace EnglishLearning.Controllers
{
    public class UserController : Controller
    {
        dbEnglishDataContext db = new dbEnglishDataContext();

        // GET: User/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        public ActionResult Register(FormCollection collection, User u)
        {
            var fullName = collection["FullName"];
            var email = collection["Email"];
            var password = collection["Password"];
            var confirmPassword = collection["ConfirmPassword"];

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewData["Error"] = "Please fill in all required fields.";
            }
            else if (password != confirmPassword)
            {
                ViewData["Error"] = "Passwords do not match.";
            }
            else if (db.Users.Any(x => x.Email == email))
            {
                ViewData["Error"] = "Email already registered.";
            }
            else
            {
                u.FullName = fullName;
                u.Email = email;
                u.PasswordHash = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");

                db.Users.InsertOnSubmit(u);
                db.SubmitChanges();

                return RedirectToAction("Login");
            }

            return View();
        }

        // GET: User/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: User/Login
        [HttpPost]
        public ActionResult Login(FormCollection collection, string returnUrl)
        {
            var email = collection["Email"];
            var password = collection["Password"];
            var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");

            User user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                Session["User"] = user;
                TempData["Success"] = "Login successful!";

                // Nếu có returnUrl, chuyển hướng người dùng đến đúng trang trước khi login
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Vocabulary");
            }
            else
            {
                ViewBag.Error = "Invalid email or password.";
                ViewBag.ReturnUrl = returnUrl; // giữ lại returnUrl nếu login sai
                return View();
            }
        }

        // GET: User/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }
}
