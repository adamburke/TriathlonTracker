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

**Important**: This application stores sensitive configuration (like Google OAuth credentials) securely in the PostgreSQL database, not in configuration files or user secrets.

The application will automatically:
- Create the necessary database tables on first run
- Seed the Google OAuth credentials into the database
- Load configuration from the database at startup

To update Google OAuth credentials after initial setup:
1. Connect to your PostgreSQL database
2. Update the `Configurations` table:
   ```sql
   UPDATE "Configurations"
   SET "Value" = 'YOUR_NEW_CLIENT_ID', "UpdatedAt" = NOW()
   WHERE "Key" = 'Authentication:Google:ClientId';
   
   UPDATE "Configurations"
   SET "Value" = 'YOUR_NEW_CLIENT_SECRET', "UpdatedAt" = NOW()
   WHERE "Key" = 'Authentication:Google:ClientSecret';
   ```

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