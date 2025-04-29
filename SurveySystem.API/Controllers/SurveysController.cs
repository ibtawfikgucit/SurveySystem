using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public SurveysController(
            ISurveyService surveyService,
            ISurveyRepository surveyRepository,
            ISurveyResponseRepository surveyResponseRepository,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _surveyService = surveyService;
            _surveyRepository = surveyRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        // GET: api/surveys
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SurveyDto>>> GetSurveys()
        {
            IEnumerable<Survey> surveys;
            var userId = _currentUserService.GetCurrentUserId();
            //var isAdmin = _currentUserService.IsInRole("Administrator");

            //if (isAdmin)
            //{
                // Admins can see all surveys
                surveys = await _surveyService.GetAllSurveysAsync();
            //}
            //else if (Guid.TryParse(userId, out Guid userGuid))
            //{
            //    // Normal users see published surveys + their own
            //    var allSurveys = await _surveyService.GetAllSurveysAsync();
            //    surveys = allSurveys.Where(s =>
            //        s.IsPublished || s.CreatedByUserId == userGuid);
            //}
            //else
            //{
            //    // Unauthenticated users only see published surveys
            //    var allSurveys = await _surveyService.GetAllSurveysAsync();
            //    surveys = allSurveys.Where(s => s.IsPublished);
            //}

            var surveyDtos = surveys.Select(s => new SurveyDto
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

            return Ok(surveyDtos);
        }

        // GET: api/surveys/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SurveyDetailDto>> GetSurvey(Guid id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id, includeQuestions: true);

            if (survey == null)
            {
                return NotFound();
            }

            // Check access permissions for non-published surveys
            var userId = _currentUserService.GetCurrentUserId();
            //var isAdmin = _currentUserService.IsInRole("Administrator");

            if (!survey.IsPublished &&
                //!isAdmin &&
                survey.CreatedByUserId.ToString() != userId)
            {
                return Forbid();
            }

            var surveyDto = new SurveyDetailDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsPublished = survey.IsPublished,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                AllowAnonymous = survey.AllowAnonymous,
                RequiresAuthentication = survey.RequiresAuthentication,
                AllowMultipleResponses = survey.AllowMultipleResponses,
                CreatedBy = survey.CreatedByUser?.DisplayName ?? "Unknown",
                CreatedAt = survey.CreatedAt,
                Questions = survey.Questions?.Select(q => new QuestionDto
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
                }).OrderBy(q => q.DisplayOrder).ToList()
            };

            return Ok(surveyDto);
        }

        // POST: api/surveys
        [HttpPost]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<ActionResult<SurveyDto>> CreateSurvey(CreateSurveyDto createSurveyDto)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var survey = new Survey
            {
                Title = createSurveyDto.Title,
                Description = createSurveyDto.Description,
                StartDate = createSurveyDto.StartDate,
                EndDate = createSurveyDto.EndDate,
                IsPublished = createSurveyDto.IsPublished,
                AllowAnonymous = createSurveyDto.AllowAnonymous,
                RequiresAuthentication = createSurveyDto.RequiresAuthentication,
                AllowMultipleResponses = createSurveyDto.AllowMultipleResponses,
                CreatedByUserId = userGuid
            };

            await _surveyService.CreateSurveyAsync(survey);

            return CreatedAtAction(nameof(GetSurvey), new { id = survey.Id }, new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsPublished = survey.IsPublished,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                CreatedBy = _currentUserService.GetCurrentUsername(),
                CreatedAt = survey.CreatedAt
            });
        }

        // PUT: api/surveys/5
        [HttpPut("{id}")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<IActionResult> UpdateSurvey(Guid id, UpdateSurveyDto updateSurveyDto)
        {
            if (id != updateSurveyDto.Id)
            {
                return BadRequest();
            }

            var survey = await _surveyService.GetSurveyByIdAsync(id);

            if (survey == null)
            {
                return NotFound();
            }

            // Check user permissions
            var userId = _currentUserService.GetCurrentUserId();
            //var isAdmin = _currentUserService.IsInRole("Administrator");

            if (/*!isAdmin && */survey.CreatedByUserId.ToString() != userId)
            {
                return Forbid("You don't have permission to update this survey");
            }

            // Update survey properties
            survey.Title = updateSurveyDto.Title;
            survey.Description = updateSurveyDto.Description;
            survey.StartDate = updateSurveyDto.StartDate;
            survey.EndDate = updateSurveyDto.EndDate;
            survey.IsPublished = updateSurveyDto.IsPublished;
            survey.AllowAnonymous = updateSurveyDto.AllowAnonymous;
            survey.RequiresAuthentication = updateSurveyDto.RequiresAuthentication;
            survey.AllowMultipleResponses = updateSurveyDto.AllowMultipleResponses;

            await _surveyService.UpdateSurveyAsync(survey);

            return NoContent();
        }

        // DELETE: api/surveys/5
        [HttpDelete("{id}")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);

            if (survey == null)
            {
                return NotFound();
            }

            await _surveyService.DeleteSurveyAsync(id);

            return NoContent();
        }

        // GET: api/surveys/5/stats
        [HttpGet("{id}/stats")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<ActionResult<SurveyStatsDto>> GetSurveyStats(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);

            if (survey == null)
            {
                return NotFound();
            }

            // Check user permissions
            var userId = _currentUserService.GetCurrentUserId();
            //var isAdmin = _currentUserService.IsInRole("Administrator");

            if (/*!isAdmin && */survey.CreatedByUserId.ToString() != userId)
            {
                return Forbid("You don't have permission to view this survey's stats");
            }

            // Get responses for this survey
            var responses = await _surveyResponseRepository.GetResponsesBySurveyIdAsync(id);
            var totalResponses = responses.Count();
            var completedResponses = responses.Count(r => r.CompletedAt.HasValue);

            // Calculate completion rate
            int completionRate = totalResponses > 0 ? (completedResponses * 100) / totalResponses : 0;

            // Calculate average completion time
            TimeSpan averageTime = TimeSpan.Zero;
            if (completedResponses > 0)
            {
                var totalTime = responses
                    .Where(r => r.CompletedAt.HasValue)
                    .Sum(r => (r.CompletedAt.Value - r.StartedAt).TotalSeconds);

                averageTime = TimeSpan.FromSeconds(totalTime / completedResponses);
            }

            var stats = new SurveyStatsDto
            {
                SurveyId = id,
                TotalResponses = totalResponses,
                CompletedResponses = completedResponses,
                CompletionRate = completionRate,
                AverageCompletionTime = averageTime.TotalMinutes >= 1
                    ? $"{Math.Round(averageTime.TotalMinutes, 1)} min"
                    : $"{Math.Round(averageTime.TotalSeconds)} sec"
            };

            return Ok(stats);
        }
    }
}