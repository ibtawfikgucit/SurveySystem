namespace SurveySystem.Core.Models
{
    public class SurveyResponse : BaseEntity
    {
        public Guid SurveyId { get; set; }
        public Guid? RespondentId { get; set; }
        public string RespondentEmail { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        // Navigation properties
        public virtual Survey Survey { get; set; }
        public virtual User Respondent { get; set; }
        public virtual ICollection<QuestionResponse> QuestionResponses { get; set; }
    }
}