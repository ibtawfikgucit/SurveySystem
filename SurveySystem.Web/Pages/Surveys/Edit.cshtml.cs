// SurveySystem.Web/Pages/Surveys/Edit.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    [Authorize(Policy = "RequireSurveyCreatorRole")]
    public class EditModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public EditModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        [BindProperty]
        public UpdateSurveyDto Survey { get; set; }

        public IEnumerable<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

        public bool IsAdmin => _currentUserService.IsInRole("Administrator");

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var surveyEntity = await _surveyService.GetSurveyByIdAsync(id, includeQuestions: true);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            if (surveyEntity.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() && !IsAdmin)
            {
                return Forbid();
            }

            Survey = new UpdateSurveyDto
            {
                Id = surveyEntity.Id,
                Title = surveyEntity.Title,
                Description = surveyEntity.Description,
                StartDate = surveyEntity.StartDate,
                EndDate = surveyEntity.EndDate,
                IsPublished = surveyEntity.IsPublished,
                AllowAnonymous = surveyEntity.AllowAnonymous,
                RequiresAuthentication = surveyEntity.RequiresAuthentication,
                AllowMultipleResponses = surveyEntity.AllowMultipleResponses
            };

            Questions = surveyEntity.Questions?.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Description = q.Description,
                Type = q.Type.ToString(),
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options?.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    DisplayOrder = o.DisplayOrder
                }).OrderBy(o => o.DisplayOrder).ToList()
            }).OrderBy(q => q.DisplayOrder).ToList() ?? new List<QuestionDto>();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var surveyEntity = await _surveyService.GetSurveyByIdAsync(Survey.Id, includeQuestions: true);
                Questions = surveyEntity.Questions?.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Description = q.Description,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder,
                    Options = q.Options?.Select(o => new QuestionOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        DisplayOrder = o.DisplayOrder
                    }).OrderBy(o => o.DisplayOrder).ToList()
                }).OrderBy(q => q.DisplayOrder).ToList() ?? new List<QuestionDto>();

                return Page();
            }

            var survey = await _surveyService.GetSurveyByIdAsync(Survey.Id);

            if (survey == null)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() && !IsAdmin)
            {
                return Forbid();
            }

            // Update survey properties
            survey.Title = Survey.Title;
            survey.Description = Survey.Description;
            survey.StartDate = Survey.StartDate;
            survey.EndDate = Survey.EndDate;
            survey.IsPublished = Survey.IsPublished;
            survey.AllowAnonymous = Survey.AllowAnonymous;
            survey.RequiresAuthentication = Survey.RequiresAuthentication;
            survey.AllowMultipleResponses = Survey.AllowMultipleResponses;

            await _surveyService.UpdateSurveyAsync(survey);

            return RedirectToPage("./Edit", new { id = Survey.Id });
        }
    }
}