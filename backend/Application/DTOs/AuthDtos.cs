using System;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRoleEnum Role { get; set; }
        public string RoleName => Role.ToString();
        
        public Guid? AgencyId { get; set; }
        public string? AgencyCode { get; set; }
        public string? AgencyName { get; set; }

        public bool IsActive { get; set; }
        public bool HasDataOperations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRoleEnum Role { get; set; } = UserRoleEnum.AgencyUser;
        public string? RoleString { get; set; }
        public Guid? AgencyId { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRoleEnum Role { get; set; }
        public string? RoleString { get; set; }
        public Guid? AgencyId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string EmailOrUsername { get; set; } = string.Empty;
    }
}
