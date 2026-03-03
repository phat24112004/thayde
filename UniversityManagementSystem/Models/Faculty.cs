using System;
using System.Collections.Generic;

// QUAN TRỌNG: Namespace phải đúng y hệt dòng này
namespace UniversityManagementSystem.Models;

public partial class Faculty
{
    public int FacultyId { get; set; }

    public string FacultyName { get; set; } = null!;

    public DateOnly? EstablishedDate { get; set; }

    // Mình khởi tạo luôn list này để tránh lỗi null sau này
    public virtual ICollection<SchoolClass> SchoolClasses { get; set; } = new List<SchoolClass>();
}