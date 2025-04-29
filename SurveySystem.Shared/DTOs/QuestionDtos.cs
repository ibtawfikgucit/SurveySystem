using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Shared.DTOs
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public List<QuestionOptionDto> Options { get; set; }
    }

    public class QuestionOptionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CreateQuestionDto
    {
        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; }

        public bool IsRequired { get; set; }

        public int? DisplayOrder { get; set; }

        public int? MinValue { get; set; }

        public int? MaxValue { get; set; }

        public string ValidationRegex { get; set; }

        public List<CreateQuestionOptionDto> Options { get; set; }
    }

    public class CreateQuestionOptionDto
    {
        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        public int? DisplayOrder { get; set; }
    }

    public class UpdateQuestionDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public int? DisplayOrder { get; set; }

        public List<UpdateQuestionOptionDto> Options { get; set; }
    }

    public class UpdateQuestionOptionDto
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        public int? DisplayOrder { get; set; }
    }
    public class QuestionOrderDto
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
    }
}