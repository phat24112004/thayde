//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using UniversityManagementSystem.Models;
//using ClosedXML.Excel;
//using System.IO;

//namespace UniversityManagementSystem.Controllers
//{
//    public class GradesController : Controller
//    {
//        private readonly UniversityManagementDbContext _context;

//        public GradesController(UniversityManagementDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Grades
//        public async Task<IActionResult> Index(string searchString, string subjectId, int? pageNumber)
//        {
//            ViewData["CurrentFilter"] = searchString;
//            ViewData["CurrentSubjectId"] = subjectId;

//            // Dropdown chọn Môn học
//            ViewData["SubjectList"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", subjectId);

//            var grades = _context.Grades.Include(g => g.Student).Include(g => g.Subject).AsQueryable();

//            if (!String.IsNullOrEmpty(searchString))
//            {
//                // Tìm điểm theo Tên sinh viên
//                grades = grades.Where(g => g.Student.FullName.Contains(searchString));
//            }

//            if (!String.IsNullOrEmpty(subjectId))
//            {
//                // Lọc điểm theo Môn học
//                grades = grades.Where(g => g.SubjectId == subjectId);
//            }

//            int pageSize = 5;
//            int pageIndex = pageNumber ?? 1;
//            int totalItems = await grades.CountAsync();
//            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
//            ViewBag.CurrentPage = pageIndex;

//            return View(await grades.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
//        }

//        // GET: Grades/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var grade = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .FirstOrDefaultAsync(m => m.GradeId == id);
//            if (grade == null)
//            {
//                return NotFound();
//            }

//            return View(grade);
//        }

//        // GET: Grades/Create
//        public IActionResult Create()
//        {
//            // Đổi tham số thứ 3 từ "StudentId" thành "FullName" để hiển thị Tên Sinh viên
//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName");

//            // Đổi tham số thứ 3 từ "SubjectId" thành "SubjectName" để hiển thị Tên Môn học
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName");

//            return View();
//        }

//        // POST: Grades/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
//        {
//            if (ModelState.IsValid)
//            {
//                // Gọi hàm tính điểm trước khi lưu
//                CalculateFinalGrade(grade);

//                _context.Add(grade);
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = "Thêm mới điểm thành công!";
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // GET: Grades/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            var grade = await _context.Grades.FindAsync(id);
//            if (grade == null) return NotFound();

//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // POST: Grades/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
//        {
//            if (id != grade.GradeId) return NotFound();

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Gọi hàm tính điểm trước khi cập nhật
//                    CalculateFinalGrade(grade);

//                    _context.Update(grade);
//                    await _context.SaveChangesAsync();
//                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!GradeExists(grade.GradeId)) return NotFound();
//                    else throw;
//                }
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // ==========================================
//        // HÀM NGHIỆP VỤ: TÍNH ĐIỂM VÀ QUY ĐỔI HỆ CHỮ
//        // ==========================================
//        private void CalculateFinalGrade(Grade grade)
//        {
//            decimal midtermAvg = 0;

//            // 1. Tính trung bình điểm quá trình
//            if (grade.MidtermScore1.HasValue && grade.MidtermScore2.HasValue)
//            {
//                midtermAvg = (grade.MidtermScore1.Value + grade.MidtermScore2.Value) / 2m;
//            }
//            else if (grade.MidtermScore1.HasValue)
//            {
//                midtermAvg = grade.MidtermScore1.Value;
//            }
//            else if (grade.MidtermScore2.HasValue)
//            {
//                midtermAvg = grade.MidtermScore2.Value;
//            }

//            // 2. Tính điểm tổng kết (Hệ 10) & Quy đổi Điểm Chữ
//            if (grade.FinalScore.HasValue)
//            {
//                // Công thức: 30% Quá trình + 70% Cuối kì. Làm tròn 2 chữ số thập phân.
//                grade.TotalScore = Math.Round((midtermAvg * 0.3m) + (grade.FinalScore.Value * 0.7m), 2);

//                // Quy đổi theo chuẩn Đại học
//                if (grade.TotalScore >= 8.5m) grade.LetterGrade = "A";
//                else if (grade.TotalScore >= 7.0m) grade.LetterGrade = "B";
//                else if (grade.TotalScore >= 5.5m) grade.LetterGrade = "C";
//                else if (grade.TotalScore >= 4.0m) grade.LetterGrade = "D";
//                else grade.LetterGrade = "F";
//            }
//            else
//            {
//                // Nếu chưa có điểm cuối kì thì chưa tính tổng kết
//                grade.TotalScore = null;
//                grade.LetterGrade = null;
//            }
//        }

//        // GET: Grades/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var grade = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .FirstOrDefaultAsync(m => m.GradeId == id);
//            if (grade == null)
//            {
//                return NotFound();
//            }

//            return View(grade);
//        }

