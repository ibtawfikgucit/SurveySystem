using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SurveySystem.API.Controllers
{
    [Route("api/surveys/{surveyId}/responses")]
    [ApiController]
    public class SurveyResponsesController : ControllerBase
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public SurveyResponsesController(
            ISurveyRepository surveyRepository,
            ISurveyResponseRepository surveyResponseRepository,
            IQuestionRepository questionRepository,
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _surveyRepository = surveyRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _questionRepository = questionRepository;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        // GET: api/surveys/5/responses
        [HttpGet]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<ActionResult<IEnumerable<SurveyResponseDto>>> GetResponses(Guid surveyId)
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
            {
                return NotFound("Survey not found");
            }

            // Check if the user is the creator or an admin
            //if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() &&
            //    !_currentUserService.IsInRole("Administrator"))
            //{
            //    return Forbid();
            //}

            var responses = await _surveyResponseRepository.GetResponsesBySurveyIdAsync(surveyId);

            var responseDtos = responses.Select(r => new SurveyResponseDto
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

            return Ok(responseDtos);
        }

        // GET: api/surveys/5/responses/1
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<ActionResult<SurveyResponseDto>> GetResponse(Guid surveyId, Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
            {
                return NotFound("Survey not found");
            }

            // Check if the user is the creator or an admin
            //if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() &&
            //    !_currentUserService.IsInRole("Administrator"))
            //{
            //    return Forbid();
            //}

            var response = await _surveyResponseRepository.GetResponseWithAnswersAsync(id);

            if (response == null || response.SurveyId != surveyId)
            {
                return NotFound();
            }

            var responseDto = new SurveyResponseDto
            {
                Id = response.Id,
                RespondentEmail = response.RespondentEmail,
                StartedAt = response.StartedAt,
                CompletedAt = response.CompletedAt,
                QuestionResponses = response.QuestionResponses?.Select(qr => new QuestionResponseDto
                {
                    QuestionId = qr.QuestionId,
                    QuestionText = qr.Question?.Text,
                    SelectedOptionId = qr.SelectedOptionId,
                    SelectedOptionText = qr.SelectedOption?.Text,
                    TextResponse = qr.TextResponse,
                    NumericResponse = qr.NumericResponse,
                    DateResponse = qr.DateResponse
                }).ToList()
            };

            return Ok(responseDto);
        }

        // DELETE: api/surveys/5/responses/1
        [HttpDelete("{id}")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteResponse(Guid surveyId, Guid id)
        {
            var response = await _surveyResponseRepository.GetByIdAsync(id);

            if (response == null || response.SurveyId != surveyId)
            {
                return NotFound();
            }

            await _surveyResponseRepository.DeleteAsync(response);

            await _auditService.LogActivityAsync(
                "Delete",
                "SurveyResponse",
                id.ToString(),
                $"Response deleted from survey '{surveyId}'");

            return NoContent();
        }
    }
}