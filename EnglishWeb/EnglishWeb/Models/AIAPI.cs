using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EnglishWeb.Models
{

    public class AIService
    {
        private readonly DeepSeekAI _deepSeekAI;

        public AIService()
        {
            _deepSeekAI = new DeepSeekAI();
        }

        public async Task<AIResponse> AskQuestionAsync(string question)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng nhập câu hỏi!" };
                }

                string response = await _deepSeekAI.AskQuestionAsync(question);
                return new AIResponse { Success = true, Content = response };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<AIResponse> TranslateAsync(string text, string direction)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng nhập văn bản cần dịch!" };
                }

                string translation;
                switch (direction.ToLower())
                {
                    case "en-vi":
                        translation = await _deepSeekAI.TranslateEnglishToVietnameseAsync(text);
                        break;
                    case "vi-en":
                        translation = await _deepSeekAI.TranslateVietnameseToEnglishAsync(text);
                        break;
                    default:
                        return new AIResponse { Success = false, ErrorMessage = "Hướng dịch không hợp lệ!" };
                }

                return new AIResponse { Success = true, Content = translation };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<List<string>> GenerateVocabularyListAsync(string topic, int numberOfWords = 10)
        {
            return await _deepSeekAI.GenerateVocabularyListAsync(topic, numberOfWords);
        }

        public async Task<DefinitionExampleResult> GenerateDefinitionAndExampleAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new DefinitionExampleResult
                {
                    Definition = "No word provided.",
                    Example = "No example."
                };
            }

            string prompt =
                $"You are an English dictionary API. Return ONLY a JSON object with two fields: 'definition' and 'example'. " +
                $"Do NOT include any explanation, markdown, or extra words. " +
                $"The output MUST be valid JSON like this:\n" +
                $"{{\n  \"definition\": \"A watermelon is a large fruit...\",\n" +
                $"  \"example\": \"We ate watermelon by the pool.\"\n}}\n" +
                $"Now give the JSON for the word: '{word}'.";

            string response = await _deepSeekAI.AskQuestionAsync(prompt);

            System.Diagnostics.Debug.WriteLine("AI RAW RESPONSE:\n" + response);

            try
            {
          
                var match = System.Text.RegularExpressions.Regex.Match(response, @"\{[\s\S]*?\}");

                if (match.Success)
                {
                    string json = match.Value;

                    var result = JsonConvert.DeserializeObject<DefinitionExampleResult>(json);

                   
                    if (!string.IsNullOrWhiteSpace(result?.Definition) && !string.IsNullOrWhiteSpace(result.Example))
                    {
                        return result;
                    }
                }

             
                throw new Exception("AI không trả về JSON hợp lệ.");
            }
            catch (Exception ex)
            {
         
                System.Diagnostics.Debug.WriteLine("ERROR parsing AI response: " + ex.Message);

     
                return new DefinitionExampleResult
                {
                    Definition = "AI error: Could not parse definition.",
                    Example = "AI error: Could not parse example."
                };
            }
        }
        public async Task<AIResponse> GenerateLessonAsync(string topic, string level = "beginner")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(topic))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng nhập chủ đề!" };
                }

                string lesson = await _deepSeekAI.GenerateEnglishResponseAsync(topic, level);
                return new AIResponse { Success = true, Content = lesson };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<AIResponse> GenerateQuizAsync(string topic, int numberOfQuestions = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(topic))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng nhập chủ đề!" };
                }

                string quiz = await _deepSeekAI.GenerateQuizAsync(topic, numberOfQuestions);
                return new AIResponse { Success = true, Content = quiz };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }


        public async Task<AIResponse> GenerateEnglishParagraphAsync()
        {
            try
            {
                string paragraph = await _deepSeekAI.GenerateEnglishParagraphAsync();
                return new AIResponse { Success = true, Content = paragraph };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

    
        public async Task<AIResponse> GenerateVietnameseParagraphAsync()
        {
            try
            {
                string paragraph = await _deepSeekAI.GenerateVietnameseParagraphAsync();
                return new AIResponse { Success = true, Content = paragraph };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }


        public async Task<AIResponse> CompareTranslationAsync(string originalText, string userTranslation, string originalType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalText) || string.IsNullOrWhiteSpace(userTranslation))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng cung cấp đầy đủ văn bản gốc và bản dịch!" };
                }

                string comparison = await _deepSeekAI.CompareTranslationAsync(originalText, userTranslation, originalType);
                return new AIResponse { Success = true, Content = comparison };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }
        public async Task<AIResponse> CheckGrammarAsyncdong(string sentence)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    return new AIResponse { Success = false, ErrorMessage = "Vui lòng nhập câu cần kiểm tra!" };
                }

                string corrected = await _deepSeekAI.CheckGrammarAsyncdong(sentence);
                return new AIResponse { Success = true, Content = corrected };
            }
            catch (Exception ex)
            {
                return new AIResponse { Success = false, ErrorMessage = ex.Message };
            }
        }
        public void Dispose()
        {
            _deepSeekAI?.Dispose();
        }
    }

    public class AIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
    }




    public class DeepSeekAI
    {
        private readonly string apiKey;
        private readonly string baseUrl;
        private readonly HttpClient httpClient;

        public DeepSeekAI()
        {

            this.apiKey = "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";
            this.baseUrl = "https://openrouter.ai/api/v1";
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            this.httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://localhost");
            this.httpClient.DefaultRequestHeaders.Add("X-Title", "EnglishWeb");
            this.httpClient.Timeout = TimeSpan.FromSeconds(30);
        }


        public async Task<string> AskQuestionAsync(string question)
        {
            System.Diagnostics.Debug.WriteLine("=== AskQuestionAsync START ===");
            System.Diagnostics.Debug.WriteLine($"Question: {question.Substring(0, Math.Min(100, question.Length))}...");

            try
            {
                var requestBody = new
                {
                    model = "openai/chatgpt-4o-latest",
                    messages = new[]
                    {
                        new { role = "user", content = question }
                    },
                    max_tokens = 300,
                    temperature = 0.3 
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"Sending request to: {baseUrl}/chat/completions");
                System.Diagnostics.Debug.WriteLine($"Request body: {jsonContent}");

                HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}/chat/completions", content);

                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Response content: {responseContent}");

                    var result = JsonConvert.DeserializeObject<DeepSeekResponse>(responseContent);

                    if (result?.choices?.Length > 0)
                    {
                        string aiResponse = result.choices[0].message.content;
                        System.Diagnostics.Debug.WriteLine($"AI Response: {aiResponse}");
                        return aiResponse;
                    }
                    System.Diagnostics.Debug.WriteLine("No choices in response");
                    return "Không nhận được phản hồi từ AI.";
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error - Status: {response.StatusCode}, Content: {errorContent}");

 
                    return GetFallbackparagraph(question);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in AskQuestionAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");


                return GetFallbackparagraph(question);
            }
        }

        private string GetFallbackparagraph(string question)
        {
            System.Diagnostics.Debug.WriteLine($"GetFallbackContent called with question containing: {question.Substring(0, Math.Min(200, question.Length))}");

        
            if (question.Contains("So sánh và chấm điểm") || question.Contains("Gốc:") || question.Contains("Dịch:") || question.Contains("so sánh bản dịch") || question.Contains("Văn bản gốc"))
            {
                System.Diagnostics.Debug.WriteLine("Detected comparison request, using comparison fallback");
                return @"Translation Analysis:

 Accuracy: Your translation demonstrates good understanding of the main content. The overall meaning is conveyed clearly and comprehensibly.

Grammar & Vocabulary: Sentence structure is stable, vocabulary is appropriate for the context. You could improve the naturalness and fluency of the sentences.

Improvement suggestions: Pay attention to using connective words and more diverse sentence structures to make your translation more natural. Read more sample texts to improve your translation style.

Overall score: 7.5/10 - Your translation is good! Keep practicing to enhance your skills.

Note: AI service is temporarily unavailable. This is a basic analysis.";
            }
            else if (question.ToLower().Contains("english") || question.ToLower().Contains("daily life") || question.ToLower().Contains("science"))
            {
                return GenerateFallbackEnglishParagraph();
            }
            else if (question.ToLower().Contains("việt") || question.ToLower().Contains("vietnamese"))
            {
                return GenerateFallbackVietnameseParagraph();
            }
            else
            {
                return "AI service is temporarily unavailable. Using fallback content.";
            }
        }

   
        public string AskQuestion(string question)
        {
            return AskQuestionAsync(question).GetAwaiter().GetResult();
        }

  
        public async Task<string> GenerateEnglishResponseAsync(string topic, string level = "beginner")
        {
            string prompt = $"Tạo một bài học tiếng Anh về chủ đề '{topic}' cho người học ở trình độ {level}. " +
                          "Bao gồm từ vựng, ví dụ và giải thích bằng tiếng Việt.";

            return await AskQuestionAsync(prompt);
        }


        public async Task<string> TranslateEnglishToVietnameseAsync(string englishText)
        {
            string prompt = $"Dịch đoạn văn sau từ tiếng Anh sang tiếng Việt: '{englishText}'";
            return await AskQuestionAsync(prompt);
        }

        public async Task<string> TranslateVietnameseToEnglishAsync(string vietnameseText)
        {
            string prompt = $"Dịch đoạn văn sau từ tiếng Việt sang tiếng Anh: '{vietnameseText}'";
            return await AskQuestionAsync(prompt);
        }

        public async Task<string> GenerateQuizAsync(string topic, int numberOfQuestions = 5)
        {
            string prompt = $"Tạo {numberOfQuestions} câu hỏi trắc nghiệm tiếng Anh về chủ đề '{topic}' " +
                          "với 4 đáp án A, B, C, D cho mỗi câu. Kèm theo đáp án đúng.";

            return await AskQuestionAsync(prompt);
        }


        public async Task<string> GenerateEnglishParagraphAsync()
        {
            string prompt = "Pick a random sentence, one short English paragraph (20 to 50 words) taken from any English–Vietnamese bilingual sentence pairs. The paragraph must be simple, interesting, and different each time. Output only the English paragraph without translation or explanation.";

            try
            {
                string result = await AskQuestionAsync(prompt);

                if (result.Contains("AI service") || result.Contains("Không nhận được"))
                {
                    return GenerateFallbackEnglishParagraph();
                }
                return result;
            }
            catch
            {
                return GenerateFallbackEnglishParagraph();
            }
        }
        public async Task<string> CheckGrammarAsyncdong(string sentence)
        {
            string prompt = "You are a strict English grammar assistant.\n\n" +
                "Please correct the following sentence and return your answer in EXACTLY this multi-line format, with each item starting on a NEW LINE:\n\n" +
                "Original: \"...\"\n" +
                "Corrected: \"...\"\n" +
                "Error Analysis: ...\n" +
                "Grammar Rule: ...\n" +
                "Application: ...\n" +
                "Conclusion: ...\n\n" +
                "Only use this exact format. Do not return everything on one line.\n\n" +
                "Here is the sentence:\n" +
                "\"" + sentence + "\"";



            return await AskQuestionAsync(prompt);
        }
       
        public async Task<string> GenerateVietnameseParagraphAsync()
        {
            string prompt = "lấy 1 đoạn ngẫu nhiên tiếng Việt ngắn gọn (từ 20 tới 50 từ), lấy từ sách song ngữ Việt - Anh, đơn giản, thú vị, chỉ xuất 1 câu tiếng Việt ngẫu nhiên, không thêm giải thích hay bản dịch.";

            try
            {
                string result = await AskQuestionAsync(prompt);
                if (result.Contains("AI service") || result.Contains("Không nhận được"))
                {
                    return GenerateFallbackVietnameseParagraph();
                }
                return result;
            }
            catch
            {
                return GenerateFallbackVietnameseParagraph();
            }
        }

        public async Task<string> CompareTranslationAsync(string originalText, string userTranslation, string originalType)
        {
            System.Diagnostics.Debug.WriteLine("=== CompareTranslationAsync START ===");
            System.Diagnostics.Debug.WriteLine($"Original: {originalText}");
            System.Diagnostics.Debug.WriteLine($"User: {userTranslation}");
            System.Diagnostics.Debug.WriteLine($"Type: {originalType}");

            string prompt = $@"Compare and score the user's translation. Return ONLY clean text without HTML tags:

Original text ({originalType}): {originalText}

User's translation: {userTranslation}

Please evaluate the user's translation based on grammar, vocabulary, fluency, and accuracy.
Score the translation on a scale from 0 to 10.
Format your response like this:

 Translation Analysis:
Accuracy: [your analysis]
Grammar & Vocabulary: [your analysis]  
Improvement suggestions: [your suggestions]
Overall score: [score]/10 - [description]

Use plain text only, no HTML tags.";

            System.Diagnostics.Debug.WriteLine($"Prompt created: {prompt}");

            try
            {
                System.Diagnostics.Debug.WriteLine("Calling AskQuestionAsync...");
                string result = await AskQuestionAsync(prompt);
                System.Diagnostics.Debug.WriteLine($"AskQuestionAsync returned: {result}");

                if (string.IsNullOrWhiteSpace(result) ||
                    result.Contains("AI service is temporarily unavailable") ||
                    result.Contains("Không nhận được phản hồi") ||
                    result.Contains("fallback"))
                {
                    System.Diagnostics.Debug.WriteLine("AI result is invalid, using fallback");
                    return WrapForTranslator(GetFallbackComparison(originalText, userTranslation, originalType));
                }

                System.Diagnostics.Debug.WriteLine("Returning AI result wrapped for translator");
                return WrapForTranslator(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in CompareTranslationAsync: {ex.Message}");
                return WrapForTranslator(GetFallbackComparison(originalText, userTranslation, originalType));
            }
        }

        private string WrapForTranslator(string content)
        {
  
            return $@"<div class='translatable-content ai-response-content' style='line-height: 1.8; padding: 10px;'>
                {content.Replace("\n", "<br>").Replace("\r", "")}
            </div>";
        }

        private string GetFallbackComparison(string originalText, string userTranslation, string originalType)
        {
            System.Diagnostics.Debug.WriteLine("=== GetFallbackComparison called ===");
            System.Diagnostics.Debug.WriteLine($"Original: {originalText}");
            System.Diagnostics.Debug.WriteLine($"User: {userTranslation}");
            System.Diagnostics.Debug.WriteLine($"Type: {originalType}");

            // Kiểm tra input
            if (string.IsNullOrWhiteSpace(originalText) || string.IsNullOrWhiteSpace(userTranslation))
            {
                return "⚠️ Thông báo: Không có đủ thông tin để so sánh. Vui lòng kiểm tra lại văn bản gốc và bản dịch của bạn.";
            }

        
            string lengthComment = "";
            string contentComment = "";
            double score = 7.5;

            int originalLength = originalText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int userLength = userTranslation.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            double lengthRatio = originalLength > 0 ? (double)userLength / originalLength : 1.0;

            if (lengthRatio > 1.8)
            {
                lengthComment = "Bản dịch khá dài so với văn bản gốc. ";
                score -= 0.7;
            }
            else if (lengthRatio > 1.4)
            {
                lengthComment = "Bản dịch hơi dài so với văn bản gốc. ";
                score -= 0.4;
            }
            else if (lengthRatio < 0.5)
            {
                lengthComment = "Bản dịch khá ngắn, có thể thiếu một số ý quan trọng. ";
                score -= 1.0;
            }
            else if (lengthRatio < 0.7)
            {
                lengthComment = "Bản dịch hơi ngắn, có thể thiếu một số chi tiết. ";
                score -= 0.6;
            }
            else
            {
                lengthComment = "Độ dài bản dịch khá phù hợp. ";
                score += 0.3;
            }

      
            if (originalType?.ToLower().Contains("english") == true)
            {
                if (userTranslation.Contains("the ") || userTranslation.Contains("a "))
                {
                    contentComment = "Có thể cần chú ý về việc dịch mạo từ tiếng Anh sang tiếng Việt. ";
                    score -= 0.2;
                }
                else
                {
                    contentComment = "Việc loại bỏ mạo từ khi dịch sang tiếng Việt là hợp lý. ";
                    score += 0.1;
                }
            }
            else if (originalType?.ToLower().Contains("vietnamese") == true)
            {
                if (!userTranslation.Contains(".") && !userTranslation.Contains("!") && !userTranslation.Contains("?"))
                {
                    contentComment = "Cần chú ý về dấu câu khi dịch sang tiếng Anh. ";
                    score -= 0.3;
                }
                else
                {
                    contentComment = "Việc sử dụng dấu câu trong tiếng Anh là phù hợp. ";
                    score += 0.2;
                }
            }

            score = Math.Max(6.0, Math.Min(9.5, score)); 

            string scoreDescription = "";
            if (score >= 9.0) scoreDescription = "excellent";
            else if (score >= 8.5) scoreDescription = "very good";
            else if (score >= 7.5) scoreDescription = "good";
            else if (score >= 6.5) scoreDescription = "adequate";
            else scoreDescription = "needs improvement";

            return $@"📝 Translation Analysis:

✅ Accuracy: {lengthComment}Your translation demonstrates good understanding of the main content. The overall meaning is conveyed clearly and comprehensibly.

📚 Grammar & Vocabulary: {contentComment}Sentence structure is stable, vocabulary is appropriate for the context. You could improve the naturalness and fluency of the sentences.

💡 Improvement suggestions: Pay attention to using connective words, idiomatic phrases, and more diverse sentence structures. Read more sample texts to improve your translation style.

🎯 Overall score: {score:F1}/10 - Your translation is {scoreDescription}! Keep practicing to enhance your translation skills.

💭 Note: AI service is temporarily unavailable. This is a basic algorithmic analysis.";
        }

        private string GenerateFallbackEnglishParagraph()
        {
            var samples = new[]
            {
                "Reading books expands our knowledge and imagination. It helps us understand different cultures and perspectives. Good books teach valuable life lessons. They improve our vocabulary and critical thinking skills. Reading regularly develops better communication abilities and emotional intelligence for personal growth and success.",
                "Science helps us understand the world around us through careful observation and experimentation. Scientific discoveries improve our daily lives with new technologies and innovations. From medicine to communication, science drives human progress. It provides solutions to global challenges and creates opportunities for future generations.",
                "Learning a new language opens doors to different cultures and exciting opportunities. It enhances cognitive abilities, improves memory, and develops problem-solving skills. Bilingual people communicate effectively across cultural boundaries. Language learning builds confidence and creates new possibilities for travel, work, and meaningful international friendships.",
                "Exercise plays a crucial role in maintaining physical and mental health. Regular physical activity strengthens muscles, improves cardiovascular health, and boosts immune system function. It also reduces stress, anxiety, and depression while increasing energy levels. Daily exercise habits contribute to better sleep quality and overall life satisfaction.",
                "Technology has transformed how we communicate, work, and learn in modern society. Digital tools connect people across vast distances instantly. Smartphones, computers, and internet access provide unlimited information and educational resources. However, balanced technology use is essential for maintaining healthy relationships and personal wellbeing.",
                "Environmental conservation is essential for protecting our planet's future. Climate change affects weather patterns, wildlife habitats, and human communities worldwide. Sustainable practices like recycling, renewable energy, and responsible consumption help preserve natural resources. Everyone can contribute to environmental protection through small daily actions and conscious choices.",
                "Music has the power to heal, inspire, and bring people together across all cultures. It stimulates brain development, improves memory, and enhances emotional expression. Playing musical instruments develops discipline, creativity, and coordination skills. Music therapy helps patients recover from illness and trauma while providing comfort and joy.",
                "Traveling broadens our horizons and teaches us about different ways of life. It challenges our assumptions and helps us grow as individuals. Experiencing new cultures develops empathy and understanding. Travel creates lasting memories and friendships. It also improves problem-solving skills and builds confidence in navigating unfamiliar situations effectively.",
                "Cooking is both an art and a practical life skill that brings people together. Preparing meals teaches patience, creativity, and attention to detail. Sharing food strengthens family bonds and cultural traditions. Home cooking promotes healthier eating habits and saves money. It also provides a relaxing outlet for stress relief and creative expression.",
                "Photography captures moments and preserves memories for future generations. It teaches us to observe the world more carefully and appreciate beauty in everyday life. Taking photos improves visual awareness and artistic skills. Digital photography makes this hobby accessible to everyone. Sharing images connects us with others and documents important life events.",
                "Gardening connects us with nature and provides fresh, healthy food. It teaches patience, responsibility, and the cycles of life. Working with soil and plants reduces stress and improves physical fitness. Gardens create beautiful spaces and support local wildlife. Growing your own vegetables saves money and ensures food quality and safety.",
                "Time management is essential for achieving personal and professional success. Planning and prioritizing tasks reduces stress and increases productivity. Good time management creates more opportunities for leisure and family time. It helps achieve goals more efficiently and builds self-discipline. Learning to manage time effectively improves overall quality of life significantly.",
                "Friendship plays a vital role in our emotional well-being and happiness. Good friends provide support during difficult times and celebrate our successes. Strong friendships are built on trust, respect, and shared experiences. Maintaining friendships requires effort and communication skills. Quality relationships contribute to better mental health and longer, more fulfilling lives.",
                "Art education develops creativity, critical thinking, and cultural awareness in students. Creating art improves fine motor skills and visual perception. Art classes provide emotional expression and stress relief opportunities. Students learn about different cultures through artistic traditions. Arts education enhances problem-solving abilities and prepares students for creative careers.",
                "Water conservation is crucial for sustaining life on Earth and protecting future generations. Simple actions like fixing leaks and taking shorter showers make significant differences. Conserving water reduces energy consumption and environmental impact. Communities must work together to protect water sources from pollution. Everyone can contribute to water preservation through mindful daily choices.",
                "Sleep is fundamental to physical health, mental clarity, and emotional stability. Quality sleep strengthens the immune system and improves memory consolidation. Regular sleep schedules help regulate body rhythms and energy levels. Poor sleep affects concentration, mood, and decision-making abilities. Establishing good sleep habits is essential for overall well-being and life satisfaction.",
                "Volunteering enriches communities while providing personal fulfillment and growth opportunities. Helping others builds empathy, leadership skills, and social connections. Volunteer work creates positive change and addresses important social issues. It provides valuable experience and skills for future career development. Service to others brings meaning and purpose to life.",
                "Public speaking skills are valuable in personal and professional settings throughout life. Practice builds confidence and improves communication abilities significantly. Effective speakers inspire, inform, and persuade audiences successfully. Overcoming fear of public speaking opens doors to leadership opportunities. These skills enhance career prospects and personal relationships in meaningful ways.",
                "Mathematics provides essential tools for understanding patterns, solving problems, and making informed decisions. Math skills are used in everyday activities like budgeting, cooking, and shopping. Mathematical thinking develops logical reasoning and analytical abilities. These concepts form the foundation for science, technology, and engineering careers. Math literacy is crucial for navigating modern society.",
                "History teaches us valuable lessons from past experiences and helps avoid repeating mistakes. Understanding historical events provides context for current global issues and conflicts. Learning about different civilizations develops cultural awareness and appreciation. History preserves important stories and achievements of human civilization. This knowledge helps us make better decisions for the future.",
                "Literature explores human experiences, emotions, and universal themes across different cultures and time periods. Reading fiction develops empathy and imagination while improving vocabulary and writing skills. Classic works provide insights into historical contexts and social issues. Literature encourages critical thinking and philosophical reflection about life's big questions and moral dilemmas.",
                "Community service builds stronger neighborhoods and addresses local needs effectively. Working together creates lasting solutions to social problems. Service projects bring diverse groups together for common causes. Helping neighbors builds trust and social connections. Active citizenship creates positive change and improves quality of life for everyone involved.",
                "Financial literacy is essential for making smart money decisions throughout life. Understanding budgeting, saving, and investing principles builds long-term security. Good financial habits start early and compound over time significantly. Managing money effectively reduces stress and creates opportunities. Financial education empowers people to achieve their dreams and goals successfully.",
                "Team sports teach cooperation, leadership, and perseverance while promoting physical fitness. Playing sports builds character and teaches important life lessons about winning and losing gracefully. Athletic participation develops discipline, time management, and goal-setting skills. Sports create friendships and school spirit while providing healthy outlets for competition and energy.",
                "Innovation drives progress and solves complex problems in creative and unexpected ways. Inventors and entrepreneurs transform ideas into products that improve people's lives. Creative thinking combines existing knowledge in new and useful combinations. Innovation requires persistence, risk-taking, and learning from failure. Breakthrough discoveries often come from questioning assumptions and exploring possibilities."
            };

            var random = new Random();
            return samples[random.Next(samples.Length)];
        }


        private string GenerateFallbackVietnameseParagraph()
        {
            var samples = new[]
            {
                "Đọc sách là thói quen tốt giúp phát triển tri thức và mở rộng tầm nhìn. Sách nuôi dưỡng tâm hồn và truyền đạt những bài học quý giá. Qua đọc, chúng ta hiểu được nhiều văn hóa khác nhau. Việc đọc thường xuyên cải thiện khả năng tư duy, giao tiếp và phát triển cảm xúc tích cực.",
                "Khoa học giúp con người hiểu rõ thế giới xung quanh qua quan sát và thí nghiệm. Nhờ nghiên cứu khoa học, chúng ta có nhiều công nghệ hữu ích. Y học, thông tin và giao thông phát triển vượt bậc. Kiến thức khoa học giải quyết vấn đề toàn cầu và tạo cơ hội cho tương lai.",
                "Học ngoại ngữ mở ra cánh cửa văn hóa và cơ hội mới. Nó giúp phát triển khả năng nhận thức và cải thiện trí nhớ. Người biết nhiều ngôn ngữ giao tiếp hiệu quả hơn. Học ngôn ngữ xây dựng sự tự tin và tạo cơ hội du lịch, làm việc cùng kết bạn quốc tế.",
                "Thể dục đóng vai trò quan trọng trong việc duy trì sức khỏe thể chất và tinh thần. Hoạt động thể chất thường xuyên tăng cường cơ bắp và hệ miễn dịch. Nó giảm căng thẳng, lo âu và tăng năng lượng sống. Tập thể dục hàng ngày cải thiện giấc ngủ và chất lượng cuộc sống.",
                "Công nghệ đã thay đổi cách chúng ta giao tiếp, làm việc và học tập. Các công cụ số kết nối mọi người trên toàn thế giới ngay lập tức. Điện thoại, máy tính và internet cung cấp vô số thông tin giáo dục. Tuy nhiên, cần sử dụng công nghệ cân bằng để duy trì mối quan hệ khỏe mạnh.",
                "Bảo vệ môi trường là nhiệm vụ thiết yếu cho tương lai hành tinh. Biến đổi khí hậu ảnh hưởng đến thời tiết và đời sống. Thực hành bền vững như tái chế và năng lượng tái tạo giúp bảo tồn tài nguyên. Mọi người đều có thể đóng góp qua những hành động nhỏ hàng ngày.",
                "Âm nhạc có sức mạnh chữa lành, truyền cảm hứng và kết nối mọi người. Nó kích thích phát triển não bộ, cải thiện trí nhớ và biểu đạt cảm xúc. Chơi nhạc cụ phát triển kỷ luật, sáng tạo và phối hợp. Liệu pháp âm nhạc giúp bệnh nhân hồi phục và mang lại niềm vui.",
                "Du lịch mở rộng tầm nhìn và dạy chúng ta về những cách sống khác nhau. Nó thách thức những giả định và giúp chúng ta trưởng thành. Trải nghiệm văn hóa mới phát triển sự đồng cảm và hiểu biết. Du lịch tạo ra những kỷ niệm và tình bạn lâu dài đáng nhớ.",
                "Nấu ăn vừa là nghệ thuật vừa là kỹ năng sống thực tế gắn kết mọi người. Chuẩn bị bữa ăn dạy sự kiên nhẫn, sáng tạo và chú ý đến chi tiết. Chia sẻ thức ăn tăng cường tình cảm gia đình và truyền thống văn hóa. Nấu ăn tại nhà khuyến khích thói quen ăn uống lành mạnh.",
                "Nhiếp ảnh ghi lại những khoảnh khắc và bảo tồn ký ức cho các thế hệ tương lai. Nó dạy chúng ta quan sát thế giới cẩn thận hơn và trân trọng vẻ đẹp trong cuộc sống hàng ngày. Chụp ảnh cải thiện nhận thức thị giác và kỹ năng nghệ thuật độc đáo.",
                "Làm vườn kết nối chúng ta với thiên nhiên và cung cấp thức ăn tươi, lành mạnh. Nó dạy sự kiên nhẫn, trách nhiệm và chu kỳ của sự sống. Làm việc với đất và cây cối giảm căng thẳng và cải thiện thể lực. Vườn tạo không gian đẹp và hỗ trợ động vật hoang dã.",
                "Quản lý thời gian là điều cần thiết để đạt được thành công cá nhân và nghề nghiệp. Lập kế hoạch và ưu tiên công việc giảm căng thẳng và tăng năng suất. Quản lý thời gian tốt tạo cơ hội cho giải trí và thời gian gia đình. Nó giúp đạt mục tiêu hiệu quả hơn.",
                "Tình bạn đóng vai trò quan trọng trong sức khỏe cảm xúc và hạnh phúc của chúng ta. Bạn tốt hỗ trợ trong thời điểm khó khăn và ăn mừng thành công. Tình bạn bền chặt được xây dựng trên niềm tin, tôn trọng và trải nghiệm chung. Duy trì tình bạn đòi hỏi nỗ lực và kỹ năng giao tiếp.",
                "Giáo dục nghệ thuật phát triển sự sáng tạo, tư duy phản biện và nhận thức văn hóa ở học sinh. Tạo ra nghệ thuật cải thiện kỹ năng vận động tinh và nhận thức thị giác. Lớp học nghệ thuật cung cấp cơ hội biểu đạt cảm xúc và giảm căng thẳng. Học sinh tìm hiểu văn hóa khác nhau qua truyền thống nghệ thuật.",
                "Tiết kiệm nước là điều quan trọng để duy trì sự sống trên Trái đất và bảo vệ các thế hệ tương lai. Những hành động đơn giản như sửa chữa rò rỉ và tắm ngắn tạo ra sự khác biệt đáng kể. Tiết kiệm nước giảm tiêu thụ năng lượng và tác động môi trường. Cộng đồng phải hợp tác để bảo vệ nguồn nước.",
                "Giấc ngủ là nền tảng cho sức khỏe thể chất, tinh thần rõ ràng và ổn định cảm xúc. Giấc ngủ chất lượng tăng cường hệ miễn dịch và cải thiện củng cố trí nhớ. Lịch ngủ đều đặn giúp điều chỉnh nhịp sinh học và mức năng lượng. Giấc ngủ kém ảnh hưởng đến khả năng tập trung, tâm trạng.",
                "Tình nguyện làm phong phú cộng đồng đồng thời cung cấp sự hoàn thiện cá nhân và cơ hội phát triển. Giúp đỡ người khác xây dựng sự đồng cảm, kỹ năng lãnh đạo và kết nối xã hội. Công việc tình nguyện tạo ra thay đổi tích cực và giải quyết các vấn đề xã hội quan trọng.",
                "Kỹ năng nói trước công chúng có giá trị trong môi trường cá nhân và nghề nghiệp suốt đời. Thực hành xây dựng sự tự tin và cải thiện khả năng giao tiếp đáng kể. Người nói hiệu quả truyền cảm hứng, thông báo và thuyết phục khán giả thành công. Vượt qua nỗi sợ nói trước công chúng mở ra cơ hội lãnh đạo.",
                "Toán học cung cấp các công cụ thiết yếu để hiểu các mẫu, giải quyết vấn đề và đưa ra quyết định sáng suốt. Kỹ năng toán học được sử dụng trong các hoạt động hàng ngày như lập ngân sách, nấu ăn và mua sắm. Tư duy toán học phát triển khả năng lý luận logic và phân tích. Những khái niệm này tạo nền tảng cho khoa học.",
                "Lịch sử dạy chúng ta những bài học quý giá từ kinh nghiệm quá khứ và giúp tránh lặp lại sai lầm. Hiểu các sự kiện lịch sử cung cấp bối cảnh cho các vấn đề và xung đột toàn cầu hiện tại. Tìm hiểu về các nền văn minh khác nhau phát triển nhận thức và sự trân trọng văn hóa.",
                "Văn học khám phá trải nghiệm con người, cảm xúc và chủ đề phổ quát qua các nền văn hóa và thời kỳ khác nhau. Đọc tiểu thuyết phát triển sự đồng cảm và trí tưởng tượng đồng thời cải thiện từ vựng và kỹ năng viết. Các tác phẩm kinh điển cung cấp cái nhìn sâu sắc về bối cảnh lịch sử.",
                "Dịch vụ cộng đồng xây dựng các khu phố mạnh mẽ hơn và giải quyết nhu cầu địa phương một cách hiệu quả. Làm việc cùng nhau tạo ra giải pháp lâu dài cho các vấn đề xã hội. Các dự án dịch vụ đưa các nhóm đa dạng lại với nhau vì những mục đích chung. Giúp đỡ hàng xóm xây dựng niềm tin.",
                "Hiểu biết tài chính là điều cần thiết để đưa ra quyết định tiền bạc thông minh suốt đời. Hiểu các nguyên tắc lập ngân sách, tiết kiệm và đầu tư xây dựng an ninh lâu dài. Thói quen tài chính tốt bắt đầu từ sớm và gộp lại theo thời gian đáng kể. Quản lý tiền hiệu quả giảm căng thẳng.",
                "Thể thao đồng đội dạy hợp tác, lãnh đạo và kiên trì đồng thời thúc đẩy thể lực. Chơi thể thao xây dựng tính cách và dạy những bài học sống quan trọng về chiến thắng và thất bại một cách duyên dáng. Tham gia thể thao phát triển kỷ luật, quản lý thời gian và kỹ năng đặt mục tiêu.",
                "Sáng tạo thúc đẩy tiến bộ và giải quyết các vấn đề phức tạp theo những cách sáng tạo và bất ngờ. Các nhà phát minh và doanh nhân biến ý tưởng thành sản phẩm cải thiện cuộc sống con người. Tư duy sáng tạo kết hợp kiến thức hiện có theo những cách mới và hữu ích. Sáng tạo đòi hỏi kiên trì, chấp nhận rủi ro."
            };

            var random = new Random();
            return samples[random.Next(samples.Length)];
        }

        public async Task<List<string>> GenerateVocabularyListAsync(string topic, int numberOfWords = 10)
        {
            string prompt = $"Liệt kê {numberOfWords} từ vựng tiếng Anh phổ biến về chủ đề '{topic}', chỉ hiển thị mỗi từ, không thêm giải thích.";

            string response = await AskQuestionAsync(prompt);

            var words = new List<string>();
            using (var reader = new StringReader(response))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string word = line.TrimStart('-', ' ', '*', '•').Trim();
                    if (!string.IsNullOrWhiteSpace(word))
                        words.Add(word);
                }
            }

            return words;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }

    public class DeepSeekResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}