using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Core.Interfaces
{
    public interface ISurveyResponseRepository : IRepository<SurveyResponse>
    {
        Task<IReadOnlyList<SurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyId);
        Task<SurveyResponse> GetResponseWithAnswersAsync(Guid id);
        Task<int> GetResponseCountBySurveyIdAsync(Guid surveyId);
        Task<IReadOnlyList<QuestionResponse>> GetQuestionResponsesAsync(Guid questionId);
    }
}