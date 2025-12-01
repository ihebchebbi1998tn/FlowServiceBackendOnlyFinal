-- =====================================================
-- Auth & Users Module Tables
-- =====================================================

-- Main Admin Users Table
CREATE TABLE IF NOT EXISTS "MainAdminUsers" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "PhoneNumber" VARCHAR(20),
    "Country" VARCHAR(2) NOT NULL,
    "Industry" VARCHAR(100) NOT NULL DEFAULT '',
    "AccessToken" VARCHAR(500),
    "RefreshToken" VARCHAR(500),
    "TokenExpiresAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "LastLoginAt" TIMESTAMP,
    "CompanyName" VARCHAR(255),
    "CompanyWebsite" VARCHAR(500),
    "PreferencesJson" TEXT,
    "OnboardingCompleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Users Table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "PhoneNumber" VARCHAR(20),
    "Country" VARCHAR(2) NOT NULL,
    "CreatedUser" VARCHAR(100) NOT NULL,
    "ModifyUser" VARCHAR(100),
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifyDate" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "Role" VARCHAR(50),
    "LastLoginAt" TIMESTAMP,
    "AccessToken" VARCHAR(500),
    "RefreshToken" VARCHAR(500),
    "TokenExpiresAt" TIMESTAMP
);

-- User Preferences Table
CREATE TABLE IF NOT EXISTS "UserPreferences" (
    "Id" VARCHAR(50) PRIMARY KEY DEFAULT gen_random_uuid()::text,
    "UserId" INTEGER NOT NULL,
    "Theme" VARCHAR(20) NOT NULL DEFAULT 'system',
    "Language" VARCHAR(5) NOT NULL DEFAULT 'en',
    "PrimaryColor" VARCHAR(20) NOT NULL DEFAULT 'blue',
    "LayoutMode" VARCHAR(20) NOT NULL DEFAULT 'sidebar',
    "DataView" VARCHAR(10) NOT NULL DEFAULT 'table',
    "Timezone" VARCHAR(100),
    "DateFormat" VARCHAR(20) NOT NULL DEFAULT 'MM/DD/YYYY',
    "TimeFormat" VARCHAR(5) NOT NULL DEFAULT '12h',
    "Currency" VARCHAR(5) NOT NULL DEFAULT 'USD',
    "NumberFormat" VARCHAR(10) NOT NULL DEFAULT 'comma',
    "Notifications" TEXT DEFAULT '{}',
    "SidebarCollapsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CompactMode" BOOLEAN NOT NULL DEFAULT FALSE,
    "ShowTooltips" BOOLEAN NOT NULL DEFAULT TRUE,
    "AnimationsEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "SoundEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "AutoSave" BOOLEAN NOT NULL DEFAULT TRUE,
    "WorkArea" VARCHAR(100),
    "DashboardLayout" TEXT,
    "QuickAccessItems" TEXT DEFAULT '[]',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "MainAdminUsers"("Id") ON DELETE CASCADE
);

-- Indexes for Auth & Users
CREATE INDEX IF NOT EXISTS "idx_mainadminusers_email" ON "MainAdminUsers"("Email");
CREATE INDEX IF NOT EXISTS "idx_users_email" ON "Users"("Email");
CREATE INDEX IF NOT EXISTS "idx_userpreferences_userid" ON "UserPreferences"("UserId");
