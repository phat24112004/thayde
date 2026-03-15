using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Bổ sung thư viện này

// QUAN TRỌNG: Namespace phải đúng y hệt dòng này
namespace UniversityManagementSystem.Models;

public partial class Faculty
{
    [Display(Name = "Mã Khoa / Tổ")]
    public int FacultyId { get; set; }

    [Display(Name = "Tên Khoa / Tổ Đào tạo")]
    [Required(ErrorMessage = "Vui lòng nhập Tên Khoa/Tổ, không được bỏ trống!")]
    public string FacultyName { get; set; } = null!;

    [Display(Name = "Ngày thành lập")]
    public DateOnly? EstablishedDate { get; set; }

    // Mình khởi tạo luôn list này để tránh lỗi null sau này
    public virtual ICollection<SchoolClass> SchoolClasses { get; set; } = new List<SchoolClass>();
}