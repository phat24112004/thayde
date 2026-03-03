using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagementSystem.Models;
using ClosedXML.Excel;
using System.IO;

namespace UniversityManagementSystem.Controllers
{
    public class GradesController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public GradesController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // GET: Grades
        public async Task<IActionResult> Index(string searchString, string subjectId, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSubjectId"] = subjectId;

            // Dropdown chọn Môn học
            ViewData["SubjectList"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", subjectId);

            var grades = _context.Grades.Include(g => g.Student).Include(g => g.Subject).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                // Tìm điểm theo Tên sinh viên
                grades = grades.Where(g => g.Student.FullName.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(subjectId))
            {
                // Lọc điểm theo Môn học
                grades = grades.Where(g => g.SubjectId == subjectId);
            }

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;
            int totalItems = await grades.CountAsync();
            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            ViewBag.CurrentPage = pageIndex;

            return View(await grades.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // GET: Grades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.GradeId == id);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // GET: Grades/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectId");
            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GradeId,StudentId,SubjectId,Score,ExamDate,Semester")] Grade grade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới điểm thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectId", grade.SubjectId);
            return View(grade);
        }

        // GET: Grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }
            // Đổi "StudentId" thứ 2 thành "FullName"
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);

            // Đổi "SubjectId" thứ 2 thành "SubjectName"
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
            return View(grade);
        }

        // POST: Grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GradeId,StudentId,SubjectId,Score,ExamDate,Semester")] Grade grade)
        {
            if (id != grade.GradeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                    // Đặt ngay sau dòng await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.GradeId))
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
            // Đổi "StudentId" thứ 2 thành "FullName"
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);

            // Đổi "SubjectId" thứ 2 thành "SubjectName"
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
            return View(grade);
        }

        // GET: Grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.GradeId == id);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa điểm khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }
        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.GradeId == id);
        }
        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy danh sách Điểm kèm theo thông tin Sinh viên và Môn học
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();

            // Khai báo biến chứa dữ liệu file ở ngoài cùng
            byte[] content;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDiem");
                var currentRow = 1;

                // Tạo Tiêu đề các cột
                worksheet.Cell(currentRow, 1).Value = "Mã SV";
                worksheet.Cell(currentRow, 2).Value = "Tên Sinh viên";
                worksheet.Cell(currentRow, 3).Value = "Môn học";
                worksheet.Cell(currentRow, 4).Value = "Điểm số";

                // Tô màu nền vàng, in đậm cho Tiêu đề
                worksheet.Range("A1:D1").Style.Font.Bold = true;
                worksheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.Yellow;

                // Đổ dữ liệu vào từng dòng
                foreach (var grade in grades)
                {
                    currentRow++;
                    // Lấy thông tin từ các bảng liên kết qua
                    worksheet.Cell(currentRow, 1).Value = grade.Student?.StudentId ?? "N/A";
                    worksheet.Cell(currentRow, 2).Value = grade.Student?.FullName ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = grade.Subject?.SubjectName ?? "N/A";

                    // Chú ý: Nếu cột điểm của bạn tên khác (ví dụ: Score, Value...), hãy đổi lại chữ grade.Score cho đúng nhé
                    worksheet.Cell(currentRow, 4).Value = grade.Score;
                }

                // Tự động căn chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // Lưu ra bộ nhớ ảo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    content = stream.ToArray();
                }
            }

            // Trả file về cho trình duyệt
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDiem.xlsx");
        }
    }
}
