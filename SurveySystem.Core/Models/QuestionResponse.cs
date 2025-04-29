namespace SurveySystem.Core.Models
{
    public class QuestionResponse : BaseEntity
    {
        public Guid SurveyResponseId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public string TextResponse { get; set; }
        public int? NumericResponse { get; set; }
        public DateTime? DateResponse { get; set; }

        // Navigation properties
        public virtual SurveyResponse SurveyResponse { get; set; }
        public virtual Question Question { get; set; }
        public virtual QuestionOption SelectedOption { get; set; }
    }
}