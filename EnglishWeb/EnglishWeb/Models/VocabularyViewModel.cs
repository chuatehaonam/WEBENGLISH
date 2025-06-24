public class VocabularyViewModel
{
    public int WordId { get; set; }
    public int LessonId { get; set; }
    public string Word { get; set; }
    public string Definition { get; set; }
    public string Example { get; set; }
    public string PronunciationUrl { get; set; }

    public string ImagePath { get; set; } // thêm dòng này
}
