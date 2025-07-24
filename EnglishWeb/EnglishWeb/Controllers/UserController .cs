using EnglishWeb;
using EnglishWeb.Models; // namespace đúng theo project của bạn
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EnglishLearning.Controllers
{
    public class UserController : Controller
    {
        EnglishLearningDataContext db = new EnglishLearningDataContext();

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

        // Đăng nhập bằng Google
        [HttpGet]
        public ActionResult GoogleLogin(string returnUrl)
        {
            // Lưu returnUrl vào session để sử dụng sau khi callback
            Session["ReturnUrl"] = returnUrl;
            
            // Tạo URL redirect đến Google OAuth
            var clientId = System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"];
            
            // Kiểm tra xem đã cấu hình ClientId chưa
            if (string.IsNullOrEmpty(clientId) || clientId.Contains("YOUR_GOOGLE_CLIENT_ID") || clientId.Contains("PASTE_YOUR_REAL_CLIENT_ID_HERE"))
            {
                ViewBag.Error = "Google OAuth chưa được cấu hình. Vui lòng cập nhật GoogleClientId trong Web.config";
                return View("Login");
            }
            
            var redirectUri = Url.Action("GoogleCallback", "User", null, Request.Url.Scheme);
            var scope = "openid email profile";
            var responseType = "code";
            var state = Guid.NewGuid().ToString(); // Để bảo mật
            Session["GoogleState"] = state;

            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                               $"client_id={clientId}&" +
                               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                               $"scope={Uri.EscapeDataString(scope)}&" +
                               $"response_type={responseType}&" +
                               $"state={state}";

            return Redirect(googleAuthUrl);
        }

        // Callback từ Google OAuth
        [HttpGet]
        public async Task<ActionResult> GoogleCallback(string code, string state, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = "Google login was cancelled or failed.";
                return View("Login");
            }

            // Kiểm tra state để bảo mật
            var sessionState = Session["GoogleState"] as string;
            if (state != sessionState)
            {
                ViewBag.Error = "Invalid state parameter.";
                return View("Login");
            }

            try
            {
                // Lấy access token từ Google
                var tokenResponse = await GetGoogleAccessToken(code);
                if (tokenResponse == null)
                {
                    ViewBag.Error = "Failed to get access token from Google.";
                    return View("Login");
                }

                // Lấy thông tin user từ Google
                var userInfo = await GetGoogleUserInfo(tokenResponse.access_token);
                if (userInfo == null)
                {
                    ViewBag.Error = "Failed to get user information from Google.";
                    return View("Login");
                }

                // Tìm hoặc tạo user trong database
                var user = await LoginOrCreateGoogleUser(userInfo);
                if (user != null)
                {
                    Session["User"] = user;
                    TempData["Success"] = "Google login successful!";

                    // Lấy returnUrl từ session
                    var returnUrl = Session["ReturnUrl"] as string;
                    Session.Remove("ReturnUrl");
                    Session.Remove("GoogleState");

                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Vocabulary");
                }
                else
                {
                    ViewBag.Error = "Failed to create or login user.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred during Google login: " + ex.Message;
                return View("Login");
            }
        }

        // Lấy access token từ Google
        private async Task<GoogleTokenResponse> GetGoogleAccessToken(string code)
        {
            try
            {
                var clientId = System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"];
                var clientSecret = System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"];
                var redirectUri = Url.Action("GoogleCallback", "User", null, Request.Url.Scheme);

                using (var httpClient = new HttpClient())
                {
                    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
                    var parameters = new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", redirectUri)
                    };

                    tokenRequest.Content = new FormUrlEncodedContent(parameters);
                    var response = await httpClient.SendAsync(tokenRequest);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Lấy thông tin user từ Google
        private async Task<GoogleUserInfo> GetGoogleUserInfo(string accessToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<GoogleUserInfo>(responseContent);
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Đăng nhập hoặc tạo user từ Google
        private async Task<User> LoginOrCreateGoogleUser(GoogleUserInfo googleUser)
        {
            try
            {
                // Tìm user theo email
                var existingUser = db.Users.FirstOrDefault(u => u.Email == googleUser.email);
                
                if (existingUser != null)
                {
                    // User đã tồn tại, đăng nhập
                    return existingUser;
                }
                else
                {
                    // Tạo user mới
                    var newUser = new User
                    {
                        FullName = googleUser.name,
                        Email = googleUser.email,
                        PasswordHash = "GOOGLE_AUTH" // Đánh dấu đây là tài khoản Google
                    };

                    db.Users.InsertOnSubmit(newUser);
                    db.SubmitChanges();

                    return newUser;
                }
            }
            catch
            {
                return null;
            }
        }

        // GET: User/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }

    // Models cho Google OAuth response
    public class GoogleTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string id_token { get; set; }
    }

    public class GoogleUserInfo
    {
        public string id { get; set; }
        public string email { get; set; }
        public bool verified_email { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
    }
}
