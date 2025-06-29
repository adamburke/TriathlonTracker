# TriathlonTracker Setup Guide

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL database
- Google OAuth 2.0 credentials

## Environment Configuration

### 1. Database Setup

Create a PostgreSQL database and user:

```sql
CREATE DATABASE TriathlonTracker;
CREATE USER TriathlonTrackerUser WITH PASSWORD 'TriathlonTrackerUser';
GRANT ALL PRIVILEGES ON DATABASE TriathlonTracker TO TriathlonTrackerUser;
```

### 2. Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to "APIs & Services" > "Credentials"
5. Create OAuth 2.0 Client ID credentials
6. Add these Authorized redirect URIs:
   - `http://localhost:5217/signin-google`
   - `https://localhost:7193/signin-google`

### 3. Environment Variables Setup

**Important**: This application uses environment variables to securely seed sensitive configuration into the database and for encryption.

#### Required Environment Variables

Set these environment variables before running the application:

**For Development (PowerShell):**
```powershell
$env:DATABASE_CONNECTION_STRING = "Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser" # (optional, overrides appsettings.json)
$env:GOOGLE_CLIENT_ID = "your-google-client-id"
$env:GOOGLE_CLIENT_SECRET = "your-google-client-secret"
$env:JWT_KEY = "your-secure-jwt-key"
$env:ENCRYPTION_KEY = "your-32-char-encryption-key"   # (32 chars for AES-256)
$env:ENCRYPTION_IV = "your-16-char-encryption-iv"     # (16 chars for AES IV)
```

**For Development (Command Prompt):**
```cmd
set DATABASE_CONNECTION_STRING=Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser
set GOOGLE_CLIENT_ID=your-google-client-id
set GOOGLE_CLIENT_SECRET=your-google-client-secret
set JWT_KEY=your-secure-jwt-key
set ENCRYPTION_KEY=your-32-char-encryption-key
set ENCRYPTION_IV=your-16-char-encryption-iv
```

**For Production:**
Set these as environment variables in your hosting platform (Azure, AWS, etc.)

#### Optional Environment Variables

- `JWT_ISSUER` - JWT issuer (defaults to localhost for development)
- `JWT_AUDIENCE` - JWT audience (defaults to localhost for development)
- `BASE_URL` - Application base URL (overrides config)

#### Generate Secure JWT Key

To generate a secure JWT key:

**PowerShell:**
```powershell
$jwtKey = [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
$env:JWT_KEY = $jwtKey
```

**Command Line:**
```bash
# On Linux/macOS
openssl rand -base64 64
```

### 4. Environment Variables Reference

This application uses environment variables for all sensitive and environment-specific configuration.  
**Set these variables in your shell, CI/CD pipeline, or hosting environment before running the app.**

#### **Required Environment Variables**

| Variable Name                | Description                                              | Example Value / Notes                                 |
|------------------------------|---------------------------------------------------------|-------------------------------------------------------|
| `DATABASE_CONNECTION_STRING` | PostgreSQL connection string (overrides appsettings)    | `Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser` |
| `GOOGLE_CLIENT_ID`           | Google OAuth 2.0 Client ID                             | Obtain from Google Cloud Console                      |
| `GOOGLE_CLIENT_SECRET`       | Google OAuth 2.0 Client Secret                         | Obtain from Google Cloud Console                      |
| `JWT_KEY`                    | JWT signing key (base64 or long random string)         | Generate securely (see below)                         |
| `ENCRYPTION_KEY`             | AES encryption key (32 chars for AES-256)              | Generate securely, keep secret                        |
| `ENCRYPTION_IV`              | AES IV (16 chars)                                      | Generate securely, keep secret                        |

#### **Optional Environment Variables**

| Variable Name        | Description                                  | Example Value                  |
|----------------------|----------------------------------------------|-------------------------------|
| `JWT_ISSUER`         | JWT token issuer                             | `https://yourdomain.com`      |
| `JWT_AUDIENCE`       | JWT token audience                           | `https://yourdomain.com`      |
| `BASE_URL`           | Application base URL (for links, GDPR, etc.) | `https://yourdomain.com`      |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment (dev/prod)      | `Development` or `Production` |

#### **How to Set Environment Variables**

**PowerShell (Windows):**
```powershell
$env:DATABASE_CONNECTION_STRING = "Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser"
$env:GOOGLE_CLIENT_ID = "your-google-client-id"
$env:GOOGLE_CLIENT_SECRET = "your-google-client-secret"
$env:JWT_KEY = "your-secure-jwt-key"
$env:ENCRYPTION_KEY = "your-32-char-encryption-key"
$env:ENCRYPTION_IV = "your-16-char-encryption-iv"
```