//        // POST: Grades/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var grade = await _context.Grades.FindAsync(id);
//            if (grade != null)
//            {
//                _context.Grades.Remove(grade);
//                await _context.SaveChangesAsync();
//                // Đặt ngay sau dòng await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = "Đã xóa điểm khỏi hệ thống!";
//            }
//            return RedirectToAction(nameof(Index));
//        }
//        private bool GradeExists(int id)
//        {
//            return _context.Grades.Any(e => e.GradeId == id);
//        }
//        public async Task<IActionResult> ExportToExcel()
//        {
//            // Lấy danh sách Điểm kèm theo thông tin Sinh viên và Môn học
//            var grades = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .ToListAsync();

//            // Khai báo biến chứa dữ liệu file ở ngoài cùng
//            byte[] content;

//            using (var workbook = new XLWorkbook())
//            {
//                var worksheet = workbook.Worksheets.Add("DanhSachDiem");
//                var currentRow = 1;

//                // BỔ SUNG ĐẦY ĐỦ TIÊU ĐỀ CÁC CỘT CHUẨN ĐẠI HỌC
//                worksheet.Cell(currentRow, 1).Value = "Mã SV";
//                worksheet.Cell(currentRow, 2).Value = "Tên Sinh viên";
//                worksheet.Cell(currentRow, 3).Value = "Môn học";
//                worksheet.Cell(currentRow, 4).Value = "Học kỳ";
//                worksheet.Cell(currentRow, 5).Value = "Ngày thi";
//                worksheet.Cell(currentRow, 6).Value = "Giữa kì 1";
//                worksheet.Cell(currentRow, 7).Value = "Giữa kì 2";
//                worksheet.Cell(currentRow, 8).Value = "Cuối kì";
//                worksheet.Cell(currentRow, 9).Value = "Tổng kết (Hệ 10)";
//                worksheet.Cell(currentRow, 10).Value = "Điểm Chữ";

//                // Style cho Tiêu đề: Nền xanh dương, chữ trắng, in đậm, căn giữa
//                var headerRange = worksheet.Range("A1:J1");
//                headerRange.Style.Font.Bold = true;
//                headerRange.Style.Font.FontColor = XLColor.White;
//                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a73e8");
//                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

//                // Đổ dữ liệu vào từng dòng
//                foreach (var grade in grades)
//                {
//                    currentRow++;
//                    // Lấy thông tin từ các bảng liên kết
//                    worksheet.Cell(currentRow, 1).Value = grade.Student?.StudentId ?? "N/A";
//                    worksheet.Cell(currentRow, 2).Value = grade.Student?.FullName ?? "N/A";
//                    worksheet.Cell(currentRow, 3).Value = grade.Subject?.SubjectName ?? "N/A";
//                    worksheet.Cell(currentRow, 4).Value = grade.Semester ?? "-";

//                    // Ép kiểu hiển thị ngày tháng
//                    worksheet.Cell(currentRow, 5).Value = grade.ExamDate.HasValue ? grade.ExamDate.Value.ToString("dd/MM/yyyy") : "-";

//                    // Lấy dữ liệu điểm (Nếu null thì tự động để trống)
//                    worksheet.Cell(currentRow, 6).Value = grade.MidtermScore1;
//                    worksheet.Cell(currentRow, 7).Value = grade.MidtermScore2;
//                    worksheet.Cell(currentRow, 8).Value = grade.FinalScore;
//                    worksheet.Cell(currentRow, 9).Value = grade.TotalScore;
//                    worksheet.Cell(currentRow, 10).Value = grade.LetterGrade ?? "-";
//                }

//                // Căn giữa nội dung cho các cột Điểm số, Ngày thi, Học kỳ cho đẹp mắt
//                worksheet.Range($"D2:J{currentRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

//                // Tự động căn chỉnh độ rộng tất cả các cột cho vừa vặn text
//                worksheet.Columns().AdjustToContents();

//                // Lưu ra bộ nhớ ảo
//                using (var stream = new MemoryStream())
//                {
//                    workbook.SaveAs(stream);
//                    content = stream.ToArray();
//                }
//            }

//            // Trả file về cho trình duyệt (Đổi tên file cho ngầu hơn)
//            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiem_DaiHoc.xlsx");
//        }
//    }
//}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using UniversityManagementSystem.Models;
//using ClosedXML.Excel;
//using System.IO;
//using Microsoft.AspNetCore.Authorization;

//namespace UniversityManagementSystem.Controllers
//{
//    [Authorize]
//    public class GradesController : Controller
//    {
//        private readonly UniversityManagementDbContext _context;

//        public GradesController(UniversityManagementDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Grades
//        public async Task<IActionResult> Index(string searchString, string subjectId, int? pageNumber)
//        {
//            ViewData["CurrentFilter"] = searchString;
//            ViewData["CurrentSubjectId"] = subjectId;

//            // Dropdown chọn Môn học
//            ViewData["SubjectList"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", subjectId);

//            var grades = _context.Grades.Include(g => g.Student).Include(g => g.Subject).AsQueryable();

