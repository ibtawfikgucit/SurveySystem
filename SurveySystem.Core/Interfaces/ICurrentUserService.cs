namespace SurveySystem.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string GetCurrentUserId();
        string GetCurrentUsername();
        string GetCurrentIpAddress();
        bool IsAuthenticated();
        bool IsInRole(string role);
    }
}