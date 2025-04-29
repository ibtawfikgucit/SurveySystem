namespace SurveySystem.Core.Models
{
    public class Question : BaseEntity
    {
        public Guid SurveyId { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }

        // Additional properties for specific question types
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string? ValidationRegex { get; set; }

        // Navigation properties
        public virtual Survey Survey { get; set; }
        public virtual ICollection<QuestionOption> Options { get; set; }
        public virtual ICollection<QuestionResponse> Responses { get; set; }
    }
}