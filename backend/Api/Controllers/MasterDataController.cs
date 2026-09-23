using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Infrastructure.Data;
using Cdsqg.Core.Entities;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/masterdata")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MasterDataController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// POST /api/masterdata/clear-demo-data
        /// Xóa toàn bộ dữ liệu nghiệp vụ (Mục tiêu, Nhiệm vụ, Báo cáo tiến độ, Đôn đốc, Văn bản QPPL, Lịch sử nạp file, Thông báo).
        /// Giữ nguyên Cài đặt chung: Danh mục Cơ quan, Đơn vị tính, Tài khoản Người dùng.
        /// </summary>
        [HttpPost("clear-demo-data")]
        [HttpDelete("clear-demo-data")]
        public async Task<IActionResult> ClearDemoData()
        {
            try
            {
                _context.ProgressLogs.RemoveRange(_context.ProgressLogs);
                _context.TaskUrgeLogs.RemoveRange(_context.TaskUrgeLogs);
                _context.TargetBaselines.RemoveRange(_context.TargetBaselines);
                _context.AgencyTaskExecutions.RemoveRange(_context.AgencyTaskExecutions);
                _context.DataImportLogs.RemoveRange(_context.DataImportLogs);
                _context.Notifications.RemoveRange(_context.Notifications);
                _context.LegalDocuments.RemoveRange(_context.LegalDocuments);
                _context.GoalTaskItems.RemoveRange(_context.GoalTaskItems);

                var doc1266Id = Guid.Parse("12660000-0000-0000-0000-000000001266");
                var otherDocs = await _context.Documents.Where(d => d.Id != doc1266Id && d.DocumentNumber != "1266/QĐ-TTg").ToListAsync();
                _context.Documents.RemoveRange(otherDocs);

                await _context.SaveChangesAsync();

                var doc1266 = await _context.Documents.FirstOrDefaultAsync(d => d.Id == doc1266Id || d.DocumentNumber == "1266/QĐ-TTg");
                if (doc1266 == null)
                {
                    doc1266 = new Document
                    {
                        Id = doc1266Id,
                        DocumentNumber = "1266/QĐ-TTg",
                        Name = "Quyết định số 1266/QĐ-TTg ngày 14/07/2026 của Thủ tướng Chính phủ",
                        Summary = "Hệ thống theo dõi nhiệm vụ được giao tại Quyết định số 1266/QĐ-TTg",
                        Signer = "Thủ tướng Chính phủ",
                        IssueDate = new DateTime(2026, 7, 14),
                        StartYear = 2026,
                        EndYear = 2030,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Documents.Add(doc1266);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Đã xóa toàn bộ dữ liệu demo. Hệ thống đã sẵn sàng để nhập dữ liệu thật." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi xóa dữ liệu demo: " + ex.Message });
            }
        }
    }
}
