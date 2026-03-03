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
    public class StudentsController : Controller
    {
        private readonly UniversityManagementDbContext _context;

        public StudentsController(UniversityManagementDbContext context)
        {
            _context = context;
        }

        // GET: Students
        //public async Task<IActionResult> Index()
        //{
        //    var universityManagementDbContext = _context.Students.Include(s => s.SchoolClass);
        //    return View(await universityManagementDbContext.ToListAsync());
        //}

        // Thêm biến searchString vào hàm Index
        // Thêm tham số pageNumber vào hàm
        public async Task<IActionResult> Index(string searchString, int? classId, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentClassId"] = classId;
            ViewData["ClassList"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", classId);

            var students = _context.Students.Include(s => s.SchoolClass).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FullName.Contains(searchString)
                                            || s.StudentId.Contains(searchString));
            }

            if (classId.HasValue)
            {
                students = students.Where(s => s.ClassId == classId.Value);
            }

            // --- BẮT ĐẦU LOGIC PHÂN TRANG ---
            int pageSize = 5; // Số lượng sinh viên hiển thị trên 1 trang
            int pageIndex = pageNumber ?? 1; // Nếu không có số trang thì mặc định là trang 1

            // Đếm tổng số sinh viên (sau khi đã lọc)
            int totalItems = await students.CountAsync();

            // Tính tổng số trang (Ví dụ: 12 SV / 5 = 2.4 -> Làm tròn lên là 3 trang)
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages = totalPages;

            // Cắt dữ liệu: Bỏ qua các trang trước, và Lấy đúng số lượng của trang hiện tại
            var paginatedStudents = await students.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(paginatedStudents);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            // Sửa chỗ này: "ClassId" -> "ClassName"
            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,FullName,DateOfBirth,Email,PhoneNumber,Address,ClassId,Status")] Student student)
        {
            // FIX QUAN TRỌNG 1: Bỏ qua lỗi kiểm tra 2 biến liên kết này
            ModelState.Remove("SchoolClass");
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                // Đặt ngay sau dòng await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sinh viên mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // FIX QUAN TRỌNG 2: 
            // Tham số thứ 3 đổi thành "ClassName" để dropdown hiện Tên Lớp (SE1701) thay vì hiện số (1, 2...)
            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", student.ClassId);

            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassId", student.ClassId);
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.ở 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("StudentId,FullName,DateOfBirth,Email,PhoneNumber,Address,ClassId,Status")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            // --- THÊM 2 DÒNG NÀY VÀO TRƯỚC IF ---
            // Xóa bỏ việc kiểm tra lỗi của các khóa ngoại (Bởi vì form Edit không gửi lên dữ liệu của toàn bộ Lớp và Điểm)
            ModelState.Remove("SchoolClass");
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin sinh viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentId))
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

            // --- SỬA LẠI CHỖ NÀY ---
            // Đổi "ClassId" thứ hai thành "ClassName" để Dropdown luôn hiện Tên lớp
            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", student.ClassId);

            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sinh viên khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        // Hàm xử lý Xuất file Excel
        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy toàn bộ danh sách sinh viên kèm theo thông tin Lớp học
            var students = await _context.Students.Include(s => s.SchoolClass).ToListAsync();

            // Khởi tạo một file Excel ảo
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachSinhVien");
                var currentRow = 1;

                // Tạo Tiêu đề các cột (Dòng 1)
                worksheet.Cell(currentRow, 1).Value = "Mã SV";
                worksheet.Cell(currentRow, 2).Value = "Họ và Tên";
                worksheet.Cell(currentRow, 3).Value = "Ngày sinh";
                worksheet.Cell(currentRow, 4).Value = "Lớp học";
                worksheet.Cell(currentRow, 5).Value = "Email";
                worksheet.Cell(currentRow, 6).Value = "Số điện thoại";
                worksheet.Cell(currentRow, 7).Value = "Trạng thái";

                // Định dạng in đậm và tô nền vàng cho Header
                worksheet.Range("A1:G1").Style.Font.Bold = true;
                worksheet.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.Yellow;

                // Đổ dữ liệu từ Database vào từng dòng
                foreach (var student in students)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = student.StudentId;
                    worksheet.Cell(currentRow, 2).Value = student.FullName;
                    worksheet.Cell(currentRow, 3).Value = student.DateOfBirth?.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 4).Value = student.SchoolClass?.ClassName ?? "Chưa xếp lớp";
                    worksheet.Cell(currentRow, 5).Value = student.Email;
                    worksheet.Cell(currentRow, 6).Value = student.PhoneNumber;
                    worksheet.Cell(currentRow, 7).Value = student.Status;
                }

                // Tự động kéo giãn độ rộng các cột cho vừa với độ dài chữ
                worksheet.Columns().AdjustToContents();

                // Trả file về cho trình duyệt để tải xuống
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachSinhVien.xlsx");
                }
            }
        }
    }
}

