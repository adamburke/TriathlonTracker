using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class AdminDashboardViewModel
    {
        public GdprComplianceMetrics ComplianceMetrics { get; set; } = new();
        public List<UserGdprStatus> UserStatuses { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<ComplianceAlert> Alerts { get; set; } = new();
        public DataRetentionSummary RetentionSummary { get; set; } = new();
    }
} 