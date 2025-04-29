using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Infrastructure.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISurveyResponseRepository _surveyResponseRepository;
        private readonly IAuditService _auditService;

        public SurveyService(
            ISurveyRepository surveyRepository,
            ISurveyResponseRepository surveyResponseRepository,
            IAuditService auditService)
        {
            _surveyRepository = surveyRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Survey>> GetAllSurveysAsync()
        {
            return await _surveyRepository.GetAllAsync();
        }

        public async Task<Survey> GetSurveyByIdAsync(Guid id, bool includeQuestions = false)
        {
            if (includeQuestions)
            {
                return await _surveyRepository.GetSurveyWithQuestionsAsync(id);
            }

            return await _surveyRepository.GetByIdAsync(id);
        }

        public async Task<Survey> CreateSurveyAsync(Survey survey)
        {
            await _surveyRepository.AddAsync(survey);

            await _auditService.LogActivityAsync(
                "Create",
                "Survey",
                survey.Id.ToString(),
                $"Survey '{survey.Title}' created");

            return survey;
        }

        public async Task UpdateSurveyAsync(Survey survey)
        {
            await _surveyRepository.UpdateAsync(survey);

            await _auditService.LogActivityAsync(
                "Update",
                "Survey",
                survey.Id.ToString(),
                $"Survey '{survey.Title}' updated");
        }

        public async Task DeleteSurveyAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey != null)
            {
                await _surveyRepository.DeleteAsync(survey);

                await _auditService.LogActivityAsync(
                    "Delete",
                    "Survey",
                    id.ToString(),
                    $"Survey '{survey.Title}' deleted");
            }
        }

        public async Task<IEnumerable<SurveyResponse>> GetSurveyResponsesAsync(Guid surveyId)
        {
            return await _surveyResponseRepository.GetResponsesBySurveyIdAsync(surveyId);
        }

        public async Task<SurveyResponse> SubmitSurveyResponseAsync(SurveyResponse response)
        {
            await _surveyResponseRepository.AddAsync(response);

            await _auditService.LogActivityAsync(
                "Create",
                "SurveyResponse",
                response.Id.ToString(),
                $"Response submitted for survey '{response.SurveyId}'");

            return response;
        }
    }
}