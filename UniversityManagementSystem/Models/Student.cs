using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public int? ClassId { get; set; }

    public string? Status { get; set; }

    public virtual SchoolClass SchoolClass { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