//            if (!String.IsNullOrEmpty(searchString))
//            {
//                // Tìm điểm theo Tên sinh viên
//                grades = grades.Where(g => g.Student.FullName.Contains(searchString));
//            }

//            if (!String.IsNullOrEmpty(subjectId))
//            {
//                // Lọc điểm theo Môn học
//                grades = grades.Where(g => g.SubjectId == subjectId);
//            }

//            int pageSize = 5;
//            int pageIndex = pageNumber ?? 1;
//            int totalItems = await grades.CountAsync();
//            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
//            ViewBag.CurrentPage = pageIndex;

//            return View(await grades.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
//        }

//        // GET: Grades/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null) return NotFound();

//            var grade = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .FirstOrDefaultAsync(m => m.GradeId == id);

//            if (grade == null) return NotFound();

//            return View(grade);
//        }

//        // GET: Grades/Create
//        public IActionResult Create()
//        {
//            // CHỐT CHẶN 1: Chỉ lấy danh sách những Sinh viên đang trong trạng thái "Đang học"
//            var activeStudents = _context.Students.Where(s => s.Status == "Đang học").ToList();

//            ViewData["StudentId"] = new SelectList(activeStudents, "StudentId", "FullName");
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName");

//            return View();
//        }

//        // POST: Grades/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
//        {
//            if (ModelState.IsValid)
//            {
//                // CHỐT CHẶN 2: Kiểm tra lại lần nữa trước khi lưu vào DB đề phòng user hack HTML
//                var student = await _context.Students.FindAsync(grade.StudentId);
//                if (student != null && student.Status != "Đang học")
//                {
//                    TempData["ErrorMessage"] = $"Lỗi: Không thể nhập điểm! Sinh viên {student.FullName} hiện đang trong trạng thái '{student.Status}'.";
//                    return RedirectToAction(nameof(Index));
//                }

//                CalculateFinalGrade(grade);
//                _context.Add(grade);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Thêm mới điểm thành công!";
//                return RedirectToAction(nameof(Index));
//            }

//            // Nếu form bị lỗi, đổ lại danh sách sinh viên đang học
//            var activeStudents = _context.Students.Where(s => s.Status == "Đang học").ToList();
//            ViewData["StudentId"] = new SelectList(activeStudents, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // GET: Grades/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            var grade = await _context.Grades.Include(g => g.Student).FirstOrDefaultAsync(g => g.GradeId == id);
//            if (grade == null) return NotFound();

//            // CHỐT CHẶN 3: Chặn ngay từ cửa nếu bấm nút "Sửa" trên một sinh viên đã thôi học
//            if (grade.Student != null && grade.Student.Status != "Đang học")
//            {
//                TempData["ErrorMessage"] = $"Lỗi: Hệ thống đã đóng băng hồ sơ! Sinh viên {grade.Student.FullName} đang trong trạng thái '{grade.Student.Status}'. Không thể sửa điểm.";
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // POST: Grades/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
//        {
//            if (id != grade.GradeId) return NotFound();

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // CHỐT CHẶN 4: Kiểm tra trạng thái Sinh viên trước khi lưu đè dữ liệu
//                    var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == grade.StudentId);
//                    if (student != null && student.Status != "Đang học")
//                    {
//                        TempData["ErrorMessage"] = $"Lỗi bảo mật: Sinh viên đang trong trạng thái '{student.Status}'. Không cho phép cập nhật dữ liệu.";
//                        return RedirectToAction(nameof(Index));
//                    }

//                    CalculateFinalGrade(grade);
//                    _context.Update(grade);
//                    await _context.SaveChangesAsync();

//                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!GradeExists(grade.GradeId)) return NotFound();
//                    else throw;
//                }
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
//            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
//            return View(grade);
//        }

//        // ==========================================
//        // HÀM NGHIỆP VỤ: TÍNH ĐIỂM VÀ QUY ĐỔI HỆ CHỮ
//        // ==========================================
//        private void CalculateFinalGrade(Grade grade)
//        {
//            decimal midtermAvg = 0;

//            if (grade.MidtermScore1.HasValue && grade.MidtermScore2.HasValue)
//            {
//                midtermAvg = (grade.MidtermScore1.Value + grade.MidtermScore2.Value) / 2m;
//            }
//            else if (grade.MidtermScore1.HasValue)
//            {
//                midtermAvg = grade.MidtermScore1.Value;
//            }
//            else if (grade.MidtermScore2.HasValue)
//            {
//                midtermAvg = grade.MidtermScore2.Value;
//            }

//            if (grade.FinalScore.HasValue)
//            {
//                grade.TotalScore = Math.Round((midtermAvg * 0.3m) + (grade.FinalScore.Value * 0.7m), 2);

