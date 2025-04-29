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
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int count)
        {
            return await _dbContext.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> GetLogsByEntityAsync(string entityName, string entityId)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> GetLogsByUserAsync(string userId)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}