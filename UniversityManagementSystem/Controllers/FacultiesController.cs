using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers
{
    public class FacultiesController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public FacultiesController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // GET: Faculties
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            var faculties = _context.Faculties.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                faculties = faculties.Where(f => f.FacultyName.Contains(searchString));
            }

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;
            int totalItems = await faculties.CountAsync();
            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            ViewBag.CurrentPage = pageIndex;

            return View(await faculties.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // GET: Faculties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.FacultyId == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }

        // GET: Faculties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Faculties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacultyName")] Faculty faculty)
        {
            // 1. Bỏ qua lỗi ID (vì DB tự tăng)
            ModelState.Remove("FacultyId");

            // 2. QUAN TRỌNG: Bỏ qua lỗi danh sách lớp (vì lúc tạo Khoa chưa có Lớp nào, nó bị null)
            ModelState.Remove("SchoolClasses");

            // Kiểm tra lại lần cuối
            if (ModelState.IsValid)
            {
                _context.Add(faculty);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm khoa mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu vẫn lỗi thì hiển thị lại form
            return View(faculty);
        }

        // GET: Faculties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }
            return View(faculty);
        }

        // POST: Faculties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FacultyId,FacultyName,EstablishedDate")] Faculty faculty)
        {
            if (id != faculty.FacultyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faculty);
                    await _context.SaveChangesAsync();
                    // Đặt ngay sau dòng await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin khoa thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacultyExists(faculty.FacultyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(faculty);
        }

        // GET: Faculties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.FacultyId == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }

        // POST: Faculties/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var faculty = await _context.Faculties.FindAsync(id);
        //    if (faculty != null)
        //    {
        //        _context.Faculties.Remove(faculty);
        //        await _context.SaveChangesAsync();
        //        // Đặt ngay sau dòng await _context.SaveChangesAsync();
        //        TempData["SuccessMessage"] = "Đã xóa khoa khỏi hệ thống!";
        //    }
        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id) // Thay 'int id' bằng kiểu dữ liệu đúng của bạn (nếu là chuỗi thì string id)
        {
            try
            {
                var faculty = await _context.Faculties.FindAsync(id);
                if (faculty != null)
                {
                    _context.Faculties.Remove(faculty);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa Khoa thành công!";
                }
            }
            catch (DbUpdateException) // BẮT LỖI KHÓA NGOẠI TẠI ĐÂY
            {
                TempData["ErrorMessage"] = "Không thể xóa! Khoa này đang chứa các Lớp học trực thuộc. Vui lòng chuyển hoặc xóa các Lớp học trước.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FacultyExists(int id)
        {
            return _context.Faculties.Any(e => e.FacultyId == id);
        }
    }
}
