namespace TriathlonTracker.Models.Enums
{
    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical,
        Information,
        Warning,
        Error
    }

    public enum RequestStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Expired,
        Cancelled,
        Scheduled,
        Running
    }

    public enum AccessType
    {
        Allow,
        Block,
        Monitor
    }

    public enum JobType
    {
        Cleanup,
        Archive,
        Notify
    }

    public enum NotificationType
    {
        DataExpiring,
        DataExpired,
        DataArchived,
        ConsentRenewal,
        BreachNotification
    }

    public enum DataRetentionPreference
    {
        Standard,
        Minimal,
        Extended
    }

    public enum ConsentType
    {
        Marketing,
        Analytics,
        Essential,
        ThirdParty
    }

    public enum DeletionType
    {
        SoftDelete,
        HardDelete,
        Anonymize
    }
} 