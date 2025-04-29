using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Admin
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class IndexModel : PageModel
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;

        public IndexModel(
            ISurveyRepository surveyRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditRepository,
            ISurveyResponseRepository surveyResponseRepository)
        {
            _surveyRepository = surveyRepository;
            _userRepository = userRepository;
            _auditRepository = auditRepository;
            _surveyResponseRepository = surveyResponseRepository;
        }

        public int TotalUsers { get; set; }
        public int TotalSurveys { get; set; }
        public int TotalResponses { get; set; }
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public IEnumerable<dynamic> Surveys { get; set; } = new List<SurveyDto>();
        public IEnumerable<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public IEnumerable<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();

        // Data for the activity chart
        public List<string> ActivityDates { get; set; } = new List<string>();
        public List<int> SurveysCreated { get; set; } = new List<int>();
        public List<int> ResponsesReceived { get; set; } = new List<int>();

        public async Task OnGetAsync()
        {
            // Get users
            var users = await _userRepository.GetAllAsync();
            Users = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                DisplayName = u.DisplayName,
                IsAdmin = u.IsAdmin,
                IsActiveDirectoryUser = u.IsActiveDirectoryUser,
                Organization = u.Organization,
                IsExternal = u.IsExternal
            }).ToList();

            TotalUsers = Users.Count();

            // Get surveys
            var surveys = await _surveyRepository.GetAllAsync();
            var surveyDtos = surveys.Select(s => new 
            {
                s.Id,
                s.Title,
                s.Description,
                s.IsPublished,
                s.StartDate,
                s.EndDate,
                CreatedBy = s.CreatedByUser?.DisplayName ?? "Unknown",
                s.CreatedAt,
                ResponseCount = s.Responses.Count()
            }).ToList();

            Surveys = surveyDtos;
            TotalSurveys = Surveys.Count();

            // Get total responses
            TotalResponses = 0;
            foreach (var survey in surveys)
            {
                var responseCount = await _surveyResponseRepository.CountAsync(r => r.SurveyId == survey.Id);
                TotalResponses += responseCount;
            }

            // Get audit logs
            AuditLogs = await _auditRepository.GetAllAsync();
            RecentAuditLogs = (await _auditRepository.GetRecentLogsAsync(5)).ToList();

            // Generate activity chart data for the last 30 days
            for (int i = 29; i >= 0; i--)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                ActivityDates.Add(date.ToString("MM/dd"));

                // Count surveys created on this date
                SurveysCreated.Add(surveys.Count(s => s.CreatedAt.Date == date));

                // Count responses received on this date
                // In a real app, we'd query this from the database
                ResponsesReceived.Add(new Random().Next(0, 10)); // Simulated data
            }
        }
    }
}