//                if (grade.TotalScore >= 8.5m) grade.LetterGrade = "A";
//                else if (grade.TotalScore >= 7.0m) grade.LetterGrade = "B";
//                else if (grade.TotalScore >= 5.5m) grade.LetterGrade = "C";
//                else if (grade.TotalScore >= 4.0m) grade.LetterGrade = "D";
//                else grade.LetterGrade = "F";
//            }
//            else
//            {
//                grade.TotalScore = null;
//                grade.LetterGrade = null;
//            }
//        }

//        // GET: Grades/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null) return NotFound();

//            var grade = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .FirstOrDefaultAsync(m => m.GradeId == id);

//            if (grade == null) return NotFound();

//            return View(grade);
//        }

//        // POST: Grades/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var grade = await _context.Grades.FindAsync(id);
//            if (grade != null)
//            {
//                _context.Grades.Remove(grade);
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = "Đã xóa điểm khỏi hệ thống!";
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        private bool GradeExists(int id)
//        {
//            return _context.Grades.Any(e => e.GradeId == id);
//        }

//        public async Task<IActionResult> ExportToExcel()
//        {
//            var grades = await _context.Grades
//                .Include(g => g.Student)
//                .Include(g => g.Subject)
//                .ToListAsync();

//            byte[] content;

//            using (var workbook = new XLWorkbook())
//            {
//                var worksheet = workbook.Worksheets.Add("DanhSachDiem");
//                var currentRow = 1;

//                worksheet.Cell(currentRow, 1).Value = "Mã SV";
//                worksheet.Cell(currentRow, 2).Value = "Tên Sinh viên";
//                worksheet.Cell(currentRow, 3).Value = "Môn học";
//                worksheet.Cell(currentRow, 4).Value = "Học kỳ";
//                worksheet.Cell(currentRow, 5).Value = "Ngày thi";
//                worksheet.Cell(currentRow, 6).Value = "Giữa kì 1";
//                worksheet.Cell(currentRow, 7).Value = "Giữa kì 2";
//                worksheet.Cell(currentRow, 8).Value = "Cuối kì";
//                worksheet.Cell(currentRow, 9).Value = "Tổng kết (Hệ 10)";
//                worksheet.Cell(currentRow, 10).Value = "Điểm Chữ";

//                var headerRange = worksheet.Range("A1:J1");
//                headerRange.Style.Font.Bold = true;
//                headerRange.Style.Font.FontColor = XLColor.White;
//                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a73e8");
//                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

//                foreach (var grade in grades)
//                {
//                    currentRow++;
//                    worksheet.Cell(currentRow, 1).Value = grade.Student?.StudentId ?? "N/A";
//                    worksheet.Cell(currentRow, 2).Value = grade.Student?.FullName ?? "N/A";
//                    worksheet.Cell(currentRow, 3).Value = grade.Subject?.SubjectName ?? "N/A";
//                    worksheet.Cell(currentRow, 4).Value = grade.Semester ?? "-";
//                    worksheet.Cell(currentRow, 5).Value = grade.ExamDate.HasValue ? grade.ExamDate.Value.ToString("dd/MM/yyyy") : "-";
//                    worksheet.Cell(currentRow, 6).Value = grade.MidtermScore1;
//                    worksheet.Cell(currentRow, 7).Value = grade.MidtermScore2;
//                    worksheet.Cell(currentRow, 8).Value = grade.FinalScore;
//                    worksheet.Cell(currentRow, 9).Value = grade.TotalScore;
//                    worksheet.Cell(currentRow, 10).Value = grade.LetterGrade ?? "-";
//                }

//                worksheet.Range($"D2:J{currentRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                worksheet.Columns().AdjustToContents();

//                using (var stream = new MemoryStream())
//                {
//                    workbook.SaveAs(stream);
//                    content = stream.ToArray();
//                }
//            }

//            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiem_DaiHoc.xlsx");
//        }

//        // ==========================================
//        // TÍNH NĂNG MỚI: TẢI FILE EXCEL MẪU (TEMPLATE) CHO ĐIỂM SỐ
//        // ==========================================
//        public IActionResult DownloadTemplate()
//        {
//            using (var workbook = new XLWorkbook())
//            {
//                var worksheet = workbook.Worksheets.Add("NhapDiem_Template");

//                worksheet.Cell(1, 1).Value = "Mã Sinh viên (*)";
//                worksheet.Cell(1, 2).Value = "Mã Môn học (*)";
//                worksheet.Cell(1, 3).Value = "Học kỳ (*)";
//                worksheet.Cell(1, 4).Value = "Ngày thi (dd/MM/yyyy)";
//                worksheet.Cell(1, 5).Value = "Giữa kì 1 (0-10)";
//                worksheet.Cell(1, 6).Value = "Giữa kì 2 (0-10)";
//                worksheet.Cell(1, 7).Value = "Cuối kì (0-10)";

//                var headerRange = worksheet.Range("A1:G1");
//                headerRange.Style.Font.Bold = true;
//                headerRange.Style.Font.FontColor = XLColor.White;
//                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");

