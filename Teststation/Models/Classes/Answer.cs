namespace Teststation.Models
{
    public abstract class Answer
    {
        public long Id { get; set; }
        public string CandidateId { get; set; }
        public User Candidate { get; set; }


        public abstract bool IsCorrect();

        public abstract Question GetQuestion();
    }
}
