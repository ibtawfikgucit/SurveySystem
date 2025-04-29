namespace SurveySystem.Core.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; } // Create, Update, Delete
        public DateTime Timestamp { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string IpAddress { get; set; }
    }
}