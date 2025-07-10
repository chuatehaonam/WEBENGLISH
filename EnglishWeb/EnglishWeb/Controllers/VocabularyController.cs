using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using EnglishWeb.Models;

namespace EnglishWeb.Controllers
{
    public class VocabularyController : Controller
    {
        private readonly AIService _aiService;

        public VocabularyController()
        {
            _aiService = new AIService();
        }

        // GET: Vocabulary/AskAI
        public ActionResult AskAI()
        {
            return View();
        }

        // GET: Vocabulary/Edit
        public ActionResult Edit()
        {
            return View();
        }

        // POST: Vocabulary/GetAIResponse
        [HttpPost]
        public async Task<JsonResult> GetAIResponse(string question)
        {
            var result = await _aiService.AskQuestionAsync(question);
            
            return Json(new { 
                success = result.Success, 
                response = result.Content, 
                error = result.ErrorMessage 
            });
        }

        // POST: Vocabulary/TranslateText
        [HttpPost]
        public async Task<JsonResult> TranslateText(string text, string direction)
        {
            var result = await _aiService.TranslateAsync(text, direction);
            
            return Json(new { 
                success = result.Success, 
                translation = result.Content, 
                error = result.ErrorMessage 
            });
        }

        // POST: Vocabulary/GenerateLesson
        [HttpPost]
        public async Task<JsonResult> GenerateLesson(string topic, string level = "beginner")
        {
            var result = await _aiService.GenerateLessonAsync(topic, level);
            
            return Json(new { 
                success = result.Success, 
                lesson = result.Content, 
                error = result.ErrorMessage 
            });
        }

        // POST: Vocabulary/GenerateQuiz
        [HttpPost]
        public async Task<JsonResult> GenerateQuiz(string topic, int numberOfQuestions = 5)
        {
            var result = await _aiService.GenerateQuizAsync(topic, numberOfQuestions);
            
            return Json(new { 
                success = result.Success, 
                quiz = result.Content, 
                error = result.ErrorMessage 
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _aiService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 