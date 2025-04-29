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
    public class PreviewModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public PreviewModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        public SurveyDetailDto Survey { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var surveyEntity = await _surveyService.GetSurveyByIdAsync(id, includeQuestions: true);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            bool isCreator = surveyEntity.CreatedByUserId.ToString() == _currentUserService.GetCurrentUserId();
            bool isAdmin = _currentUserService.IsInRole("Administrator");

            if (!isCreator && !isAdmin)
            {
                return Forbid();
            }

            Survey = new SurveyDetailDto
            {
                Id = surveyEntity.Id,
                Title = surveyEntity.Title,
                Description = surveyEntity.Description,
                StartDate = surveyEntity.StartDate,
                EndDate = surveyEntity.EndDate,
                IsPublished = surveyEntity.IsPublished,
                AllowAnonymous = surveyEntity.AllowAnonymous,
                RequiresAuthentication = surveyEntity.RequiresAuthentication,
                AllowMultipleResponses = surveyEntity.AllowMultipleResponses,
                CreatedBy = surveyEntity.CreatedByUser?.DisplayName ?? "Unknown",
                CreatedAt = surveyEntity.CreatedAt
            };

            Questions = surveyEntity.Questions?
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Description = q.Description,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder,
                    Options = q.Options?
                        .Select(o => new QuestionOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            DisplayOrder = o.DisplayOrder
                        })
                        .OrderBy(o => o.DisplayOrder)
                        .ToList()
                })
                .OrderBy(q => q.DisplayOrder)
                .ToList() ?? new List<QuestionDto>();

            return Page();
        }
    }
}