using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using EnglishWeb.Models;

namespace EnglishWeb.Controllers
{
    public class GrammarController : Controller
    {
        private readonly AIService _aiService = new AIService();

        // GET: Grammar
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // POST: Grammar
        [HttpPost]
        public async Task<ActionResult> Index(string sentence)
        {
            ViewBag.Input = sentence;

            if (string.IsNullOrWhiteSpace(sentence))
            {
                ViewBag.Error = "Vui lòng nhập một câu tiếng Anh.";
                return View();
            }

            var response = await _aiService.CheckGrammarAsyncdong(sentence);

            if (response.Success)
            {
                ViewBag.Corrected = response.Content;
            }
            else
            {
                ViewBag.Error = response.ErrorMessage;
            }

            return View();
        }
    }
}