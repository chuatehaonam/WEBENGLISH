using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using EnglishWeb.Models;

namespace EnglishWeb.Controllers
{
    public class PragraphController : Controller
    {
        private readonly AIService _aiService;

        public PragraphController()
        {
            _aiService = new AIService();
        }

        // GET: Pragraph
        public ActionResult Index()
        {
            return View();
        }

        // POST: Pragraph/GenerateEnglishParagraph
        [HttpPost]
        public async Task<JsonResult> GenerateEnglishParagraph()
        {
            var result = await _aiService.GenerateEnglishParagraphAsync();
            
            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    content = result.Content,
                    type = "English",
                    message = "English paragraph generated successfully!"
                });
            }
            else
            {
                return Json(new { 
                    success = false, 
                    error = "An error occurred: " + result.ErrorMessage 
                });
            }
        }

        // POST: Pragraph/GenerateVietnameseParagraph
        [HttpPost]
        public async Task<JsonResult> GenerateVietnameseParagraph()
        {
            var result = await _aiService.GenerateVietnameseParagraphAsync();
            
            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    content = result.Content,
                    type = "Vietnamese",
                    message = "Đoạn văn tiếng Việt đã được tạo thành công!"
                });
            }
            else
            {
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra: " + result.ErrorMessage 
                });
            }
        }

        // POST: Pragraph/CompareTranslation
        [HttpPost]
        public async Task<JsonResult> CompareTranslation(string originalText, string userTranslation, string originalType)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"CompareTranslation called:");
                System.Diagnostics.Debug.WriteLine($"originalText: {originalText}");
                System.Diagnostics.Debug.WriteLine($"userTranslation: {userTranslation}");
                System.Diagnostics.Debug.WriteLine($"originalType: {originalType}");
                
                if (string.IsNullOrWhiteSpace(originalText) || string.IsNullOrWhiteSpace(userTranslation))
                {
                    return Json(new { 
                        success = false, 
                        error = "Dữ liệu không đầy đủ - originalText hoặc userTranslation bị thiếu" 
                    });
                }

                var result = await _aiService.CompareTranslationAsync(originalText, userTranslation, originalType);
                
                System.Diagnostics.Debug.WriteLine($"AI Service result - Success: {result.Success}");
                System.Diagnostics.Debug.WriteLine($"AI Service content: {result.Content}");
                System.Diagnostics.Debug.WriteLine($"AI Service error: {result.ErrorMessage}");
                
                if (result.Success)
                {
                    return Json(new { 
                        success = true, 
                        comparison = result.Content,
                        message = "So sánh bản dịch hoàn tất!",
                        debug = "AI service returned successfully"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        error = "Có lỗi xảy ra khi so sánh: " + result.ErrorMessage,
                        debug = "AI service returned error"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in CompareTranslation: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return Json(new { 
                    success = false, 
                    error = "Lỗi server: " + ex.Message,
                    debug = "Exception caught in controller"
                });
            }
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