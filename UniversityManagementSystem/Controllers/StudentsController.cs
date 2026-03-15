using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniversityManagementSystem.Models;

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
            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;

            int totalItems = await students.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages = totalPages;

            var paginatedStudents = await students.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(paginatedStudents);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName");
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ĐÃ THÊM GENDER VÀO DANH SÁCH BIND
        public async Task<IActionResult> Create([Bind("StudentId,FullName,DateOfBirth,Gender,Email,PhoneNumber,Address,ClassId,Status")] Student student)
        {
            ModelState.Remove("SchoolClass");
            ModelState.Remove("Grades");

            // Lúc này ModelState.IsValid sẽ tự động kiểm tra [Required] bên Model
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sinh viên mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", student.ClassId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", student.ClassId);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ĐÃ THÊM GENDER VÀO DANH SÁCH BIND
        public async Task<IActionResult> Edit(string id, [Bind("StudentId,FullName,DateOfBirth,Gender,Email,PhoneNumber,Address,ClassId,Status")] Student student)
        {
            if (id != student.StudentId) return NotFound();

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
                    if (!StudentExists(student.StudentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClassId"] = new SelectList(_context.SchoolClasses, "ClassId", "ClassName", student.ClassId);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var student = await _context.Students.FindAsync(id);
        //    if (student != null)
        //    {
        //        _context.Students.Remove(student);
        //        await _context.SaveChangesAsync();
        //        TempData["SuccessMessage"] = "Đã xóa sinh viên khỏi hệ thống!";
        //    }
        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id) // Giả sử StudentId là string
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa hồ sơ Sinh viên thành công!";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa Sinh viên này vì họ đã có Bảng điểm lưu trong hệ thống! Hãy thử chuyển trạng thái thành 'Thôi học' thay vì xóa.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        // Hàm xử lý Xuất file Excel (Đã cập nhật thêm Giới tính)
        public async Task<IActionResult> ExportToExcel()
        {
            var students = await _context.Students.Include(s => s.SchoolClass).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachSinhVien");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Mã SV";
                worksheet.Cell(currentRow, 2).Value = "Họ và Tên";
                worksheet.Cell(currentRow, 3).Value = "Ngày sinh";
                worksheet.Cell(currentRow, 4).Value = "Giới tính"; // Thêm cột Giới tính
                worksheet.Cell(currentRow, 5).Value = "Lớp học";
                worksheet.Cell(currentRow, 6).Value = "Email";
                worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                worksheet.Cell(currentRow, 8).Value = "Trạng thái";

                worksheet.Range("A1:H1").Style.Font.Bold = true;
                worksheet.Range("A1:H1").Style.Font.FontColor = XLColor.White;
                worksheet.Range("A1:H1").Style.Fill.BackgroundColor = XLColor.FromHtml("#1a73e8");

                foreach (var student in students)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = student.StudentId;
                    worksheet.Cell(currentRow, 2).Value = student.FullName;
                    worksheet.Cell(currentRow, 3).Value = student.DateOfBirth?.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 4).Value = student.Gender ?? "-"; // Đổ dữ liệu Giới tính
                    worksheet.Cell(currentRow, 5).Value = student.SchoolClass?.ClassName ?? "Chưa xếp lớp";
                    worksheet.Cell(currentRow, 6).Value = student.Email;
                    worksheet.Cell(currentRow, 7).Value = student.PhoneNumber;
                    worksheet.Cell(currentRow, 8).Value = student.Status;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachSinhVien.xlsx");
                }
            }
        }

        // ==========================================
        // TÍNH NĂNG MỚI: TẢI FILE EXCEL MẪU (TEMPLATE)
        // ==========================================
        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SinhVien_Template");

                // Dòng 1: Tiêu đề cột
                worksheet.Cell(1, 1).Value = "Mã Sinh viên (*)";
                worksheet.Cell(1, 2).Value = "Họ và Tên (*)";
                worksheet.Cell(1, 3).Value = "Ngày sinh (dd/MM/yyyy)";
                worksheet.Cell(1, 4).Value = "Giới tính (Nam/Nữ)";
                worksheet.Cell(1, 5).Value = "Email";
                worksheet.Cell(1, 6).Value = "Số điện thoại";
                worksheet.Cell(1, 7).Value = "Địa chỉ";
                worksheet.Cell(1, 8).Value = "Mã Lớp (*)"; // Phải nhập ClassId dạng số
                worksheet.Cell(1, 9).Value = "Trạng thái";

                // Định dạng Header
                var headerRange = worksheet.Range("A1:I1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#28a745"); // Màu xanh lá cho file mẫu

                // Dòng 2: Dữ liệu mẫu (Gợi ý cho người dùng)
                worksheet.Cell(2, 1).Value = "SV001";
                worksheet.Cell(2, 2).Value = "Nguyễn Văn A";
                worksheet.Cell(2, 3).Value = "15/08/2004";
                worksheet.Cell(2, 4).Value = "Nam";
                worksheet.Cell(2, 5).Value = "nva@gmail.com";
                worksheet.Cell(2, 6).Value = "0901234567";
                worksheet.Cell(2, 7).Value = "Hà Nội";
                worksheet.Cell(2, 8).Value = "1"; // VD ClassId = 1
                worksheet.Cell(2, 9).Value = "Đang học";

                // Tô màu xám nhạt cho dòng dữ liệu mẫu
                worksheet.Range("A2:I2").Style.Font.Italic = true;
                worksheet.Range("A2:I2").Style.Font.FontColor = XLColor.Gray;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_NhapSinhVien.xlsx");
                }
            }
        }

        // ==========================================
        // TÍNH NĂNG MỚI: IMPORT DỮ LIỆU TỪ EXCEL (BẢN NÂNG CẤP BẮT LỖI CHI TIẾT)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel để tải lên!";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["ErrorMessage"] = "Định dạng file không hợp lệ! Vui lòng chọn file .xlsx hoặc .xls";
                return RedirectToAction(nameof(Index));
            }

            int successCount = 0;
            List<string> errorDetails = new List<string>(); // Danh sách gom lỗi

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                        foreach (var row in rows)
                        {
                            int rowNumber = row.RowNumber();
                            string studentId = row.Cell(1).Value.ToString().Trim();
                            string fullName = row.Cell(2).Value.ToString().Trim();

                            // Bỏ qua dòng trống hoàn toàn
                            if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(fullName)) continue;

                            // Cố tình bỏ dòng mã giả mẫu (SV001)
                            if (rowNumber == 2 && studentId == "SV001") continue;

                            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(fullName))
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Bị trống Mã SV hoặc Họ tên.");
                                continue;
                            }

                            if (_context.Students.Any(s => s.StudentId == studentId))
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Mã SV '{studentId}' đã tồn tại.");
                                continue;
                            }

                            var newStudent = new Student
                            {
                                StudentId = studentId,
                                FullName = fullName,
                                Gender = row.Cell(4).Value.ToString().Trim(),
                                Email = row.Cell(5).Value.ToString().Trim(),
                                PhoneNumber = row.Cell(6).Value.ToString().Trim(),
                                Address = row.Cell(7).Value.ToString().Trim(),
                                Status = row.Cell(9).Value.ToString().Trim()
                            };

                            // ĐỌC NGÀY SINH AN TOÀN TỪ EXCEL
                            var dobCell = row.Cell(3);
                            if (!dobCell.IsEmpty())
                            {
                                if (dobCell.TryGetValue<DateTime>(out DateTime dateValue))
                                {
                                    // Excel nhận dạng đây là Date
                                    newStudent.DateOfBirth = DateOnly.FromDateTime(dateValue);
                                }
                                else
                                {
                                    // Excel nhận dạng đây là Text bình thường
                                    string dobString = dobCell.Value.ToString().Trim();
                                    if (DateOnly.TryParseExact(dobString, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate))
                                    {
                                        newStudent.DateOfBirth = parsedDate;
                                    }
                                    else
                                    {
                                        errorDetails.Add($"Dòng {rowNumber}: Ngày sinh sai định dạng (Chuẩn: dd/MM/yyyy).");
                                        continue;
                                    }
                                }
                            }

                            // ĐỌC MÃ LỚP AN TOÀN
                            string classIdString = row.Cell(8).Value.ToString().Trim();
                            if (int.TryParse(classIdString, out int classId))
                            {
                                if (_context.SchoolClasses.Any(c => c.ClassId == classId))
                                {
                                    newStudent.ClassId = classId;
                                }
                                else
                                {
                                    errorDetails.Add($"Dòng {rowNumber}: Mã lớp '{classId}' không tồn tại trong DB.");
                                    continue;
                                }
                            }
                            else
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Mã lớp phải là số nguyên.");
                                continue;
                            }

                            _context.Students.Add(newStudent);
                            successCount++;
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Import thành công {successCount} sinh viên mới!";
                }

                if (errorDetails.Count > 0)
                {
                    TempData["ErrorMessage"] = "Có " + errorDetails.Count + " lỗi:\n" + string.Join("\n", errorDetails.Take(5)) + (errorDetails.Count > 5 ? "\n..." : "");
                }
                else if (successCount == 0)
                {
                    TempData["ErrorMessage"] = "Không có sinh viên nào được thêm. Vui lòng kiểm tra lại file Excel.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống khi đọc file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}