using Microsoft.AspNetCore.Http;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Services;
using System.Security.Claims;

namespace SurveySystem.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActiveDirectoryService _activeDirectoryService;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryService activeDirectoryService)
        {
            _httpContextAccessor = httpContextAccessor;
            _activeDirectoryService = activeDirectoryService;
        }

        public string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier).ToString() ?? "System";
        }

        public string GetCurrentUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name).ToString() ?? "System";
        }

        public string GetCurrentIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsInRole(string role)
        {
            if (_httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false)
            {
                return true;
            }

            // If not directly in the role, check AD groups
            var username = GetCurrentUsername();
            if (!string.IsNullOrEmpty(username))
            {
                var userInfo = _activeDirectoryService.GetUserInfo(username);
                if (userInfo != null)
                {
                    if (role == "Administrator" && userInfo.IsAdmin)
                    {
                        return true;
                    }
                    else if (role == "SurveyCreator" && (userInfo.IsAdmin || userInfo.IsCreator))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}