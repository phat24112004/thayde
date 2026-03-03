using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniversityManagementSystem.Models; // Đảm bảo gọi Models vào đây

namespace UniversityManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Thêm biến _context để kết nối Database
        private readonly UniversityManagementDbContext _context;

        // Tiêm (Inject) DbContext vào hàm tạo
        public HomeController(ILogger<HomeController> logger, UniversityManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Đếm số lượng tổng (Code cũ của bạn)
            ViewBag.TotalFaculties = _context.Faculties.Count();
            ViewBag.TotalClasses = _context.SchoolClasses.Count();
            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.TotalSubjects = _context.Subjects.Count();

            // 2. LOGIC MỚI: Thống kê sinh viên theo Trạng thái
            var statusStats = _context.Students
                .GroupBy(s => s.Status)
                .Select(g => new
                {
                    StatusName = g.Key ?? "Chưa cập nhật",
                    Count = g.Count()
                })
                .ToList();

            // Đẩy mảng Tên trạng thái và mảng Số lượng ra View (chuyển sang JSON để Javascript đọc được)
            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.StatusName));
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(statusStats.Select(s => s.Count));

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