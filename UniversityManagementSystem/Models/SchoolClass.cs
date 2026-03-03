using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class SchoolClass
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int? FacultyId { get; set; }

    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
