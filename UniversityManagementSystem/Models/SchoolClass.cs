using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniversityManagementSystem.Models;

public partial class SchoolClass
{
    [Display(Name = "Mã Lớp")]
    public int ClassId { get; set; }

    [Display(Name = "Tên Lớp học")]
    [Required(ErrorMessage = "Vui lòng nhập Tên Lớp học!")]
    public string ClassName { get; set; } = null!;

    [Display(Name = "Thuộc Khoa / Tổ")]
    [Required(ErrorMessage = "Vui lòng chọn Khoa hoặc Tổ quản lý!")]
    public int? FacultyId { get; set; }

    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}