**Command Prompt (Windows):**
```cmd
set DATABASE_CONNECTION_STRING=Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser
set GOOGLE_CLIENT_ID=your-google-client-id
set GOOGLE_CLIENT_SECRET=your-google-client-secret
set JWT_KEY=your-secure-jwt-key
set ENCRYPTION_KEY=your-32-char-encryption-key
set ENCRYPTION_IV=your-16-char-encryption-iv
```

**Linux/macOS:**
```bash
export DATABASE_CONNECTION_STRING="Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser"
export GOOGLE_CLIENT_ID="your-google-client-id"
export GOOGLE_CLIENT_SECRET="your-google-client-secret"
export JWT_KEY="your-secure-jwt-key"
export ENCRYPTION_KEY="your-32-char-encryption-key"
export ENCRYPTION_IV="your-16-char-encryption-iv"
```

#### **How to Generate Secure Keys**

**JWT Key (PowerShell):**
```powershell
$jwtKey = [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
$env:JWT_KEY = $jwtKey
```

**Encryption Key (32 chars, PowerShell):**
```powershell
$env:ENCRYPTION_KEY = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | % {[char]$_})
```

**Encryption IV (16 chars, PowerShell):**
```powershell
$env:ENCRYPTION_IV = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 16 | % {[char]$_})
```

**Linux/macOS (OpenSSL):**
```bash
openssl rand -base64 64   # For JWT_KEY
openssl rand -base64 32   # For ENCRYPTION_KEY (trim/pad to 32 chars)
openssl rand -base64 16   # For ENCRYPTION_IV (trim/pad to 16 chars)
```

#### **User Secrets for Local Development (Optional)**

