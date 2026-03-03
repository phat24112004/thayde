using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;

namespace UniversityManagementSystem.Controllers
{
    public class SchoolClassesController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public SchoolClassesController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // GET: SchoolClasses
        //public async Task<IActionResult> Index()
        //{
        //    var universityManagementDbContext = _context.SchoolClasses.Include(s => s.Faculty);
        //    return View(await universityManagementDbContext.ToListAsync());
        //}

        // 1. ĐỔI string facultyId THÀNH int? facultyId
        public async Task<IActionResult> Index(string searchString, int? facultyId, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentFacultyId"] = facultyId;

            ViewData["FacultyList"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName", facultyId);

            var classes = _context.SchoolClasses.Include(s => s.Faculty).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(c => c.ClassName.Contains(searchString));
            }

            // 2. SỬA LẠI ĐOẠN NÀY DÙNG .HasValue VÀ .Value VÌ NÓ LÀ KIỂU SỐ (int?)
            if (facultyId.HasValue)
            {
                classes = classes.Where(c => c.FacultyId == facultyId.Value);
            }

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;

            int totalItems = await classes.CountAsync();
            int totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;

            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages = totalPages;

            var paginatedClasses = await classes.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(paginatedClasses);
        }

        // GET: SchoolClasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClass = await _context.SchoolClasses
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(m => m.ClassId == id);
            if (schoolClass == null)
            {
                return NotFound();
            }

            return View(schoolClass);
        }

        // GET: SchoolClasses/Create
        public IActionResult Create()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId");
            return View();
        }

        // POST: SchoolClasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassId,ClassName,FacultyId")] SchoolClass schoolClass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schoolClass);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới lớp học thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", schoolClass.FacultyId);
            return View(schoolClass);
        }

        // GET: SchoolClasses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClass = await _context.SchoolClasses.FindAsync(id);
            if (schoolClass == null)
            {
                return NotFound();
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", schoolClass.FacultyId);
            return View(schoolClass);
        }

        // POST: SchoolClasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClassId,ClassName,FacultyId")] SchoolClass schoolClass)
        {
            if (id != schoolClass.ClassId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schoolClass);
                    await _context.SaveChangesAsync();
                    // Đặt ngay sau dòng await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchoolClassExists(schoolClass.ClassId))
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
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", schoolClass.FacultyId);
            return View(schoolClass);
        }

        // GET: SchoolClasses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClass = await _context.SchoolClasses
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(m => m.ClassId == id);
            if (schoolClass == null)
            {
                return NotFound();
            }

            return View(schoolClass);
        }

        // POST: SchoolClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolClass = await _context.SchoolClasses.FindAsync(id);
            if (schoolClass != null)
            {
                _context.SchoolClasses.Remove(schoolClass);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa lớp học khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SchoolClassExists(int id)
        {
            return _context.SchoolClasses.Any(e => e.ClassId == id);
        }
    }
}
