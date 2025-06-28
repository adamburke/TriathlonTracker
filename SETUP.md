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

### 3. Configuration Management

**Important**: This application stores sensitive configuration (like Google OAuth credentials) securely **encrypted** in the PostgreSQL database, not in configuration files or user secrets.

#### Initial Setup of Google OAuth Credentials

After setting up your Google Cloud Console credentials, you need to securely store them in the database:

**Option 1: Using the Configuration Seeder (Recommended)**
```csharp
// Add this to a temporary console app or run in the application
await ConfigurationSeeder.SeedGoogleCredentials(
    "Host=localhost;Database=TriathlonTracker;Username=TriathlonTrackerUser;Password=TriathlonTrackerUser",
    "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET"
);
```

**Option 2: Direct Database Update**
1. Connect to your PostgreSQL database
2. Insert/Update the encrypted credentials:
   ```sql
   -- The application will automatically encrypt these values
   INSERT INTO "Configurations" ("Key", "Value", "Description", "IsEncrypted", "CreatedAt", "UpdatedAt")
   VALUES
   ('Authentication:Google:ClientId', 'YOUR_ACTUAL_CLIENT_ID', 'Google OAuth Client ID', true, NOW(), NOW()),
   ('Authentication:Google:ClientSecret', 'YOUR_ACTUAL_CLIENT_SECRET', 'Google OAuth Client Secret', true, NOW(), NOW())
   ON CONFLICT ("Key") DO UPDATE SET
       "Value" = EXCLUDED."Value",
       "UpdatedAt" = NOW();
   ```

The application will automatically:
- Create the necessary database tables on first run
- Encrypt sensitive configuration values (Client ID, Client Secret, JWT keys)
- Load and decrypt configuration from the database at startup
- Migrate existing unencrypted values to encrypted format

## Running the Application

1. Restore packages:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Navigate to:
   - HTTP: `http://localhost:5217`
   - HTTPS: `https://localhost:7193`
   - Swagger API: `https://localhost:7193/swagger`

## Security Features

- ✅ Sensitive credentials stored securely in database
- ✅ No secrets in configuration files or source control
- ✅ GitHub push protection compliance
- ✅ Automatic configuration seeding on first run
- ✅ Database-driven configuration management

## Troubleshooting

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