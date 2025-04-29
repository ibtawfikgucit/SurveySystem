using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Core.Interfaces
{
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<Question> GetQuestionWithOptionsAsync(Guid id);
        Task<IReadOnlyList<Question>> GetQuestionsBySurveyIdAsync(Guid surveyId);
        Task UpdateQuestionOrderAsync(IEnumerable<(Guid QuestionId, int DisplayOrder)> questionOrders);
    }
}