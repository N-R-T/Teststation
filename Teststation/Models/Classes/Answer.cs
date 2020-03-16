namespace Teststation.Models
{
    public abstract class Answer
    {
        public long Id { get; set; }
        public long CandidateId { get; set; }
        public UserInformation Candidate { get; set; }


        public abstract bool IsCorrect();

        public abstract Question GetQuestion();
    }
}
