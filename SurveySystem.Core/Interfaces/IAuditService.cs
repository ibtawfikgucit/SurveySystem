namespace SurveySystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActivityAsync(string action, string entityName, string entityId, string details);
    }
}