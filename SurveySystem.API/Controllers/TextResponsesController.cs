using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveySystem.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.API.Controllers
{
    [Route("api/surveys/{surveyId}/questions/{questionId}/responses")]
    [ApiController]
    [Authorize(Policy = "RequireSurveyCreatorRole")]
    public class TextResponsesController : ControllerBase
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly ICurrentUserService _currentUserService;
        public TextResponsesController(
            ISurveyRepository surveyRepository,
            IQuestionRepository questionRepository,
            ISurveyResponseRepository surveyResponseRepository,
            ICurrentUserService currentUserService)
        {
            _surveyRepository = surveyRepository;
            _questionRepository = questionRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _currentUserService = currentUserService;
        }

        // GET: api/surveys/{surveyId}/questions/{questionId}/responses
        [HttpGet]
        public async Task<IActionResult> GetTextResponses(Guid surveyId, Guid questionId)
        {
            // Check if the survey exists
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
            {
                return NotFound("Survey not found");
            }

            // Check if the user is authorized
            if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() &&
                !_currentUserService.IsInRole("Administrator"))
            {
                return Forbid();
            }

            // Check if the question exists and belongs to the survey
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null || question.SurveyId != surveyId)
            {
                return NotFound("Question not found in this survey");
            }

            // Get all text responses for this question
            var questionResponses = await _surveyResponseRepository.GetQuestionResponsesAsync(questionId);
            var textResponses = questionResponses
                .Where(qr => !string.IsNullOrEmpty(qr.TextResponse))
                .Select(qr => qr.TextResponse)
                .ToList();

            return Ok(textResponses);
        }
    }
}