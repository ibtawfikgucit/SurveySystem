namespace SurveySystem.Core.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActiveDirectoryUser { get; set; }
        public string Organization { get; set; }
        public bool IsExternal { get; set; }

        // Navigation properties
        public virtual ICollection<Survey> CreatedSurveys { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }
}