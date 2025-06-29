using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Models;
using TriathlonTracker.Models.Base;

namespace TriathlonTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Triathlon> Triathlons { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<ConsentRecord> ConsentRecords { get; set; }
        public DbSet<DataProcessingLog> DataProcessingLogs { get; set; }
        public DbSet<DataRetentionPolicy> DataRetentionPolicies { get; set; }
        public DbSet<DataExportRequest> DataExportRequests { get; set; }
        public DbSet<DataRectificationRequest> DataRectificationRequests { get; set; }
        public DbSet<AccountDeletionRequest> AccountDeletionRequests { get; set; }
        public DbSet<BreachIncident> BreachIncidents { get; set; }
        
        // Phase 3 GDPR Models
        public DbSet<GdprAuditLog> GdprAuditLogs { get; set; }
        public DbSet<ConsentAnalytics> ConsentAnalytics { get; set; }
        public DbSet<DataProcessingAnalytics> DataProcessingAnalytics { get; set; }
        public DbSet<ComplianceMonitor> ComplianceMonitors { get; set; }
        public DbSet<SuspiciousActivity> SuspiciousActivities { get; set; }
        public DbSet<SessionMonitoring> SessionMonitorings { get; set; }
        public DbSet<RetentionJob> RetentionJobs { get; set; }
        public DbSet<RetentionJobExecution> RetentionJobExecutions { get; set; }
        public DbSet<DataArchive> DataArchives { get; set; }
        public DbSet<RetentionNotification> RetentionNotifications { get; set; }
        public DbSet<RetentionAuditTrail> RetentionAuditTrails { get; set; }
        public DbSet<IpAccessControl> IpAccessControls { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<EncryptionKey> EncryptionKeys { get; set; }
        public DbSet<AccessAttempt> AccessAttempts { get; set; }
        public DbSet<ThreatIntelligence> ThreatIntelligence { get; set; }
        public DbSet<AuditLogEntry> AuditLogEntries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Triathlon entity
            builder.Entity<Triathlon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RaceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired();
                entity.Property(e => e.SwimDistance).IsRequired();
                entity.Property(e => e.BikeDistance).IsRequired();
                entity.Property(e => e.RunDistance).IsRequired();
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ConsentVersion).HasMaxLength(50);
                entity.Property(e => e.DataRetentionPreference)
                      .HasConversion<string>();
                entity.Property(e => e.PreferredDataFormat).HasMaxLength(20);
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // Configure Configuration entity
            builder.Entity<Configuration>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsEncrypted).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // Configure ConsentRecord entity
            builder.Entity<ConsentRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ConsentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsGranted).IsRequired();
                entity.Property(e => e.ConsentDate).IsRequired();
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.LegalBasis).HasMaxLength(100);
                entity.Property(e => e.ConsentVersion).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ConsentRecords)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DataProcessingLog entity
            builder.Entity<DataProcessingLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Purpose).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LegalBasis).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ProcessedBy).HasMaxLength(100);
                entity.Property(e => e.AdditionalData).HasMaxLength(1000);
                entity.Property(e => e.ProcessedAt).IsRequired();
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.DataProcessingLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DataRetentionPolicy entity
            builder.Entity<DataRetentionPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RetentionPeriodDays).IsRequired();
                entity.Property(e => e.LegalBasis).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RetentionReason).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.AutoDelete).IsRequired();
                entity.Property(e => e.DeletionMethod).HasMaxLength(100);
                entity.Property(e => e.NotificationSettings).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            });

            // Configure DataExportRequest entity
            builder.Entity<DataExportRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Format).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RequestDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DownloadUrl).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DataRectificationRequest entity
            builder.Entity<DataRectificationRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CurrentValue).HasMaxLength(500);
                entity.Property(e => e.RequestedValue).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RequestDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReviewedBy).HasMaxLength(100);
                entity.Property(e => e.ReviewNotes).HasMaxLength(1000);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.SupportingDocuments).HasMaxLength(1000);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AccountDeletionRequest entity
            builder.Entity<AccountDeletionRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.RequestDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.DeletionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProcessedBy).HasMaxLength(100);
                entity.Property(e => e.ConfirmationToken).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ProcessingNotes).HasMaxLength(1000);
                entity.Property(e => e.DataBackupInfo).HasMaxLength(1000);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BreachIncident entity
            builder.Entity<BreachIncident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IncidentId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BreachType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DetectedDate).IsRequired();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.AffectedDataTypes).HasMaxLength(500);
                entity.Property(e => e.AffectedUserIds).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DetectedBy).HasMaxLength(100);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.ImpactAssessment).HasMaxLength(2000);
                entity.Property(e => e.ContainmentActions).HasMaxLength(2000);
                entity.Property(e => e.RemediationActions).HasMaxLength(2000);
                entity.Property(e => e.NotificationMethod).HasMaxLength(1000);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                entity.Property(e => e.LessonsLearned).HasMaxLength(2000);
                entity.Property(e => e.PreventiveMeasures).HasMaxLength(2000);
                entity.Property(e => e.SourceIpAddress).HasMaxLength(200);
                entity.Property(e => e.SourceUserAgent).HasMaxLength(500);
                entity.Property(e => e.TechnicalDetails).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                
                // Create unique index on IncidentId
                entity.HasIndex(e => e.IncidentId).IsUnique();
            });

            // Configure Phase 3 GDPR Models
            
            // Configure GdprAuditLog entity
            builder.Entity<GdprAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AdminUserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.RequestId).HasMaxLength(100);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
            });

            // Configure ConsentAnalytics entity
            builder.Entity<ConsentAnalytics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.ConsentType).HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.ConsentType);
                entity.Ignore(e => e.ConsentByPurpose);
                entity.Property(e => e.ConsentByPurposeJson).HasColumnName("ConsentByPurpose");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure DataProcessingAnalytics entity
            builder.Entity<DataProcessingAnalytics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.HasIndex(e => e.Date);
                entity.Ignore(e => e.ProcessingByType);
                entity.Property(e => e.ProcessingByTypeJson).HasColumnName("ProcessingByType");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure ComplianceMonitor entity
            builder.Entity<ComplianceMonitor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MonitorType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.MonitorType);
                entity.Ignore(e => e.Configuration);
                entity.Property(e => e.ConfigurationJson).HasColumnName("Configuration");
                entity.Ignore(e => e.LastResult);
                entity.Property(e => e.LastResultJson).HasColumnName("LastResult");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure SuspiciousActivity entity
            builder.Entity<SuspiciousActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Severity).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.DetectedAt);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsInvestigated);
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure SessionMonitoring entity
            builder.Entity<SessionMonitoring>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Device).HasMaxLength(200);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.Ignore(e => e.AccessedResources);
                entity.Property(e => e.AccessedResourcesJson).HasColumnName("AccessedResources");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure RetentionJob entity
            builder.Entity<RetentionJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.JobType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Schedule).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.IsEnabled);
                entity.HasIndex(e => e.JobType);
                entity.Ignore(e => e.Configuration);
                entity.Property(e => e.ConfigurationJson).HasColumnName("Configuration");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure RetentionJobExecution entity
            builder.Entity<RetentionJobExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.StartTime);
                entity.Ignore(e => e.ExecutionDetails);
                entity.Property(e => e.ExecutionDetailsJson).HasColumnName("ExecutionDetails");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
                
                // Configure relationship with RetentionJob
                entity.HasOne(e => e.Job)
                      .WithMany()
                      .HasForeignKey(e => e.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DataArchive entity
            builder.Entity<DataArchive>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalEntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OriginalEntityId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ArchivedBy).HasMaxLength(100);
                entity.Property(e => e.EncryptionKey).HasMaxLength(500);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ArchivedAt);
                entity.HasIndex(e => e.OriginalEntityType);
                entity.Ignore(e => e.StringMetadata);
                entity.Property(e => e.StringMetadataJson).HasColumnName("StringMetadata");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure RetentionNotification entity
            builder.Entity<RetentionNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DataType).HasMaxLength(100);
                entity.Property(e => e.DeliveryStatus).HasMaxLength(50);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsSent);
                entity.HasIndex(e => e.CreatedAt);
                entity.Ignore(e => e.NotificationData);
                entity.Property(e => e.NotificationDataJson).HasColumnName("NotificationData");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure RetentionAuditTrail entity
            builder.Entity<RetentionAuditTrail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PerformedBy).HasMaxLength(100);
                entity.Property(e => e.JobId).HasMaxLength(100);
                entity.HasIndex(e => e.ActionDate);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.Ignore(e => e.ActionDetails);
                entity.Property(e => e.ActionDetailsJson).HasColumnName("ActionDetails");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
                
                // Configure relationship with RetentionJob
                entity.HasOne(e => e.Job)
                      .WithMany()
                      .HasForeignKey(e => e.JobId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure IpAccessControl entity
            builder.Entity<IpAccessControl>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.IpRange).HasMaxLength(200);
                entity.Property(e => e.AccessType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.HasIndex(e => e.IpAddress);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.AccessType);
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure SecurityEvent entity
            builder.Entity<SecurityEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.IsResolved);
                entity.Ignore(e => e.EventData);
                entity.Property(e => e.EventDataJson).HasColumnName("EventData");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure EncryptionKey entity
            builder.Entity<EncryptionKey>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.KeyName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.KeyType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Purpose).HasMaxLength(200);
                entity.HasIndex(e => e.KeyName).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.KeyType);
                entity.Ignore(e => e.KeyMetadata);
                entity.Property(e => e.KeyMetadataJson).HasColumnName("KeyMetadata");
                entity.Ignore(e => e.StringMetadata);
                entity.Property(e => e.StringMetadataJson).HasColumnName("StringMetadata");
            });

            // Configure AccessAttempt entity
            builder.Entity<AccessAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Resource).HasMaxLength(200);
                entity.Property(e => e.Method).HasMaxLength(20);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.ReferrerUrl).HasMaxLength(500);
                entity.HasIndex(e => e.AttemptTime);
                entity.HasIndex(e => e.IpAddress);
                entity.HasIndex(e => e.IsSuccessful);
                entity.HasIndex(e => e.IsSuspicious);
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
                entity.Ignore(e => e.AdditionalData);
                entity.Property(e => e.AdditionalDataJson).HasColumnName("AdditionalData");
            });

            // Configure ThreatIntelligence entity
            builder.Entity<ThreatIntelligence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ThreatType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Indicator).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IndicatorType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Severity).HasMaxLength(50);
                entity.Property(e => e.Source).HasMaxLength(200);
                entity.HasIndex(e => e.Indicator);
                entity.HasIndex(e => e.IndicatorType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ThreatType);
                entity.Ignore(e => e.ThreatData);
                entity.Property(e => e.ThreatDataJson).HasColumnName("ThreatData");
                entity.Ignore(e => e.Metadata);
                entity.Property(e => e.MetadataJson).HasColumnName("Metadata");
            });

            // Configure AuditLogEntry entity
            builder.Entity<AuditLogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.UserId);
            });

            // Configure AuditLog entity
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LogLevel).HasMaxLength(20);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.EntityId);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(200);
            });
        }
    }
}