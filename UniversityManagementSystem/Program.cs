using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- ĐOẠN ĐƯỢC THÊM VÀO ĐỂ KẾT NỐI DATABASE ---
            builder.Services.AddDbContext<UniversityManagementDbContext>(options =>
                options.UseSqlServer("Server=DESKTOP-N16L2CC;Database=UniversityManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));
            // ----------------------------------------------

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            // Lưu ý: Nếu dòng MapStaticAssets() này bị báo lỗi đỏ, hãy đổi nó thành app.UseStaticFiles();
            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}