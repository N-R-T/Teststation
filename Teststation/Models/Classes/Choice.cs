namespace Teststation.Models
{
    public sealed class Choice
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public bool Correct { get; set; }
        public long QuestionId { get; set; }
        public MultipleChoiceQuestion Question { get; set; }
    }
}
