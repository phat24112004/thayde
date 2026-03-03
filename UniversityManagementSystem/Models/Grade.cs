using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public string? StudentId { get; set; }

    public string? SubjectId { get; set; }

    public decimal? Score { get; set; }

    public DateOnly? ExamDate { get; set; }

    public string? Semester { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }
}
