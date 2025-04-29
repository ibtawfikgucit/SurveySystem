namespace SurveySystem.Core.Models
{
    public class QuestionOption : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public string Text { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual Question Question { get; set; }
        public virtual ICollection<QuestionResponse> Responses { get; set; }
    }
}