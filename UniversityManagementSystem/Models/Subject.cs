using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class Subject
{
    public string SubjectId { get; set; } = null!;

    public string SubjectName { get; set; } = null!;

    public int Credits { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
