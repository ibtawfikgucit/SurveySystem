using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using System;
using System.Threading.Tasks;

namespace SurveySystem.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            ICurrentUserService currentUserService)
        {
            _auditLogRepository = auditLogRepository;
            _currentUserService = currentUserService;
        }

        public async Task LogActivityAsync(string action, string entityName, string entityId, string details)
        {
            var auditLog = new AuditLog
            {
                UserId = _currentUserService.GetCurrentUserId(),
                Username = _currentUserService.GetCurrentUsername(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                NewValues = details,
                IpAddress = _currentUserService.GetCurrentIpAddress()
            };

            await _auditLogRepository.AddAsync(auditLog);
        }
    }
}