//using Microsoft.EntityFrameworkCore;
//using UniversityManagementSystem.Models;
//using Microsoft.AspNetCore.Authentication.Cookies;

//namespace UniversityManagementSystem
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);

//            // --- ĐOẠN ĐƯỢC THÊM VÀO ĐỂ KẾT NỐI DATABASE ---
//            builder.Services.AddDbContext<UniversityManagementDbContext>(options =>
//                options.UseSqlServer("Server=DESKTOP-N16L2CC;Database=UniversityManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));
//            // ----------------------------------------------

//            // Add services to the container.
//            builder.Services.AddControllersWithViews();

//            // 1. CẤP PHÉP SỬ DỤNG SESSION TRONG HỆ THỐNG
//            builder.Services.AddSession(options => {
//                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thẻ bài hết hạn sau 30 phút không hoạt động
//                options.Cookie.HttpOnly = true;
//                options.Cookie.IsEssential = true;
//            }); // <--- THÊM DÒNG NÀY

//            var app = builder.Build();

//            // Configure the HTTP request pipeline.
//            if (!app.Environment.IsDevelopment())
//            {
//                app.UseExceptionHandler("/Home/Error");
//                app.UseHsts();
//            }

//            app.UseHttpsRedirection();
//            app.UseRouting();

//            // 2. KÍCH HOẠT SESSION CHO TỪNG REQUEST (Bắt buộc phải nằm TRƯỚC UseAuthorization)
//            app.UseSession(); // <--- THÊM DÒNG NÀY

//            app.UseAuthorization();

//            // Lưu ý: Nếu dòng MapStaticAssets() này bị báo lỗi đỏ, hãy đổi nó thành app.UseStaticFiles();
//            app.MapStaticAssets();

//            app.MapControllerRoute(
//                name: "default",
//                pattern: "{controller=Login}/{action=Index}/{id?}")
//                .WithStaticAssets();

//            app.Run();
//        }
//    }
//}
using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies; // <--- THÊM THƯ VIỆN NÀY

namespace UniversityManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- KẾT NỐI DATABASE ---
            builder.Services.AddDbContext<UniversityManagementDbContext>(options =>
                options.UseSqlServer("Server=DESKTOP-N16L2CC;Database=UniversityManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));

            builder.Services.AddControllersWithViews();

            // CẤP PHÉP SỬ DỤNG SESSION
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // --- BƯỚC 1: CẤU HÌNH BỘ KHÓA BẢO MẬT (AUTHENTICATION) ---
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index"; // Đường dẫn văng ra nếu chưa đăng nhập
                    options.AccessDeniedPath = "/Login/AccessDenied"; // Đường dẫn văng ra nếu không đủ quyền
                    options.ExpireTimeSpan = TimeSpan.FromHours(2); // Giữ đăng nhập trong 2 tiếng
                });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();

            // --- BƯỚC 2: KÍCH HOẠT BỘ KHÓA (Bắt buộc nằm trên UseAuthorization) ---
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            // Đổi lại trang chủ làm mặc định, bộ khóa sẽ tự lo việc chặn cửa
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}