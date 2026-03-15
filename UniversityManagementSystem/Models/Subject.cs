using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniversityManagementSystem.Models;

public partial class Subject
{
    [Display(Name = "Mã Môn học")]
    [Required(ErrorMessage = "Vui lòng nhập Mã Môn học (VD: IT101)!")]
    public string SubjectId { get; set; } = null!;

    [Display(Name = "Tên Môn học")]
    [Required(ErrorMessage = "Vui lòng nhập Tên Môn học!")]
    public string SubjectName { get; set; } = null!;

    [Display(Name = "Số Tín chỉ / Số Tiết")]
    [Required(ErrorMessage = "Vui lòng nhập số tín chỉ!")]
    [Range(1, 50, ErrorMessage = "Số tín chỉ phải lớn hơn 0 và hợp lý")]
    public int Credits { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}