using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var metrics = await _analyticsService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }

        [HttpGet("nft-trend")]
        public async Task<IActionResult> GetNFTTrend(int days = 30)
        {
            var trend = await _analyticsService.GetNFTCreationTrendAsync(days);
            return Ok(trend);
        }

        [HttpGet("transfer-trend")]
        public async Task<IActionResult> GetTransferTrend(int days = 30)
        {
            var trend = await _analyticsService.GetTransferTrendAsync(days);
            return Ok(trend);
        }

        [HttpGet("revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend(int days = 30)
        {
            var trend = await _analyticsService.GetRevenueTrendAsync(days);
            return Ok(trend);
        }

        [HttpGet("nft-by-manufacturer")]
        public async Task<IActionResult> GetNFTByManufacturer()
        {
            var analysis = await _analyticsService.GetNFTByManufacturerAsync();
            return Ok(analysis);
        }

        [HttpGet("transfer-by-status")]
        public async Task<IActionResult> GetTransferByStatus()
        {
            var analysis = await _analyticsService.GetTransferByStatusAsync();
            return Ok(analysis);
        }

        [HttpGet("inventory-by-location")]
        public async Task<IActionResult> GetInventoryByLocation()
        {
            var analysis = await _analyticsService.GetInventoryByLocationAsync();
            return Ok(analysis);
        }

        [HttpGet("anomalies")]
        public async Task<IActionResult> GetAnomalies()
        {
            var alerts = await _analyticsService.GetAnomalyAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("predictions")]
        public async Task<IActionResult> GetPredictions(int days = 30)
        {
            var predictions = await _analyticsService.GetPredictiveAnalysisAsync(days);
            return Ok(predictions);
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics()
        {
            var metrics = await _analyticsService.GetPerformanceMetricsAsync();
            return Ok(metrics);
        }

        [HttpGet("user-behavior")]
        public async Task<IActionResult> GetUserBehavior()
        {
            var analytics = await _analyticsService.GetUserBehaviorAnalyticsAsync();
            return Ok(analytics);
        }
    }
}
