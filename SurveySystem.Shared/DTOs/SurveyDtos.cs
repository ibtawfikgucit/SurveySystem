using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Shared.DTOs
{
    public class SurveyDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SurveyDetailDto : SurveyDto
    {
        public bool AllowAnonymous { get; set; }
        public bool RequiresAuthentication { get; set; }
        public bool AllowMultipleResponses { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class CreateSurveyDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsPublished { get; set; }

        public bool AllowAnonymous { get; set; }

        public bool RequiresAuthentication { get; set; }

        public bool AllowMultipleResponses { get; set; }
    }

    public class UpdateSurveyDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsPublished { get; set; }

        public bool AllowAnonymous { get; set; }

        public bool RequiresAuthentication { get; set; }

        public bool AllowMultipleResponses { get; set; }
    }
    public class SurveyStatsDto
    {
        public Guid SurveyId { get; set; }
        public int TotalResponses { get; set; }
        public int CompletedResponses { get; set; }
        public int CompletionRate { get; set; }
        public string AverageCompletionTime { get; set; }
    }
}