For local development, you can use [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid setting environment variables globally:

```bash
dotnet user-secrets set "GOOGLE_CLIENT_ID" "your-google-client-id"
dotnet user-secrets set "GOOGLE_CLIENT_SECRET" "your-google-client-secret"
dotnet user-secrets set "JWT_KEY" "your-secure-jwt-key"
dotnet user-secrets set "ENCRYPTION_KEY" "your-32-char-encryption-key"
dotnet user-secrets set "ENCRYPTION_IV" "your-16-char-encryption-iv"
```
> User Secrets are only for development and are never committed to source control.

#### **Order of Precedence**

1. **Environment Variables** (highest)
2. **User Secrets** (development only)
3. **appsettings.json** (lowest, for defaults only)

### 5. Configuration Management

**Important**: This application stores sensitive configuration (like Google OAuth credentials) securely **encrypted** in the PostgreSQL database, not in configuration files or user secrets.

The application automatically:
- Reads environment variables on startup
- Seeds the database with encrypted configuration values
- Creates the necessary database tables on first run
- Encrypts sensitive configuration values (Client ID, Client Secret, JWT keys, encryption keys)
- Loads and decrypts configuration from the database at runtime

#### Manual Configuration Seeding (Alternative)

If you prefer to seed configuration manually:

**Option 1: Using the Configuration Seeder**
```csharp
await ConfigurationSeeder.SeedGoogleCredentials(
    "Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser",
    "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET"
);
```

**Option 2: Direct Database Update**
```sql
INSERT INTO "Configurations" ("Key", "Value", "Description", "IsEncrypted", "CreatedAt", "UpdatedAt")
VALUES
('Authentication:Google:ClientId', 'YOUR_ACTUAL_CLIENT_ID', 'Google OAuth Client ID', true, NOW(), NOW()),
('Authentication:Google:ClientSecret', 'YOUR_ACTUAL_CLIENT_SECRET', 'Google OAuth Client Secret', true, NOW(), NOW())
ON CONFLICT ("Key") DO UPDATE SET
    "Value" = EXCLUDED."Value",
    "UpdatedAt" = NOW();
```

## Running the Application

1. **Set environment variables** (see section 3 above)

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Navigate to:
   - HTTP: `http://localhost:5217`
   - HTTPS: `https://localhost:7193`
   - Swagger API: `https://localhost:7193/swagger`

## Production Deployment

### Pre-Deployment Validation

Before deploying to production, run the validation script to ensure all configuration is correct:

```powershell
# Validate production configuration
.\validate-deployment.ps1 -Environment "Production"

# Validate staging configuration
.\validate-deployment.ps1 -Environment "Staging"
```

### Production Deployment Checklist

#### **Environment Variables**
- [ ] `DATABASE_CONNECTION_STRING` - Production database (not localhost)
- [ ] `GOOGLE_CLIENT_ID` - Production Google OAuth credentials
- [ ] `GOOGLE_CLIENT_SECRET` - Production Google OAuth credentials
- [ ] `JWT_KEY` - Secure production key (at least 32 chars)
- [ ] `ENCRYPTION_KEY` - Secure production key (32 chars)
- [ ] `ENCRYPTION_IV` - Secure production IV (16 chars)
- [ ] `JWT_ISSUER` - Production domain (e.g., `https://yourdomain.com`)
- [ ] `JWT_AUDIENCE` - Production domain (e.g., `https://yourdomain.com`)
- [ ] `BASE_URL` - Production domain (e.g., `https://yourdomain.com`)
- [ ] `ASPNETCORE_ENVIRONMENT` - Set to `"Production"`

#### **Google Cloud Console Configuration**
- [ ] Add production redirect URIs:
  - `https://yourdomain.com/signin-google`
- [ ] Verify OAuth consent screen settings
- [ ] Check API quotas and limits
- [ ] Ensure OAuth consent screen is published (if needed)

#### **Database Setup**
- [ ] Production database created and accessible
- [ ] Database user has proper permissions
- [ ] Connection string tested from deployment environment
- [ ] Backup strategy configured
- [ ] Database migrations run successfully

#### **Infrastructure Requirements**
- [ ] SSL certificates configured and valid
- [ ] HTTPS redirect enabled
- [ ] Rate limiting configured for production
- [ ] Monitoring and logging set up
- [ ] Health checks configured
- [ ] Load balancer configured (if applicable)

#### **Security Configuration**
- [ ] Firewall rules configured for database access
- [ ] Network security groups configured
- [ ] Secrets management in place
- [ ] Regular security updates scheduled

### Common Production Issues

#### **1. Google OAuth Redirect URI Mismatch**
**Symptoms**: `redirect_uri_mismatch` error during login
**Solution**: 
- Add production domain to Google Cloud Console redirect URIs
- Ensure exact match: `https://yourdomain.com/signin-google`

#### **2. Database Connection Issues**
**Symptoms**: Application fails to start, database seeding errors
**Solution**:
- Verify database is accessible from deployment environment
- Check firewall rules and network connectivity
- Test connection string manually

#### **3. JWT Token Issues**
**Symptoms**: Authentication failures, token validation errors
**Solution**:
- Ensure `JWT_ISSUER` and `JWT_AUDIENCE` match production domain
- Verify `JWT_KEY` is secure and consistent across deployments

#### **4. Encryption Key Problems**
**Symptoms**: Cannot decrypt existing data
**Solution**:
- Use consistent encryption keys across environments
- Or implement data migration strategy for key changes

#### **5. SSL/HTTPS Issues**
**Symptoms**: Google OAuth fails, mixed content warnings
**Solution**:
- Configure valid SSL certificates
- Ensure all URLs use HTTPS in production

### Deployment Commands

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run with production environment
ASPNETCORE_ENVIRONMENT=Production dotnet run

# Or set all environment variables and run
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run
```

### Post-Deployment Verification

1. **Health Check**: Verify application responds at root URL
2. **Database**: Confirm database seeding completed successfully
3. **Authentication**: Test Google OAuth login flow
4. **API**: Verify Swagger endpoints are accessible
5. **Monitoring**: Check logs for any errors or warnings

## Security Features

- ✅ Sensitive credentials stored securely in database
- ✅ No secrets in configuration files or source control
- ✅ GitHub push protection compliance
- ✅ Automatic configuration seeding from environment variables
- ✅ Database-driven configuration management
- ✅ Secure JWT key generation
- ✅ Secure encryption key/IV via environment variables
- ✅ Production deployment validation script
- ✅ Comprehensive security checklist

## Troubleshooting

### Environment Variables Not Set

If you see warnings about missing environment variables:

1. Set the required environment variables (see section 3)
2. Restart your terminal/command prompt
3. Run the application again

### Google OAuth redirect_uri_mismatch

If you get a redirect URI mismatch error:

1. Check that your Google Cloud Console has the correct redirect URIs registered:
   - `http://localhost:5217/signin-google`
   - `https://localhost:7193/signin-google`
2. Ensure you're using the correct port (5217 for HTTP, 7193 for HTTPS)
3. Clear browser cache and cookies
4. Verify the ClientId in the database matches your Google Cloud Console

### Database Connection Issues

1. Verify PostgreSQL is running
2. Check the connection string in `appsettings.json`
3. Ensure the database and user exist with proper permissions

### Configuration Issues

To view current configuration in the database:
```sql
SELECT * FROM "Configurations" WHERE "Key" LIKE 'Authentication:Google:%';
```

To manually add/update configuration:
```sql
INSERT INTO "Configurations" ("Key", "Value", "Description", "CreatedAt", "UpdatedAt")
VALUES ('YourConfigKey', 'YourConfigValue', 'Description', NOW(), NOW())
ON CONFLICT ("Key") DO UPDATE SET
    "Value" = EXCLUDED."Value",
    "UpdatedAt" = NOW();
```