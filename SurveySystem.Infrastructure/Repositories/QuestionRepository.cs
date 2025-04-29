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
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Question> GetQuestionWithOptionsAsync(Guid id)
        {
            return await _dbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IReadOnlyList<Question>> GetQuestionsBySurveyIdAsync(Guid surveyId)
        {
            return await _dbContext.Questions
                .Include(q => q.Options)
                .Where(q => q.SurveyId == surveyId)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        public async Task UpdateQuestionOrderAsync(IEnumerable<(Guid QuestionId, int DisplayOrder)> questionOrders)
        {
            foreach (var (questionId, displayOrder) in questionOrders)
            {
                var question = await _dbContext.Questions.FindAsync(questionId);
                if (question != null)
                {
                    question.DisplayOrder = displayOrder;
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}