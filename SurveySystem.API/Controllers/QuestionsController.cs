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
    [Route("api/surveys/{surveyId}/[controller]")]
    [ApiController]
    //[Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public QuestionsController(
            ISurveyRepository surveyRepository,
            IQuestionRepository questionRepository,
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _surveyRepository = surveyRepository;
            _questionRepository = questionRepository;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        // GET: api/surveys/5/questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions(Guid surveyId)
        {
            var questions = await _questionRepository.GetQuestionsBySurveyIdAsync(surveyId);

            var questionDtos = questions.Select(q => new QuestionDto
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
            });

            return Ok(questionDtos);
        }

        // GET: api/surveys/5/questions/1
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(Guid surveyId, Guid id)
        {
            var question = await _questionRepository.GetQuestionWithOptionsAsync(id);

            if (question == null || question.SurveyId != surveyId)
            {
                return NotFound();
            }

            var questionDto = new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                Description = question.Description,
                Type = question.Type.ToString(),
                IsRequired = question.IsRequired,
                DisplayOrder = question.DisplayOrder,
                Options = question.Options?.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    DisplayOrder = o.DisplayOrder
                }).OrderBy(o => o.DisplayOrder).ToList()
            };

            return Ok(questionDto);
        }

        // POST: api/surveys/5/questions
        [HttpPost]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<ActionResult<QuestionDto>> CreateQuestion(Guid surveyId, CreateQuestionDto createQuestionDto)
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

            var question = new Question
            {
                SurveyId = surveyId,
                Text = createQuestionDto.Text,
                Description = createQuestionDto.Description,
                Type = Enum.Parse<QuestionType>(createQuestionDto.Type),
                IsRequired = createQuestionDto.IsRequired,
                DisplayOrder = createQuestionDto.DisplayOrder ?? await GetNextDisplayOrderAsync(surveyId)
            };

            // Add options if provided
            if (createQuestionDto.Options != null && createQuestionDto.Options.Any())
            {
                question.Options = new List<QuestionOption>();
                int order = 1;
                foreach (var optionDto in createQuestionDto.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Text = optionDto.Text,
                        DisplayOrder = optionDto.DisplayOrder ?? order++
                    });
                }
            }

            await _questionRepository.AddAsync(question);

            await _auditService.LogActivityAsync(
                "Create",
                "Question",
                question.Id.ToString(),
                $"Question added to survey '{surveyId}'");

            return CreatedAtAction(nameof(GetQuestion),
                new { surveyId = surveyId, id = question.Id },
                new QuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Description = question.Description,
                    Type = question.Type.ToString(),
                    IsRequired = question.IsRequired,
                    DisplayOrder = question.DisplayOrder
                });
        }

        // PUT: api/surveys/5/questions/1
        [HttpPut("{id}")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<IActionResult> UpdateQuestion(Guid surveyId, Guid id, UpdateQuestionDto updateQuestionDto)
        {
            if (id != updateQuestionDto.Id)
            {
                return BadRequest();
            }

            var question = await _questionRepository.GetQuestionWithOptionsAsync(id);

            if (question == null || question.SurveyId != surveyId)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            //if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() &&
            //    !_currentUserService.IsInRole("Administrator"))
            //{
            //    return Forbid();
            //}

            await _questionRepository.DeleteAsync(question);

            await _auditService.LogActivityAsync(
                "Delete",
                "Question",
                id.ToString(),
                $"Question deleted from survey '{surveyId}'");

            return NoContent();
        }

        // DELETE: api/surveys/5/questions/1
        [HttpDelete("{id}")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<IActionResult> DeleteQuestion(Guid surveyId, Guid id)
        {
            var question = await _questionRepository.GetQuestionWithOptionsAsync(id);


            if (question == null || question.SurveyId != surveyId)
            {
                return NotFound();
            }

            // Check if the user is the creator or an admin
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            //if (survey.CreatedByUserId.ToString() != _currentUserService.GetCurrentUserId() &&
            //    !_currentUserService.IsInRole("Administrator"))
            //{
            //    return Forbid();
            //}

            // Delete the question (repository should handle cascading deletes)
            await _questionRepository.DeleteAsync(question);

            await _auditService.LogActivityAsync(
                "Delete",
                "Question",
                id.ToString(),
                $"Question deleted from survey '{surveyId}'");

            return NoContent();
        }

        // POST: api/surveys/{surveyId}/questions/reorder
        [HttpPost("reorder")]
        //[Authorize(Policy = "RequireSurveyCreatorRole")]
        public async Task<IActionResult> ReorderQuestions(Guid surveyId, [FromBody] List<QuestionOrderDto> questionsOrder)
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

            // Update question display order
            var questionOrders = questionsOrder.Select(q => (q.Id, q.DisplayOrder)).ToList();
            await _questionRepository.UpdateQuestionOrderAsync(questionOrders);

            await _auditService.LogActivityAsync(
                "Update",
                "Question",
                surveyId.ToString(),
                $"Questions reordered in survey '{surveyId}'");

            return Ok();
        }

        private async Task<int> GetNextDisplayOrderAsync(Guid surveyId)
        {
            var questions = await _questionRepository.GetQuestionsBySurveyIdAsync(surveyId);
            return questions.Any() ? questions.Max(q => q.DisplayOrder) + 1 : 1;
        }
    }
}