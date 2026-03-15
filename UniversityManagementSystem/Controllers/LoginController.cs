//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using System.Linq;
//using UniversityManagementSystem.Models;

//namespace UniversityManagementSystem.Controllers
//{
//    public class LoginController : Controller
//    {
//        private readonly UniversityManagementDbContext _context;

//        public LoginController(UniversityManagementDbContext context)
//        {
//            _context = context;
//        }

//        // 1. Mở trang Đăng nhập
//        [HttpGet]
//        public IActionResult Index()
//        {
//            return View();
//        }

//        // 2. Xử lý khi bấm nút Đăng nhập
//        [HttpPost]
//        public IActionResult Index(string username, string password)
//        {
//            var user = _context.SystemUsers.SingleOrDefault(u => u.Username == username && u.PasswordHash == password);

//            if (user != null)
//            {
//                // Đăng nhập thành công -> Phát thẻ bài Session
//                HttpContext.Session.SetInt32("UserId", user.UserId);
//                HttpContext.Session.SetString("Username", user.Username);
//                HttpContext.Session.SetString("Role", user.Role ?? "User");

//                return RedirectToAction("Index", "Home"); // Chuyển hướng tới trang Sinh viên
//            }

//            // Đăng nhập thất bại -> Báo lỗi
//            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
//            return View();
//        }

//        // 3. Đăng xuất
//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Index", "Login");
//        }
//    }
//}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public LoginController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // 1. Mở trang Đăng nhập
        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập rồi thì cho vào trang chủ luôn
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. Xử lý khi bấm nút Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            ClaimsIdentity? identity = null;

            // 2.1 Kiểm tra tài khoản Admin (trong bảng SystemUsers)
            var adminUser = await _context.SystemUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (adminUser != null)
            {
                // Tạo "Căn cước công dân" cho Admin
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, adminUser.Username),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("FullName", adminUser.FullName ?? "Quản trị viên")
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                adminUser.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            else
            {
                // 2.2 Kiểm tra Sinh viên (Mật khẩu mặc định là 1234)
                var studentUser = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentId == username);

                if (studentUser != null && password == "1234")
                {
                    // Tạo "Thẻ sinh viên"
                    identity = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Name, studentUser.StudentId),
                        new Claim(ClaimTypes.Role, "Student"),
                        new Claim("FullName", studentUser.FullName)
                    }, CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }

            // Nếu thân phận hợp lệ -> Cấp Cookie đăng nhập
            if (identity != null)
            {
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }

            // Đăng nhập thất bại -> Báo lỗi
            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        // 3. Đăng xuất
        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie thay vì xóa Session
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        // 4. Trang thông báo khi cố tình truy cập link không đủ quyền
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}