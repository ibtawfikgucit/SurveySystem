namespace SurveySystem.Core.Models
{
    public class Survey : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public bool AllowAnonymous { get; set; }
        public bool AllowMultipleResponses { get; set; }
        public bool RequiresAuthentication { get; set; }

        // Navigation properties
        public Guid CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<SurveyResponse> Responses { get; set; }
    }
}