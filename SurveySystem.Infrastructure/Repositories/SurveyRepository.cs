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
    public class SurveyRepository : Repository<Survey>, ISurveyRepository
    {
        public SurveyRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Survey> GetSurveyWithQuestionsAsync(Guid id)
        {
            return await _dbContext.Surveys
                .Include(s => s.CreatedByUser)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<Survey>> GetSurveysByUserIdAsync(Guid userId)
        {
            return await _dbContext.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.CreatedByUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Survey>> GetPublishedSurveysAsync()
        {
            return await _dbContext.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.IsPublished)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Survey>> GetActiveSurveysAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.IsPublished &&
                           s.StartDate <= now &&
                           (!s.EndDate.HasValue || s.EndDate >= now))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}