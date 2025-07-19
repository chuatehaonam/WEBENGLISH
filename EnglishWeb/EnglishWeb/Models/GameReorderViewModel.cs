
namespace EnglishWeb.Models
{
    public class GameReorderViewModel
    {
        public int LessonId { get; set; }
        public int WordId { get; set; }
        public string ShuffledWord { get; set; }
        public string OriginalWord { get; set; }
        public int Score { get; set; }
    }
}