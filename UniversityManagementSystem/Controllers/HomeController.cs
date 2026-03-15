//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using UniversityManagementSystem.Models;

//namespace UniversityManagementSystem.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly UniversityManagementDbContext _context;

//        public HomeController(ILogger<HomeController> logger, UniversityManagementDbContext context)
//        {
//            _logger = logger;
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            // 1. Đếm số lượng tổng 
//            ViewBag.TotalFaculties = _context.Faculties.Count();
//            ViewBag.TotalClasses = _context.SchoolClasses.Count();
//            ViewBag.TotalStudents = _context.Students.Count();
//            ViewBag.TotalSubjects = _context.Subjects.Count();

//            // 2. Thống kê sinh viên theo Trạng thái (Cho Biểu đồ tròn)
//            var statusStats = _context.Students
//                .GroupBy(s => s.Status)
//                .Select(g => new
//                {
//                    StatusName = g.Key ?? "Chưa cập nhật",
//                    Count = g.Count()
//                })
//                .ToList();

//            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.StatusName));
//            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.Count));

//            // 3. LOGIC MỚI: Thống kê phổ điểm hệ chữ A, B, C, D, F (Cho Biểu đồ cột)
//            var gradeStats = _context.Grades
//                .Where(g => g.LetterGrade != null) // Lọc bỏ những môn chưa nhập điểm
//                .GroupBy(g => g.LetterGrade)
//                .Select(g => new
//                {
//                    Grade = g.Key,
//                    Count = g.Count()
//                })
//                .OrderBy(g => g.Grade) // Tự động sắp xếp A, B, C, D, F theo bảng chữ cái
//                .ToList();

//            ViewBag.GradeLabels = System.Text.Json.JsonSerializer.Serialize(gradeStats.Select(g => g.Grade));
//            ViewBag.GradeData = System.Text.Json.JsonSerializer.Serialize(gradeStats.Select(g => g.Count));

//            return View();
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UniversityManagementDbContext _context;

        public HomeController(ILogger<HomeController> logger, UniversityManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Đếm số lượng tổng 
            ViewBag.TotalFaculties = _context.Faculties.Count();
            ViewBag.TotalClasses = _context.SchoolClasses.Count();
            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.TotalSubjects = _context.Subjects.Count();

            // 2. Thống kê sinh viên theo Trạng thái (Cho Biểu đồ tròn)
            var statusStats = _context.Students
                .GroupBy(s => s.Status)
                .Select(g => new
                {
                    StatusName = g.Key ?? "Chưa cập nhật",
                    Count = g.Count()
                })
                .ToList();

            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.StatusName));
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.Count));

            // 3. LOGIC MỚI: Thống kê phổ điểm hệ chữ A, B, C, D, F (Cho Biểu đồ cột)
            var gradeStats = _context.Grades
                .Where(g => g.LetterGrade != null) // Lọc bỏ những môn chưa nhập điểm
                .GroupBy(g => g.LetterGrade)
                .Select(g => new
                {
                    Grade = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Grade) // Tự động sắp xếp A, B, C, D, F theo bảng chữ cái
                .ToList();

            ViewBag.GradeLabels = System.Text.Json.JsonSerializer.Serialize(gradeStats.Select(g => g.Grade));
            ViewBag.GradeData = System.Text.Json.JsonSerializer.Serialize(gradeStats.Select(g => g.Count));

            // 4. LOGIC BỔ SUNG: Thống kê số lượng sinh viên theo từng Khoa (Biểu đồ cột)
            var facultyStats = _context.Faculties
                .Select(f => new
                {
                    FacultyName = f.FacultyName,
                    StudentCount = f.SchoolClasses.SelectMany(c => c.Students).Count()
                })
                .ToList();

            ViewBag.FacultyLabels = System.Text.Json.JsonSerializer.Serialize(facultyStats.Select(f => f.FacultyName));
            ViewBag.FacultyData = System.Text.Json.JsonSerializer.Serialize(facultyStats.Select(f => f.StudentCount));

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}