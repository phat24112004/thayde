using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này để dùng được [Column]

namespace UniversityManagementSystem.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    [Display(Name = "Sinh viên")]
    [Required(ErrorMessage = "Bạn chưa chọn Sinh viên!")]
    public string? StudentId { get; set; }

    [Display(Name = "Môn học")]
    [Required(ErrorMessage = "Bạn chưa chọn Môn học!")]
    public string? SubjectId { get; set; }

    // --- BẮT ĐẦU: CÁC CỘT ĐIỂM CHUẨN ĐẠI HỌC ---

    [Display(Name = "Điểm Giữa kì 1")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? MidtermScore1 { get; set; }

    [Display(Name = "Điểm Giữa kì 2")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? MidtermScore2 { get; set; }

    [Display(Name = "Điểm Cuối kì")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? FinalScore { get; set; }

    [Display(Name = "Điểm Tổng kết")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TotalScore { get; set; }

    [Display(Name = "Điểm Hệ Chữ")]
    public string? LetterGrade { get; set; } // A, B, C, D, F

    // --- KẾT THÚC ---

    [Display(Name = "Ngày thi")]
    public DateOnly? ExamDate { get; set; }

    [Display(Name = "Học kỳ")]
    [Required(ErrorMessage = "Vui lòng nhập Học kỳ (VD: Spring 2026)!")]
    public string? Semester { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }
}