using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Web.Pages.Surveys
{
    //[Authorize(Policy = "RequireSurveyCreatorRole")]
    public class ResultsModel : PageModel
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ICurrentUserService _currentUserService;

        public ResultsModel(
            ISurveyRepository surveyRepository,
            ISurveyResponseRepository surveyResponseRepository,
            IQuestionRepository questionRepository,
            ICurrentUserService currentUserService)
        {
            _surveyRepository = surveyRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _questionRepository = questionRepository;
            _currentUserService = currentUserService;
        }

        public SurveyDetailDto Survey { get; set; }
        public IEnumerable<SurveyResponseDto> Responses { get; set; } = new List<SurveyResponseDto>();
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();

        public int TotalResponses { get; set; }
        public int CompletionRate { get; set; }
        public string AverageCompletionTime { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var surveyEntity = await _surveyRepository.GetSurveyWithQuestionsAsync(id);

            if (surveyEntity == null)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            bool isCreator = surveyEntity.CreatedByUserId.ToString() == _currentUserService.GetCurrentUserId();
            //bool isAdmin = _currentUserService.IsInRole("Administrator");

            if (!isCreator /*&& !isAdmin*/)
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

            // Get survey responses
            var responseEntities = await _surveyResponseRepository.GetResponsesBySurveyIdAsync(id);
            TotalResponses = responseEntities.Count;

            Responses = responseEntities.Select(r => new SurveyResponseDto
            {
                Id = r.Id,
                RespondentEmail = r.RespondentEmail,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                QuestionResponses = r.QuestionResponses?.Select(qr => new QuestionResponseDto
                {
                    QuestionId = qr.QuestionId,
                    QuestionText = qr.Question?.Text,
                    SelectedOptionId = qr.SelectedOptionId,
                    SelectedOptionText = qr.SelectedOption?.Text,
                    TextResponse = qr.TextResponse,
                    NumericResponse = qr.NumericResponse,
                    DateResponse = qr.DateResponse
                }).ToList()
            }).ToList();

            // Calculate statistics
            var completedResponses = responseEntities.Count(r => r.CompletedAt.HasValue);
            CompletionRate = TotalResponses > 0 ? (completedResponses * 100) / TotalResponses : 0;

            var avgTime = responseEntities
                .Where(r => r.CompletedAt.HasValue)
                .Select(r => (r.CompletedAt.Value - r.StartedAt).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();

            AverageCompletionTime = avgTime > 0 ? $"{avgTime:F1} min" : "N/A";

            // Process question results
            if (surveyEntity.Questions != null)
            {
                foreach (var question in surveyEntity.Questions.OrderBy(q => q.DisplayOrder))
                {
                    var questionResult = new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        QuestionType = question.Type.ToString(),
                        TotalResponses = 0,
                        OptionResults = new List<OptionResultDto>(),
                        TextResponses = new List<string>(),
                        DateResponses = new List<DateTime>()
                    };

                    // Get all responses for this question
                    var questionResponses = Responses
                        .Where(r => r.QuestionResponses != null)
                        .SelectMany(r => r.QuestionResponses)
                        .Where(qr => qr.QuestionId == question.Id)
                        .ToList();

                    questionResult.TotalResponses = questionResponses.Count;

                    // Process responses based on question type
                    switch (question.Type)
                    {
                        case QuestionType.SingleChoice:
                        case QuestionType.MultipleChoice:
                            var options = question.Options.OrderBy(o => o.DisplayOrder).ToList();
                            var optionCounts = questionResponses
                                .Where(qr => qr.SelectedOptionId.HasValue)
                                .GroupBy(qr => qr.SelectedOptionId)
                                .ToDictionary(g => g.Key.Value, g => g.Count());

                            foreach (var option in options)
                            {
                                var count = optionCounts.ContainsKey(option.Id) ? optionCounts[option.Id] : 0;
                                var percentage = questionResult.TotalResponses > 0
                                    ? (count * 100.0) / questionResult.TotalResponses
                                    : 0;

                                questionResult.OptionResults.Add(new OptionResultDto
                                {
                                    OptionId = option.Id,
                                    OptionText = option.Text,
                                    Count = count,
                                    Percentage = percentage
                                });
                            }
                            break;

                        case QuestionType.Rating:
                            for (int i = 1; i <= 5; i++)
                            {
                                var count = questionResponses.Count(qr => qr.NumericResponse == i);
                                var percentage = questionResult.TotalResponses > 0
                                    ? (count * 100.0) / questionResult.TotalResponses
                                    : 0;

                                questionResult.OptionResults.Add(new OptionResultDto
                                {
                                    OptionId = Guid.Empty,
                                    OptionText = i.ToString(),
                                    Count = count,
                                    Percentage = percentage
                                });
                            }

                            // Calculate average rating
                            questionResult.AverageRating = questionResponses
                                .Where(qr => qr.NumericResponse.HasValue)
                                .Select(qr => qr.NumericResponse.Value)
                                .DefaultIfEmpty(0)
                                .Average();
                            break;

                        case QuestionType.ShortAnswer:
                        case QuestionType.LongAnswer:
                            questionResult.TextResponses = questionResponses
                                .Where(qr => !string.IsNullOrEmpty(qr.TextResponse))
                                .Select(qr => qr.TextResponse)
                                .ToList();
                            break;

                        case QuestionType.Date:
                            questionResult.DateResponses = questionResponses
                                .Where(qr => qr.DateResponse.HasValue)
                                .Select(qr => qr.DateResponse.Value)
                                .ToList();
                            break;
                    }

                    QuestionResults.Add(questionResult);
                }
            }

            return Page();
        }
    }

    public class QuestionResultDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int TotalResponses { get; set; }
        public List<OptionResultDto> OptionResults { get; set; } = new List<OptionResultDto>();
        public List<string> TextResponses { get; set; } = new List<string>();
        public List<DateTime> DateResponses { get; set; } = new List<DateTime>();
        public double AverageRating { get; set; }
    }

    public class OptionResultDto
    {
        public Guid OptionId { get; set; }
        public string OptionText { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}