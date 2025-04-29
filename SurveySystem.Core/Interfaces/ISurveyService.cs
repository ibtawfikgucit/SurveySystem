using SurveySystem.Core.Models;

namespace SurveySystem.Core.Interfaces
{
    public interface ISurveyService
    {
        Task<IEnumerable<Survey>> GetAllSurveysAsync();
        Task<Survey> GetSurveyByIdAsync(Guid id, bool includeQuestions = false);
        Task<Survey> CreateSurveyAsync(Survey survey);
        Task UpdateSurveyAsync(Survey survey);
        Task DeleteSurveyAsync(Guid id);
        Task<IEnumerable<SurveyResponse>> GetSurveyResponsesAsync(Guid surveyId);
        Task<SurveyResponse> SubmitSurveyResponseAsync(SurveyResponse response);
    }
}