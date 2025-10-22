using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _reportService.GetAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string generatedBy)
        {
            var report = await _reportService.GenerateReportAsync(reportType, startDate, endDate, generatedBy);
            return Ok(report);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _reportService.GetReportAsync(id);
            return Ok(report);
        }

        [HttpGet("products/analytics")]
        public async Task<IActionResult> GetProductAnalytics()
        {
            var analytics = await _reportService.GetProductAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("{id}/export/pdf")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var pdfContent = await _reportService.ExportReportToPdfAsync(id);
            return File(pdfContent, "application/pdf", $"report-{id}.pdf");
        }

        [HttpGet("{id}/export/excel")]
        public async Task<IActionResult> ExportExcel(int id)
        {
            var excelContent = await _reportService.ExportReportToExcelAsync(id);
            return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report-{id}.xlsx");
        }
    }
}
