namespace Teststation.Models
{
    public abstract class Answer
    {
        public long Id { get; set; }
        public long CandidateId { get; set; }
        public UserInformation Candidate { get; set; }


        public virtual bool IsCorrect()
        {
            return true;
        }

        public virtual Question GetQuestion()
        {
            return null;
        }
    }
}
