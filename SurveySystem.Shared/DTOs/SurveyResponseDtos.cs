using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Shared.DTOs
{
    public class SurveyResponseDto
    {
        public Guid Id { get; set; }
        public string RespondentEmail { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<QuestionResponseDto> QuestionResponses { get; set; }
    }

    public class QuestionResponseDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; }
        public string TextResponse { get; set; }
        public int? NumericResponse { get; set; }
        public DateTime? DateResponse { get; set; }
    }

    public class SubmitSurveyResponseDto
    {
        public DateTime? StartedAt { get; set; }

        [EmailAddress]
        public string RespondentEmail { get; set; }

        [Required]
        public List<SubmitQuestionResponseDto> QuestionResponses { get; set; }
    }

    public class SubmitQuestionResponseDto
    {
        [Required]
        public Guid QuestionId { get; set; }

        public Guid? SelectedOptionId { get; set; }

        public string TextResponse { get; set; }

        public int? NumericResponse { get; set; }

        public DateTime? DateResponse { get; set; }
    }
}