using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    [Authorize(Policy = "RequireSurveyCreatorRole")]
    public class CreateModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public CreateModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        [BindProperty]
        public CreateSurveyDto Survey { get; set; } = new CreateSurveyDto
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30),
            IsPublished = false,
            AllowAnonymous = true,
            RequiresAuthentication = false,
            AllowMultipleResponses = true
        };

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Create a new survey entity
            var surveyEntity = new Survey
            {
                Title = Survey.Title,
                Description = Survey.Description,
                StartDate = Survey.StartDate,
                EndDate = Survey.EndDate,
                IsPublished = Survey.IsPublished,
                AllowAnonymous = Survey.AllowAnonymous,
                RequiresAuthentication = Survey.RequiresAuthentication,
                AllowMultipleResponses = Survey.AllowMultipleResponses,
                CreatedByUserId = Guid.Parse(_currentUserService.GetCurrentUserId())
            };

            await _surveyService.CreateSurveyAsync(surveyEntity);

            return RedirectToPage("./Edit", new { id = surveyEntity.Id });
        }
    }
}