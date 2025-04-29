using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Core.Interfaces
{
    public interface ISurveyRepository : IRepository<Survey>
    {
        Task<Survey> GetSurveyWithQuestionsAsync(Guid id);
        Task<IReadOnlyList<Survey>> GetSurveysByUserIdAsync(Guid userId);
        Task<IReadOnlyList<Survey>> GetPublishedSurveysAsync();
        Task<IReadOnlyList<Survey>> GetActiveSurveysAsync();
    }
}