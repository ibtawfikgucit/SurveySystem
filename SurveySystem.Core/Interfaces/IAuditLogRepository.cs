using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int count);
        Task<IReadOnlyList<AuditLog>> GetLogsByEntityAsync(string entityName, string entityId);
        Task<IReadOnlyList<AuditLog>> GetLogsByUserAsync(string userId);
        Task<IReadOnlyList<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}