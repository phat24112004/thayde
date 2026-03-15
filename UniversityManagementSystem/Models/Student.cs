using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Phải có thư viện này để dùng [Required], [Display]

namespace UniversityManagementSystem.Models;

public partial class Student
{
    [Display(Name = "Mã Sinh viên")]
    [Required(ErrorMessage = "Vui lòng nhập Mã Sinh viên, không được bỏ trống!")]
    public string StudentId { get; set; } = null!;

    [Display(Name = "Họ và Tên")]
    [Required(ErrorMessage = "Vui lòng nhập Họ và Tên Sinh viên!")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Ngày sinh")]
    public DateOnly? DateOfBirth { get; set; }

    // THÊM CỘT GIỚI TÍNH VÀO ĐÂY ĐỂ TRỊ LỖI GIAO DIỆN
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng (VD: abc@gmail.com)")]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Lớp học")]
    [Required(ErrorMessage = "Vui lòng chọn Lớp học cho sinh viên!")]
    public int? ClassId { get; set; }

    [Display(Name = "Trạng thái học tập")]
    public string? Status { get; set; }

    public virtual SchoolClass? SchoolClass { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}