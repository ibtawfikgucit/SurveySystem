using Microsoft.EntityFrameworkCore;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Infrastructure.Repositories
{
    public class SurveyResponseRepository : Repository<SurveyResponse>, ISurveyResponseRepository
    {
        public SurveyResponseRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<SurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyId)
        {
            return await _dbContext.SurveyResponses
                .Include(r => r.QuestionResponses)
                    .ThenInclude(qr => qr.Question)
                .Include(r => r.QuestionResponses)
                    .ThenInclude(qr => qr.SelectedOption)
                .Where(r => r.SurveyId == surveyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<SurveyResponse> GetResponseWithAnswersAsync(Guid id)
        {
            return await _dbContext.SurveyResponses
                .Include(r => r.QuestionResponses)
                    .ThenInclude(qr => qr.Question)
                .Include(r => r.QuestionResponses)
                    .ThenInclude(qr => qr.SelectedOption)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<int> GetResponseCountBySurveyIdAsync(Guid surveyId)
        {
            return await _dbContext.SurveyResponses
                .CountAsync(r => r.SurveyId == surveyId);
        }

        public async Task<IReadOnlyList<QuestionResponse>> GetQuestionResponsesAsync(Guid questionId)
        {
            return await _dbContext.QuestionResponses
                .Where(qr => qr.QuestionId == questionId)
                .ToListAsync();
        }
    }
}