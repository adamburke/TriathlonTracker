# Auditing and Logging System

This document describes the comprehensive auditing and logging system implemented in the TriathlonTracker application to provide robust visibility into all user actions, system events, and data changes.

## Overview

The application implements a multi-layered logging and auditing approach:

1. **Structured Logging** - Using Microsoft.Extensions.Logging with multiple log levels
2. **Audit Trail** - Database-stored audit records for compliance and security
3. **Security Events** - Specialized security event tracking
4. **Performance Monitoring** - Request timing and performance metrics

## Logging Levels

The application uses the following log levels appropriately:

- **Trace** - Detailed diagnostic information (not used in production)
- **Debug** - Detailed information for debugging
- **Information** - General application flow and user actions
- **Warning** - Unexpected situations that don't stop execution
- **Error** - Error conditions that need attention
- **Critical** - Critical errors that may cause application failure

## Audit Logging

### AuditLog Model

All user actions are recorded in the `AuditLog` table with the following information:

- **Action** - The specific action performed (e.g., "CreateTriathlon", "UpdateConsent")
- **EntityType** - The type of entity affected (e.g., "Triathlon", "User", "Consent")
- **EntityId** - The ID of the specific entity affected
- **Details** - Human-readable description of the action
- **UserId** - The user who performed the action
- **IpAddress** - IP address of the user
- **UserAgent** - Browser/client information
- **Severity** - Log level (Information, Warning, Error)
- **Timestamp** - When the action occurred

### Audit Service

The `IAuditService` interface provides a centralized way to log audit events:

```csharp
await _auditService.LogAsync(
    action: "CreateTriathlon",
    entityType: "Triathlon", 
    entityId: triathlon.Id.ToString(),
    details: $"Created triathlon: {triathlon.RaceName} at {triathlon.Location}",
    userId: userId,
    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
    userAgent: Request.Headers["User-Agent"],
    severity: "Information"
);
```

## Security Event Logging

### SecurityEnhancements Model

Security-specific events are tracked separately in the `SecurityEnhancements` table:

- **UserId** - The user associated with the security event
- **EventType** - Type of security event (e.g., "SuccessfulLogin", "FailedLogin", "DataAccess")
- **Details** - Detailed description of the security event
- **Severity** - Security level (Information, Warning, Error)
- **Timestamp** - When the event occurred
- **IpAddress** - IP address of the user
- **UserAgent** - Browser/client information

### Security Service

The `ISecurityService` provides specialized security logging:

```csharp
await _securityService.LogLoginAttemptAsync(userId, success, ipAddress, userAgent, failureReason);
await _securityService.LogLogoutAsync(userId, ipAddress, userAgent);
await _securityService.LogDataAccessAsync(userId, "Triathlon", "View", ipAddress, userAgent);
await _securityService.CheckForSuspiciousActivityAsync(userId, ipAddress, userAgent);
```

## Comprehensive Logging Coverage

### Controllers with Full Logging

1. **AccountController** - Login, logout, registration, password changes
2. **TriathlonController** - CRUD operations on triathlon records
3. **AdminController** - Admin dashboard, user management, compliance
4. **ConsentController** - Consent management and GDPR operations
5. **EnhancedPrivacyController** - Data export, rectification, account deletion
6. **ReportingController** - Report generation and management
7. **PrivacyController** - Privacy policy and cookie management
8. **HomeController** - Page access and navigation
9. **GdprApiController** - GDPR API operations

### Services with Logging

1. **AuditService** - Central audit logging
2. **SecurityService** - Security event tracking
3. **AdminDashboardService** - Admin operations
4. **ConsentService** - Consent management
5. **EnhancedGdprService** - GDPR operations
6. **GdprService** - Basic GDPR functionality

## Log Categories

### User Actions
- **Authentication** - Login, logout, registration, password changes
- **Data Operations** - Create, read, update, delete operations
- **Privacy Actions** - Consent changes, data export requests, account deletion
- **Admin Actions** - User management, system configuration, compliance checks

### System Events
- **Security Events** - Failed logins, suspicious activity, data access
- **Error Conditions** - Exceptions, validation failures, access denied
- **Performance** - Slow queries, resource usage, response times

### API Operations
- **API Calls** - All GDPR API endpoints with request/response logging
- **Data Export** - Export request creation, processing, and download
- **Data Rectification** - Rectification request creation and status checks

## Log Storage

### File Logging
Logs are written to files in the `logs` directory:
- `app-{date}.log` - General application logs
- `security-{date}.log` - Security-specific events
- `audit-{date}.log` - Audit trail events

### Database Logging
- **AuditLog** table - All user actions and system events
- **SecurityEnhancements** table - Security-specific events

## Log Analysis and Monitoring

### Key Metrics Tracked
- User activity patterns
- Failed authentication attempts
- Data access patterns
- Admin operations
- API usage statistics
- Error rates and types

### Security Monitoring
- Multiple failed login attempts
- Access from new IP addresses
- Unusual data access patterns
- Admin privilege usage
- GDPR compliance activities

## Configuration

### Logging Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "TriathlonTracker": "Debug"
    },
    "File": {
      "Path": "logs/app-{Date}.log",
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

### Audit Configuration

Audit logging is configured in `Program.cs`:

```csharp
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
```

## Accessing Logs

### File Logs
```bash
# View today's application logs
tail -f logs/app-$(date +%Y-%m-%d).log

# View security logs
tail -f logs/security-$(date +%Y-%m-%d).log

# Search for specific events
grep "CreateTriathlon" logs/app-*.log
```

### Database Logs
```sql
-- View recent audit logs
SELECT * FROM AuditLogs 
WHERE Timestamp >= DATEADD(day, -1, GETUTCDATE())
ORDER BY Timestamp DESC;

-- View security events
SELECT * FROM SecurityEnhancements 
WHERE Timestamp >= DATEADD(day, -1, GETUTCDATE())
ORDER BY Timestamp DESC;

-- Find failed login attempts
SELECT * FROM SecurityEnhancements 
WHERE EventType = 'FailedLogin' 
AND Timestamp >= DATEADD(hour, -1, GETUTCDATE());
```

### Admin Dashboard
- **Audit Log** section in admin dashboard
- **Security Events** monitoring
- **Compliance Reports** with audit data
- **User Activity** tracking

## Best Practices

### For Developers
1. Always log user actions with appropriate context
2. Use structured logging with named parameters
3. Include relevant entity IDs for traceability
4. Log both successful and failed operations
5. Use appropriate log levels

### For Administrators
1. Monitor security logs regularly
2. Set up alerts for suspicious activity
3. Review audit logs for compliance
4. Archive old logs appropriately
5. Monitor log file sizes and rotation

### For Compliance
1. Audit logs are retained for required periods
2. All GDPR operations are fully logged
3. User consent changes are tracked
4. Data access is monitored
5. Admin actions are recorded

## Troubleshooting

### Common Issues
1. **Missing logs** - Check file permissions and disk space
2. **Performance impact** - Monitor log file sizes and database performance
3. **Missing audit records** - Verify audit service is properly injected
4. **Security event gaps** - Check security service configuration

### Debugging
1. Enable Debug logging for detailed information
2. Check database connection for audit logs
3. Verify file paths for log files
4. Monitor application performance with logging enabled

## Future Enhancements

1. **Real-time monitoring** - WebSocket-based live log streaming
2. **Advanced analytics** - Machine learning for anomaly detection
3. **Integration** - SIEM system integration
4. **Compliance reporting** - Automated GDPR compliance reports
5. **Performance optimization** - Async logging and batching 