//                // Dữ liệu mẫu gợi ý
//                worksheet.Cell(2, 1).Value = "SV001";
//                worksheet.Cell(2, 2).Value = "INT140";
//                worksheet.Cell(2, 3).Value = "Spring 2026";
//                worksheet.Cell(2, 4).Value = "20/05/2026";
//                worksheet.Cell(2, 5).Value = 8.5;
//                worksheet.Cell(2, 6).Value = 9.0;
//                worksheet.Cell(2, 7).Value = 8.0;

//                worksheet.Range("A2:G2").Style.Font.Italic = true;
//                worksheet.Range("A2:G2").Style.Font.FontColor = XLColor.Gray;

//                worksheet.Columns().AdjustToContents();

//                using (var stream = new MemoryStream())
//                {
//                    workbook.SaveAs(stream);
//                    var content = stream.ToArray();
//                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_NhapDiem.xlsx");
//                }
//            }
//        }

//        // ==========================================
//        // TÍNH NĂNG MỚI: IMPORT ĐIỂM SỐ TỪ EXCEL
//        // ==========================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ImportExcel(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//            {
//                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel để tải lên!";
//                return RedirectToAction(nameof(Index));
//            }

//            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
//            {
//                TempData["ErrorMessage"] = "Định dạng file không hợp lệ! Vui lòng chọn file .xlsx hoặc .xls";
//                return RedirectToAction(nameof(Index));
//            }

//            int successCount = 0;
//            List<string> errorDetails = new List<string>();

//            try
//            {
//                using (var stream = new MemoryStream())
//                {
//                    await file.CopyToAsync(stream);
//                    using (var workbook = new XLWorkbook(stream))
//                    {
//                        var worksheet = workbook.Worksheet(1);
//                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

//                        foreach (var row in rows)
//                        {
//                            int rowNumber = row.RowNumber();
//                            string studentId = row.Cell(1).Value.ToString().Trim();
//                            string subjectId = row.Cell(2).Value.ToString().Trim();
//                            string semester = row.Cell(3).Value.ToString().Trim();

//                            // Bỏ qua dòng trống và dòng mẫu
//                            if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(subjectId)) continue;
//                            if (rowNumber == 2 && studentId == "SV001") continue;

//                            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(semester))
//                            {
//                                errorDetails.Add($"Dòng {rowNumber}: Thiếu Mã SV, Mã Môn hoặc Học kỳ.");
//                                continue;
//                            }

//                            // Chốt chặn 1: Kiểm tra sinh viên có tồn tại và đang học không
//                            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == studentId);
//                            if (student == null)
//                            {
//                                errorDetails.Add($"Dòng {rowNumber}: Mã SV '{studentId}' không tồn tại.");
//                                continue;
//                            }
//                            if (student.Status != "Đang học")
//                            {
//                                errorDetails.Add($"Dòng {rowNumber}: SV '{studentId}' đang '{student.Status}', không thể nhập điểm.");
//                                continue;
//                            }

//                            // Chốt chặn 2: Kiểm tra Môn học có tồn tại không
//                            if (!_context.Subjects.Any(s => s.SubjectId == subjectId))
//                            {
//                                errorDetails.Add($"Dòng {rowNumber}: Mã Môn '{subjectId}' không tồn tại.");
//                                continue;
//                            }

//                            // Chốt chặn 3: Kiểm tra xem SV này đã có điểm môn này trong kỳ này chưa (tránh trùng)
//                            if (_context.Grades.Any(g => g.StudentId == studentId && g.SubjectId == subjectId && g.Semester == semester))
//                            {
//                                errorDetails.Add($"Dòng {rowNumber}: SV '{studentId}' đã có điểm môn '{subjectId}' học kỳ '{semester}'.");
//                                continue;
//                            }

//                            var newGrade = new Grade
//                            {
//                                StudentId = studentId,
//                                SubjectId = subjectId,
//                                Semester = semester
//                            };

//                            // Xử lý Ngày thi
//                            var dateCell = row.Cell(4);
//                            if (!dateCell.IsEmpty())
//                            {
//                                if (dateCell.TryGetValue<DateTime>(out DateTime dateValue))
//                                {
//                                    newGrade.ExamDate = DateOnly.FromDateTime(dateValue);
//                                }
//                                else if (DateOnly.TryParseExact(dateCell.Value.ToString().Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate))
//                                {
//                                    newGrade.ExamDate = parsedDate;
//                                }
//                                else
//                                {
//                                    errorDetails.Add($"Dòng {rowNumber}: Ngày thi sai định dạng (Chuẩn: dd/MM/yyyy).");
//                                    continue;
//                                }
//                            }

//                            // Xử lý đọc Điểm số an toàn
//                            if (decimal.TryParse(row.Cell(5).Value.ToString(), out decimal m1)) newGrade.MidtermScore1 = m1;
//                            if (decimal.TryParse(row.Cell(6).Value.ToString(), out decimal m2)) newGrade.MidtermScore2 = m2;
//                            if (decimal.TryParse(row.Cell(7).Value.ToString(), out decimal final)) newGrade.FinalScore = final;

