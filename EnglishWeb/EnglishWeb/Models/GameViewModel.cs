// File: Models/GameViewModel.cs
using System.Collections.Generic;
using EnglishWeb.Models;

namespace EnglishWeb.Models
{
    public class GameViewModel
    {
        public int LessonId { get; set; }
        public Vocabulary CurrentWord { get; set; }
        public List<Image> Choices { get; set; }
        public int Score { get; set; }
    }
}
