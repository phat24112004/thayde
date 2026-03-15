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
    public class SubjectsController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public SubjectsController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // GET: Subjects
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            var subjects = _context.Subjects.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                // Cho phép tìm theo cả Mã môn hoặc Tên môn
                subjects = subjects.Where(s => s.SubjectName.Contains(searchString) || s.SubjectId.Contains(searchString));
            }

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;
            int totalItems = await subjects.CountAsync();
            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            ViewBag.CurrentPage = pageIndex;

            return View(await subjects.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(m => m.SubjectId == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // GET: Subjects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectId,SubjectName")] Subject subject)
        {
            // FIX QUAN TRỌNG: Bỏ qua lỗi kiểm tra danh sách điểm
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới môn học thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SubjectId,SubjectName,Credits")] Subject subject)
        {
            if (id != subject.SubjectId)
            {
                return NotFound();
            }

            // FIX QUAN TRỌNG: Gỡ chốt chặn danh sách Điểm để hệ thống cho phép lưu
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin môn học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.SubjectId))
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
            return View(subject);
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(m => m.SubjectId == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: Subjects/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var subject = await _context.Subjects.FindAsync(id);
        //    if (subject != null)
        //    {
        //        _context.Subjects.Remove(subject);
        //        await _context.SaveChangesAsync();
        //        // Đặt ngay sau dòng await _context.SaveChangesAsync();
        //        TempData["SuccessMessage"] = "Đã xóa môn học khỏi hệ thống!";
        //    }
        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id) // Giả sử SubjectId của bạn là string
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject != null)
                {
                    _context.Subjects.Remove(subject);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa Môn học thành công!";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa Môn học này! Hệ thống phát hiện đã có Sinh viên được nhập điểm cho môn này.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(string id)
        {
            return _context.Subjects.Any(e => e.SubjectId == id);
        }
    }
}