//                            // TỰ ĐỘNG TÍNH ĐIỂM TỔNG KẾT VÀ ĐIỂM CHỮ
//                            CalculateFinalGrade(newGrade);

//                            _context.Grades.Add(newGrade);
//                            successCount++;
//                        }

//                        await _context.SaveChangesAsync();
//                    }
//                }

//                if (successCount > 0) TempData["SuccessMessage"] = $"Import thành công {successCount} bảng điểm mới!";

//                if (errorDetails.Count > 0)
//                {
//                    TempData["ErrorMessage"] = "Có " + errorDetails.Count + " dòng lỗi:\n" + string.Join("\n", errorDetails.Take(5)) + (errorDetails.Count > 5 ? "\n..." : "");
//                }
//                else if (successCount == 0)
//                {
//                    TempData["ErrorMessage"] = "Không có điểm nào được thêm. Vui lòng kiểm tra lại file Excel.";
//                }
//            }
//            catch (Exception ex)
//            {
//                TempData["ErrorMessage"] = $"Lỗi hệ thống khi đọc file: {ex.Message}";
//            }

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
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
using Microsoft.AspNetCore.Authorization;

namespace UniversityManagementSystem.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới được vào Controller này
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

            // --- BỘ LỌC BẢO MẬT ---
            if (User.IsInRole("Student"))
            {
                // Nếu là Sinh viên: Ép buộc chỉ lấy điểm của chính sinh viên đó
                string currentStudentId = User.Identity?.Name;
                grades = grades.Where(g => g.StudentId == currentStudentId);
            }
            else
            {
                // Nếu là Admin: Cho phép dùng thanh tìm kiếm theo tên tất cả sinh viên
                if (!String.IsNullOrEmpty(searchString))
                {
                    grades = grades.Where(g => g.Student.FullName.Contains(searchString));
                }
            }

            if (!String.IsNullOrEmpty(subjectId))
            {
                // Lọc điểm theo Môn học
                grades = grades.Where(g => g.SubjectId == subjectId);
            }

            int pageSize = 6; // Để 6 dòng cho đẹp giao diện
            int pageIndex = pageNumber ?? 1;
            int totalItems = await grades.CountAsync();
            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            ViewBag.CurrentPage = pageIndex;

            return View(await grades.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // GET: Grades/Details/5 (Hàm này ai cũng được xem)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.GradeId == id);

            if (grade == null) return NotFound();

            return View(grade);
        }

        // GET: Grades/Create
        [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC VÀO
        public IActionResult Create()
        {
            // CHỐT CHẶN 1: Chỉ lấy danh sách những Sinh viên đang trong trạng thái "Đang học"
            var activeStudents = _context.Students.Where(s => s.Status == "Đang học").ToList();

            ViewData["StudentId"] = new SelectList(activeStudents, "StudentId", "FullName");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName");

            return View();
        }

        // POST: Grades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC LƯU
        public async Task<IActionResult> Create([Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
        {
            if (ModelState.IsValid)
            {
                // CHỐT CHẶN 2: Kiểm tra lại lần nữa trước khi lưu vào DB đề phòng user hack HTML
                var student = await _context.Students.FindAsync(grade.StudentId);
                if (student != null && student.Status != "Đang học")
                {
                    TempData["ErrorMessage"] = $"Lỗi: Không thể nhập điểm! Sinh viên {student.FullName} hiện đang trong trạng thái '{student.Status}'.";
                    return RedirectToAction(nameof(Index));
                }

                CalculateFinalGrade(grade);
                _context.Add(grade);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm mới điểm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu form bị lỗi, đổ lại danh sách sinh viên đang học
            var activeStudents = _context.Students.Where(s => s.Status == "Đang học").ToList();
            ViewData["StudentId"] = new SelectList(activeStudents, "StudentId", "FullName", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
            return View(grade);
        }

        // GET: Grades/Edit/5
        [Authorize(Roles = "Admin")] // CHỈ ADMIN
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades.Include(g => g.Student).FirstOrDefaultAsync(g => g.GradeId == id);
            if (grade == null) return NotFound();

            // CHỐT CHẶN 3: Chặn ngay từ cửa nếu bấm nút "Sửa" trên một sinh viên đã thôi học
            if (grade.Student != null && grade.Student.Status != "Đang học")
            {
                TempData["ErrorMessage"] = $"Lỗi: Hệ thống đã đóng băng hồ sơ! Sinh viên {grade.Student.FullName} đang trong trạng thái '{grade.Student.Status}'. Không thể sửa điểm.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
            return View(grade);
        }

        // POST: Grades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // CHỈ ADMIN
        public async Task<IActionResult> Edit(int id, [Bind("GradeId,StudentId,SubjectId,MidtermScore1,MidtermScore2,FinalScore,ExamDate,Semester")] Grade grade)
        {
            if (id != grade.GradeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // CHỐT CHẶN 4: Kiểm tra trạng thái Sinh viên trước khi lưu đè dữ liệu
                    var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == grade.StudentId);
                    if (student != null && student.Status != "Đang học")
                    {
                        TempData["ErrorMessage"] = $"Lỗi bảo mật: Sinh viên đang trong trạng thái '{student.Status}'. Không cho phép cập nhật dữ liệu.";
                        return RedirectToAction(nameof(Index));
                    }

                    CalculateFinalGrade(grade);
                    _context.Update(grade);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.GradeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "SubjectName", grade.SubjectId);
            return View(grade);
        }

        // ==========================================
        // HÀM NGHIỆP VỤ: TÍNH ĐIỂM VÀ QUY ĐỔI HỆ CHỮ
        // ==========================================
        private void CalculateFinalGrade(Grade grade)
        {
            decimal midtermAvg = 0;

            if (grade.MidtermScore1.HasValue && grade.MidtermScore2.HasValue)
            {
                midtermAvg = (grade.MidtermScore1.Value + grade.MidtermScore2.Value) / 2m;
            }
            else if (grade.MidtermScore1.HasValue)
            {
                midtermAvg = grade.MidtermScore1.Value;
            }
            else if (grade.MidtermScore2.HasValue)
            {
                midtermAvg = grade.MidtermScore2.Value;
            }

            if (grade.FinalScore.HasValue)
            {
                grade.TotalScore = Math.Round((midtermAvg * 0.3m) + (grade.FinalScore.Value * 0.7m), 2);

                if (grade.TotalScore >= 8.5m) grade.LetterGrade = "A";
                else if (grade.TotalScore >= 7.0m) grade.LetterGrade = "B";
                else if (grade.TotalScore >= 5.5m) grade.LetterGrade = "C";
                else if (grade.TotalScore >= 4.0m) grade.LetterGrade = "D";
                else grade.LetterGrade = "F";
            }
            else
            {
                grade.TotalScore = null;
                grade.LetterGrade = null;
            }
        }

        // GET: Grades/Delete/5
        [Authorize(Roles = "Admin")] // CHỈ ADMIN
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.GradeId == id);

            if (grade == null) return NotFound();

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // CHỈ ADMIN
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa điểm khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.GradeId == id);
        }

        [Authorize(Roles = "Admin")] // CHỈ ADMIN ĐƯỢC XUẤT EXCEL
        public async Task<IActionResult> ExportToExcel()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();

            byte[] content;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDiem");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Mã SV";
                worksheet.Cell(currentRow, 2).Value = "Tên Sinh viên";
                worksheet.Cell(currentRow, 3).Value = "Môn học";
                worksheet.Cell(currentRow, 4).Value = "Học kỳ";
                worksheet.Cell(currentRow, 5).Value = "Ngày thi";
                worksheet.Cell(currentRow, 6).Value = "Giữa kì 1";
                worksheet.Cell(currentRow, 7).Value = "Giữa kì 2";
                worksheet.Cell(currentRow, 8).Value = "Cuối kì";
                worksheet.Cell(currentRow, 9).Value = "Tổng kết (Hệ 10)";
                worksheet.Cell(currentRow, 10).Value = "Điểm Chữ";

                var headerRange = worksheet.Range("A1:J1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a73e8");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                foreach (var grade in grades)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = grade.Student?.StudentId ?? "N/A";
                    worksheet.Cell(currentRow, 2).Value = grade.Student?.FullName ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = grade.Subject?.SubjectName ?? "N/A";
                    worksheet.Cell(currentRow, 4).Value = grade.Semester ?? "-";
                    worksheet.Cell(currentRow, 5).Value = grade.ExamDate.HasValue ? grade.ExamDate.Value.ToString("dd/MM/yyyy") : "-";
                    worksheet.Cell(currentRow, 6).Value = grade.MidtermScore1;
                    worksheet.Cell(currentRow, 7).Value = grade.MidtermScore2;
                    worksheet.Cell(currentRow, 8).Value = grade.FinalScore;
                    worksheet.Cell(currentRow, 9).Value = grade.TotalScore;
                    worksheet.Cell(currentRow, 10).Value = grade.LetterGrade ?? "-";
                }

                worksheet.Range($"D2:J{currentRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    content = stream.ToArray();
                }
            }

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiem_DaiHoc.xlsx");
        }

        // ==========================================
        // TÍNH NĂNG MỚI: TẢI FILE EXCEL MẪU (TEMPLATE) CHO ĐIỂM SỐ
        // ==========================================
        [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC TẢI TEMPLATE
        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("NhapDiem_Template");

                worksheet.Cell(1, 1).Value = "Mã Sinh viên (*)";
                worksheet.Cell(1, 2).Value = "Mã Môn học (*)";
                worksheet.Cell(1, 3).Value = "Học kỳ (*)";
                worksheet.Cell(1, 4).Value = "Ngày thi (dd/MM/yyyy)";
                worksheet.Cell(1, 5).Value = "Giữa kì 1 (0-10)";
                worksheet.Cell(1, 6).Value = "Giữa kì 2 (0-10)";
                worksheet.Cell(1, 7).Value = "Cuối kì (0-10)";

                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");

                // Dữ liệu mẫu gợi ý
                worksheet.Cell(2, 1).Value = "SV001";
                worksheet.Cell(2, 2).Value = "INT140";
                worksheet.Cell(2, 3).Value = "Spring 2026";
                worksheet.Cell(2, 4).Value = "20/05/2026";
                worksheet.Cell(2, 5).Value = 8.5;
                worksheet.Cell(2, 6).Value = 9.0;
                worksheet.Cell(2, 7).Value = 8.0;

                worksheet.Range("A2:G2").Style.Font.Italic = true;
                worksheet.Range("A2:G2").Style.Font.FontColor = XLColor.Gray;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_NhapDiem.xlsx");
                }
            }
        }

        // ==========================================
        // TÍNH NĂNG MỚI: IMPORT ĐIỂM SỐ TỪ EXCEL
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC IMPORT
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
            List<string> errorDetails = new List<string>();

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
                            string subjectId = row.Cell(2).Value.ToString().Trim();
                            string semester = row.Cell(3).Value.ToString().Trim();

                            // Bỏ qua dòng trống và dòng mẫu
                            if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(subjectId)) continue;
                            if (rowNumber == 2 && studentId == "SV001") continue;

                            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(semester))
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Thiếu Mã SV, Mã Môn hoặc Học kỳ.");
                                continue;
                            }

                            // Chốt chặn 1: Kiểm tra sinh viên có tồn tại và đang học không
                            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == studentId);
                            if (student == null)
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Mã SV '{studentId}' không tồn tại.");
                                continue;
                            }
                            if (student.Status != "Đang học")
                            {
                                errorDetails.Add($"Dòng {rowNumber}: SV '{studentId}' đang '{student.Status}', không thể nhập điểm.");
                                continue;
                            }

                            // Chốt chặn 2: Kiểm tra Môn học có tồn tại không
                            if (!_context.Subjects.Any(s => s.SubjectId == subjectId))
                            {
                                errorDetails.Add($"Dòng {rowNumber}: Mã Môn '{subjectId}' không tồn tại.");
                                continue;
                            }

                            // Chốt chặn 3: Kiểm tra xem SV này đã có điểm môn này trong kỳ này chưa (tránh trùng)
                            if (_context.Grades.Any(g => g.StudentId == studentId && g.SubjectId == subjectId && g.Semester == semester))
                            {
                                errorDetails.Add($"Dòng {rowNumber}: SV '{studentId}' đã có điểm môn '{subjectId}' học kỳ '{semester}'.");
                                continue;
                            }

                            var newGrade = new Grade
                            {
                                StudentId = studentId,
                                SubjectId = subjectId,
                                Semester = semester
                            };

                            // Xử lý Ngày thi
                            var dateCell = row.Cell(4);
                            if (!dateCell.IsEmpty())
                            {
                                if (dateCell.TryGetValue<DateTime>(out DateTime dateValue))
                                {
                                    newGrade.ExamDate = DateOnly.FromDateTime(dateValue);
                                }
                                else if (DateOnly.TryParseExact(dateCell.Value.ToString().Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate))
                                {
                                    newGrade.ExamDate = parsedDate;
                                }
                                else
                                {
                                    errorDetails.Add($"Dòng {rowNumber}: Ngày thi sai định dạng (Chuẩn: dd/MM/yyyy).");
                                    continue;
                                }
                            }

                            // Xử lý đọc Điểm số an toàn
                            if (decimal.TryParse(row.Cell(5).Value.ToString(), out decimal m1)) newGrade.MidtermScore1 = m1;
                            if (decimal.TryParse(row.Cell(6).Value.ToString(), out decimal m2)) newGrade.MidtermScore2 = m2;
                            if (decimal.TryParse(row.Cell(7).Value.ToString(), out decimal final)) newGrade.FinalScore = final;

                            // TỰ ĐỘNG TÍNH ĐIỂM TỔNG KẾT VÀ ĐIỂM CHỮ
                            CalculateFinalGrade(newGrade);

                            _context.Grades.Add(newGrade);
                            successCount++;
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                if (successCount > 0) TempData["SuccessMessage"] = $"Import thành công {successCount} bảng điểm mới!";

                if (errorDetails.Count > 0)
                {
                    TempData["ErrorMessage"] = "Có " + errorDetails.Count + " dòng lỗi:\n" + string.Join("\n", errorDetails.Take(5)) + (errorDetails.Count > 5 ? "\n..." : "");
                }
                else if (successCount == 0)
                {
                    TempData["ErrorMessage"] = "Không có điểm nào được thêm. Vui lòng kiểm tra lại file Excel.";
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
