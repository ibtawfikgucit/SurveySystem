using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    public class AnswerModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ICurrentUserService _currentUserService;

        public AnswerModel(ISurveyService surveyService, ICurrentUserService currentUserService)
        {
            _surveyService = surveyService;
            _currentUserService = currentUserService;
        }

        [BindProperty]
        public Guid SurveyId { get; set; }

        [BindProperty]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [BindProperty]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string RespondentEmail { get; set; }

        [BindProperty]
        public List<ResponseDto> Responses { get; set; } = new List<ResponseDto>();

        public SurveyDetailDto Survey { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var surveyEntity = await _surveyService.GetSurveyByIdAsync(id, includeQuestions: true);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            // Check if survey is published and active
            if (!surveyEntity.IsPublished)
            {
                return RedirectToPage("./Details", new { id = id });
            }

            if (surveyEntity.StartDate > DateTime.UtcNow)
            {
                ErrorMessage = "This survey is not yet active.";
                return Page();
            }

            if (surveyEntity.EndDate.HasValue && surveyEntity.EndDate < DateTime.UtcNow)
            {
                ErrorMessage = "This survey has ended.";
                return Page();
            }

            // Check authentication requirement
            if (surveyEntity.RequiresAuthentication && !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Surveys/Answer/{id}" });
            }

            SurveyId = id;
            StartTime = DateTime.UtcNow;

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
                AllowMultipleResponses = surveyEntity.AllowMultipleResponses
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

        public async Task<IActionResult> OnPostAsync()
        {
            var surveyEntity = await _surveyService.GetSurveyByIdAsync(SurveyId, includeQuestions: true);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            // Check if survey is still active
            if (!surveyEntity.IsPublished ||
                surveyEntity.StartDate > DateTime.UtcNow ||
                (surveyEntity.EndDate.HasValue && surveyEntity.EndDate < DateTime.UtcNow))
            {
                ErrorMessage = "This survey is not currently active.";
                await PopulateSurveyDataAsync(surveyEntity);
                return Page();
            }

            // Check authentication requirement
            if (surveyEntity.RequiresAuthentication && !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Surveys/Answer/{SurveyId}" });
            }

            // Validate required questions
            var requiredQuestionIds = surveyEntity.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .ToList();

            var answeredQuestionIds = Responses
                .Where(r => !string.IsNullOrEmpty(r.TextResponse) ||
                            r.NumericResponse.HasValue ||
                            r.DateResponse.HasValue ||
                            r.SelectedOptionId.HasValue ||
                            (r.SelectedOptionIds != null && r.SelectedOptionIds.Any()))
                .Select(r => r.QuestionId)
                .ToList();

            var missingRequiredQuestions = requiredQuestionIds
                .Except(answeredQuestionIds)
                .ToList();

            if (missingRequiredQuestions.Any())
            {
                ErrorMessage = "Please answer all required questions.";
                await PopulateSurveyDataAsync(surveyEntity);
                return Page();
            }

            // Create a survey response entity
            var response = new SurveyResponse
            {
                SurveyId = SurveyId,
                StartedAt = StartTime,
                CompletedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            // Set respondent info
            if (User.Identity.IsAuthenticated)
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    response.RespondentId = userGuid;
                }
            }
            else if (!string.IsNullOrEmpty(RespondentEmail))
            {
                response.RespondentEmail = RespondentEmail;
            }

            // Add question responses
            response.QuestionResponses = new List<QuestionResponse>();

            foreach (var responseDto in Responses)
            {
                if (responseDto == null) continue;

                var question = surveyEntity.Questions.FirstOrDefault(q => q.Id == responseDto.QuestionId);
                if (question == null) continue;

                // Handle the appropriate response type based on question type
                var questionResponse = new QuestionResponse
                {
                    QuestionId = responseDto.QuestionId
                };

                switch (question.Type)
                {
                    case QuestionType.SingleChoice:
                        questionResponse.SelectedOptionId = responseDto.SelectedOptionId;
                        break;
                    case QuestionType.MultipleChoice:
                        // Multiple choice needs special handling - we'll create separate responses for each selection
                        if (responseDto.SelectedOptionIds != null && responseDto.SelectedOptionIds.Any())
                        {
                            foreach (var optionId in responseDto.SelectedOptionIds)
                            {
                                response.QuestionResponses.Add(new QuestionResponse
                                {
                                    QuestionId = responseDto.QuestionId,
                                    SelectedOptionId = optionId
                                });
                            }
                            continue; // Skip adding the main response
                        }
                        break;
                    case QuestionType.ShortAnswer:
                    case QuestionType.LongAnswer:
                        questionResponse.TextResponse = responseDto.TextResponse;
                        break;
                    case QuestionType.Rating:
                        questionResponse.NumericResponse = responseDto.NumericResponse;
                        break;
                    case QuestionType.Date:
                        questionResponse.DateResponse = responseDto.DateResponse;
                        break;
                }

                response.QuestionResponses.Add(questionResponse);
            }

            // Submit the response
            await _surveyService.SubmitSurveyResponseAsync(response);

            // Show success message
            SuccessMessage = "Thank you for completing this survey!";
            return Page();
        }

        private async Task PopulateSurveyDataAsync(Survey surveyEntity)
        {
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
                AllowMultipleResponses = surveyEntity.AllowMultipleResponses
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
        }
    }

    public class ResponseDto
    {
        public Guid QuestionId { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public List<Guid> SelectedOptionIds { get; set; }
        public string TextResponse { get; set; }
        public int? NumericResponse { get; set; }
        public DateTime? DateResponse { get; set; }
    }
}