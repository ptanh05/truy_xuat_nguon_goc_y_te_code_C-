using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaDNA.Models
{
    // Base model for all pages
    public class BasePageModel : PageModel
    {
        public string? UserRole { get; set; }
        public string? UserName { get; set; }
        public bool IsAuthenticated { get; set; }
    }

    // Dashboard Models
    public class DashboardModel : BasePageModel
    {
        public int TotalNFTs { get; set; }
        public int TotalUsers { get; set; }
        public int TotalMilestones { get; set; }
        public int PendingTransfers { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class AdminDashboardModel : BasePageModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalNFTs { get; set; }
        public int PendingApprovals { get; set; }
        public List<User> RecentUsers { get; set; } = new();
        public List<NFT> RecentNFTs { get; set; } = new();
    }

    public class AnalyticsDashboardModel : BasePageModel
    {
        public List<AnalyticsData> ChartData { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> MonthlyTrends { get; set; } = new();
    }

    // User Management Models
    public class UserManagementModel : BasePageModel
    {
        public List<User> Users { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? FilterRole { get; set; }
    }

    // CreateUserRequest is defined in Models/CreateUserRequest.cs

    // UpdateUserRequest is defined in Models/UpdateUserRequest.cs

    // Audit Models
    public class AuditTrailModel : BasePageModel
    {
        public List<AuditLog> AuditLogs { get; set; } = new();
        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ActionType { get; set; }
    }

    // Batch Operations Models
    public class BatchOperationsModel : BasePageModel
    {
        public List<BatchOperation> BatchOperations { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }

    // Comments Models
    public class CommentsSectionModel : BasePageModel
    {
        public List<Comment> Comments { get; set; } = new();
        public int NFTId { get; set; }
        public string? NewComment { get; set; }
    }

    // Dispute Models
    public class DisputeDashboardModel : BasePageModel
    {
        public List<Dispute> Disputes { get; set; } = new();
        public Dictionary<string, int> DisputeStats { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int DisputeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
    }

    // Inventory Models
    public class InventoryIndexModel : BasePageModel
    {
        public List<NFT> NFTs { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? ManufacturerFilter { get; set; }
    }

    // Login Models
    public class LoginModel : BasePageModel
    {
        public string? WalletAddress { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
    }

    // Developer Portal Models
    public class DeveloperPortalModel : BasePageModel
    {
        public List<ApiKey> ApiKeys { get; set; } = new();
        public string? NewApiKeyName { get; set; }
        public List<ApiKeyUsage> RecentUsage { get; set; } = new();
    }

    // Manufacturer Models
    public class ManufacturerIndexModel : BasePageModel
    {
        public List<User> Manufacturers { get; set; } = new();
        public string? SearchTerm { get; set; }
    }

    public class ManufacturerCreateModel : BasePageModel
    {
        public CreateUserRequest CreateRequest { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    // Notification Models
    public class NotificationCenterModel : BasePageModel
    {
        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public string? FilterType { get; set; }
    }

    public class AlertRulesModel : BasePageModel
    {
        public List<Notification> AlertRules { get; set; } = new();
        public string? NewRuleType { get; set; }
        public string? NewRuleCondition { get; set; }
    }

    // Pharmacy Models
    public class PharmacyIndexModel : BasePageModel
    {
        public List<User> Pharmacies { get; set; } = new();
        public List<NFT> AvailableNFTs { get; set; } = new();
        public string? SearchTerm { get; set; }
    }

    // QR Code Models
    public class QRCodeGeneratorModel : BasePageModel
    {
        public List<QRCodeData> QRCodeData { get; set; } = new();
        public int? SelectedNFTId { get; set; }
        public string? QRCodeContent { get; set; }
    }

    public class QRCodeScannerModel : BasePageModel
    {
        public string? ScannedData { get; set; }
        public NFT? ScannedNFT { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // Reports Models
    public class ReportsDashboardModel : BasePageModel
    {
        public List<ReportData> Reports { get; set; } = new();
        public Dictionary<string, int> ReportStats { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Search Models
    public class SearchIndexModel : BasePageModel
    {
        public List<NFT> SearchResults { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? SearchType { get; set; }
        public SearchFilter? Filter { get; set; }
    }

    // Index Model
    public class IndexModel : BasePageModel
    {
        public List<NFT> FeaturedNFTs { get; set; } = new();
        public Dictionary<string, int> Statistics { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
