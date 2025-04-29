using System;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Shared.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsAdmin { get; set; }
        public string Token { get; set; }
    }

    public class RegisterUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        public string Organization { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActiveDirectoryUser { get; set; }
        public string Organization { get; set; }
        public bool IsExternal { get; set; }
    }
}