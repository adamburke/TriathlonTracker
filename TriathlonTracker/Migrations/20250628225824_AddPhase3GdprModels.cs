using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriathlonTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3GdprModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    Resource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSuspicious = table.Column<bool>(type: "boolean", nullable: false),
                    SuspiciousReason = table.Column<string>(type: "text", nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceMonitors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MonitorType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CheckIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    LastResult = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceMonitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsentAnalytics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalConsents = table.Column<int>(type: "integer", nullable: false),
                    NewConsents = table.Column<int>(type: "integer", nullable: false),
                    RevokedConsents = table.Column<int>(type: "integer", nullable: false),
                    UpdatedConsents = table.Column<int>(type: "integer", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConsentRate = table.Column<double>(type: "double precision", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConsentByPurpose = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataArchives",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OriginalEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginalEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArchivedData = table.Column<string>(type: "text", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginalCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchiveReason = table.Column<string>(type: "text", nullable: false),
                    ArchivedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptionKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataArchives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingAnalytics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalProcessingActivities = table.Column<int>(type: "integer", nullable: false),
                    DataAccessRequests = table.Column<int>(type: "integer", nullable: false),
                    DataExportRequests = table.Column<int>(type: "integer", nullable: false),
                    DataDeletionRequests = table.Column<int>(type: "integer", nullable: false),
                    DataRectificationRequests = table.Column<int>(type: "integer", nullable: false),
                    AverageResponseTime = table.Column<double>(type: "double precision", nullable: false),
                    FailedRequests = table.Column<int>(type: "integer", nullable: false),
                    ProcessingByType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncryptionKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    KeyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KeyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncryptedKey = table.Column<string>(type: "text", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KeyMetadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncryptionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GdprAuditLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AdminUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GdprAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IpAccessControls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IpRange = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HitCount = table.Column<int>(type: "integer", nullable: false),
                    LastHit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpAccessControls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionJobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    JobType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Schedule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcessedRecords = table.Column<int>(type: "integer", nullable: false),
                    FailedRecords = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeliveryError = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotificationData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EventData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionMonitorings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivityCount = table.Column<int>(type: "integer", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Device = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccessedResources = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMonitorings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsInvestigated = table.Column<bool>(type: "boolean", nullable: false),
                    InvestigationNotes = table.Column<string>(type: "text", nullable: true),
                    InvestigatedBy = table.Column<string>(type: "text", nullable: true),
                    InvestigatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThreatIntelligence",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ThreatType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Indicator = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IndicatorType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HitCount = table.Column<int>(type: "integer", nullable: false),
                    ThreatData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreatIntelligence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionAuditTrails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActionDetails = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetentionAuditTrails_RetentionJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "RetentionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RetentionJobExecutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    JobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcessedRecords = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulRecords = table.Column<int>(type: "integer", nullable: false),
                    FailedRecords = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutionDetails = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionJobExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetentionJobExecutions_RetentionJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "RetentionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessAttempts_AttemptTime",
                table: "AccessAttempts",
                column: "AttemptTime");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAttempts_IpAddress",
                table: "AccessAttempts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAttempts_IsSuccessful",
                table: "AccessAttempts",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAttempts_IsSuspicious",
                table: "AccessAttempts",
                column: "IsSuspicious");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceMonitors_IsActive",
                table: "ComplianceMonitors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceMonitors_MonitorType",
                table: "ComplianceMonitors",
                column: "MonitorType");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAnalytics_ConsentType",
                table: "ConsentAnalytics",
                column: "ConsentType");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAnalytics_Date",
                table: "ConsentAnalytics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DataArchives_ArchivedAt",
                table: "DataArchives",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataArchives_OriginalEntityType",
                table: "DataArchives",
                column: "OriginalEntityType");

            migrationBuilder.CreateIndex(
                name: "IX_DataArchives_UserId",
                table: "DataArchives",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingAnalytics_Date",
                table: "DataProcessingAnalytics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_IsActive",
                table: "EncryptionKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_KeyName",
                table: "EncryptionKeys",
                column: "KeyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_KeyType",
                table: "EncryptionKeys",
                column: "KeyType");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAuditLogs_Action",
                table: "GdprAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAuditLogs_Timestamp",
                table: "GdprAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_GdprAuditLogs_UserId",
                table: "GdprAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IpAccessControls_AccessType",
                table: "IpAccessControls",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_IpAccessControls_IpAddress",
                table: "IpAccessControls",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_IpAccessControls_IsActive",
                table: "IpAccessControls",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionAuditTrails_Action",
                table: "RetentionAuditTrails",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionAuditTrails_ActionDate",
                table: "RetentionAuditTrails",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionAuditTrails_JobId",
                table: "RetentionAuditTrails",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionAuditTrails_UserId",
                table: "RetentionAuditTrails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionJobExecutions_JobId",
                table: "RetentionJobExecutions",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionJobExecutions_StartTime",
                table: "RetentionJobExecutions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionJobs_IsEnabled",
                table: "RetentionJobs",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionJobs_JobType",
                table: "RetentionJobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionNotifications_CreatedAt",
                table: "RetentionNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionNotifications_IsSent",
                table: "RetentionNotifications",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionNotifications_UserId",
                table: "RetentionNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_EventType",
                table: "SecurityEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_IsResolved",
                table: "SecurityEvents",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Severity",
                table: "SecurityEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Timestamp",
                table: "SecurityEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMonitorings_IsActive",
                table: "SessionMonitorings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMonitorings_SessionId",
                table: "SessionMonitorings",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionMonitorings_UserId",
                table: "SessionMonitorings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivities_DetectedAt",
                table: "SuspiciousActivities",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivities_IsInvestigated",
                table: "SuspiciousActivities",
                column: "IsInvestigated");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivities_UserId",
                table: "SuspiciousActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIntelligence_Indicator",
                table: "ThreatIntelligence",
                column: "Indicator");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIntelligence_IndicatorType",
                table: "ThreatIntelligence",
                column: "IndicatorType");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIntelligence_IsActive",
                table: "ThreatIntelligence",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIntelligence_ThreatType",
                table: "ThreatIntelligence",
                column: "ThreatType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessAttempts");

            migrationBuilder.DropTable(
                name: "ComplianceMonitors");

            migrationBuilder.DropTable(
                name: "ConsentAnalytics");

            migrationBuilder.DropTable(
                name: "DataArchives");

            migrationBuilder.DropTable(
                name: "DataProcessingAnalytics");

            migrationBuilder.DropTable(
                name: "EncryptionKeys");

            migrationBuilder.DropTable(
                name: "GdprAuditLogs");

            migrationBuilder.DropTable(
                name: "IpAccessControls");

            migrationBuilder.DropTable(
                name: "RetentionAuditTrails");

            migrationBuilder.DropTable(
                name: "RetentionJobExecutions");

            migrationBuilder.DropTable(
                name: "RetentionNotifications");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropTable(
                name: "SessionMonitorings");

            migrationBuilder.DropTable(
                name: "SuspiciousActivities");

            migrationBuilder.DropTable(
                name: "ThreatIntelligence");

            migrationBuilder.DropTable(
                name: "RetentionJobs");
        }
    }
}
