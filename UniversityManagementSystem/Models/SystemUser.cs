using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class SystemUser
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime? LastLogin { get; set; }
}
