using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Shared.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    //[Authorize(Policy = "RequireAdministratorRole")]
    public class DeleteModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public DeleteModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        [BindProperty]
        public SurveyDto Survey { get; set; }

        public int QuestionCount { get; set; }
        public int ResponseCount { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var surveyEntity = await _surveyService.GetSurveyByIdAsync(id, includeQuestions: true);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            Survey = new SurveyDto
            {
                Id = surveyEntity.Id,
                Title = surveyEntity.Title,
                Description = surveyEntity.Description,
                IsPublished = surveyEntity.IsPublished,
                StartDate = surveyEntity.StartDate,
                EndDate = surveyEntity.EndDate,
                CreatedBy = surveyEntity.CreatedByUser?.DisplayName ?? "Unknown",
                CreatedAt = surveyEntity.CreatedAt
            };

            QuestionCount = surveyEntity.Questions?.Count() ?? 0;

            var responses = await _surveyService.GetSurveyResponsesAsync(id);
            ResponseCount = responses.Count();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Survey.Id == Guid.Empty)
            {
                return NotFound();
            }

            await _surveyService.DeleteSurveyAsync(Survey.Id);

            return RedirectToPage("./Index");
        }
    }
}