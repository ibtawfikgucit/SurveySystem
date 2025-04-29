using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    public class IndexModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public IndexModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        public IEnumerable<SurveyDto> Surveys { get; set; } = new List<SurveyDto>();

        public async Task OnGetAsync()
        {
            var surveys = await _surveyService.GetAllSurveysAsync();
            Surveys = surveys.Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                IsPublished = s.IsPublished,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                CreatedBy = s.CreatedByUser?.DisplayName ?? "Unknown",
                CreatedAt = s.CreatedAt
            });

            // If not an admin, only show published surveys and own drafts
            //if (!_currentUserService.IsInRole("Administrator"))
            //{
            //    string username = _currentUserService.GetCurrentUsername();
            //    Surveys = Surveys.Where(s => s.IsPublished || s.CreatedBy == username);
            //}
        }

        public bool IsOwnerOrAdmin(string createdBy)
        {
            return //_currentUserService.IsInRole("Administrator") ||
                   createdBy == _currentUserService.GetCurrentUsername();
        }
    }
}
