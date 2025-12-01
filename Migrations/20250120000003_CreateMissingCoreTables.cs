using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateMissingCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure MainAdminUsers table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""MainAdminUsers"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Email"" VARCHAR(255) NOT NULL UNIQUE,
                    ""PasswordHash"" VARCHAR(255) NOT NULL,
                    ""FirstName"" VARCHAR(100) NOT NULL,
                    ""LastName"" VARCHAR(100) NOT NULL,
                    ""PhoneNumber"" VARCHAR(20),
                    ""Country"" VARCHAR(2) NOT NULL,
                    ""Industry"" VARCHAR(100) NOT NULL DEFAULT '',
                    ""AccessToken"" VARCHAR(500),
                    ""RefreshToken"" VARCHAR(500),
                    ""TokenExpiresAt"" TIMESTAMP,
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" TIMESTAMP,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""LastLoginAt"" TIMESTAMP,
                    ""CompanyName"" VARCHAR(255),
                    ""CompanyWebsite"" VARCHAR(500),
                    ""PreferencesJson"" TEXT,
                    ""OnboardingCompleted"" BOOLEAN NOT NULL DEFAULT FALSE
                );
            ");

            // Ensure UserPreferences table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserPreferences"" (
                    ""Id"" VARCHAR(50) PRIMARY KEY DEFAULT gen_random_uuid()::text,
                    ""UserId"" INTEGER NOT NULL,
                    ""Theme"" VARCHAR(20) NOT NULL DEFAULT 'system',
                    ""Language"" VARCHAR(5) NOT NULL DEFAULT 'en',
                    ""PrimaryColor"" VARCHAR(20) NOT NULL DEFAULT 'blue',
                    ""LayoutMode"" VARCHAR(20) NOT NULL DEFAULT 'sidebar',
                    ""DataView"" VARCHAR(10) NOT NULL DEFAULT 'table',
                    ""Timezone"" VARCHAR(100),
                    ""DateFormat"" VARCHAR(20) NOT NULL DEFAULT 'MM/DD/YYYY',
                    ""TimeFormat"" VARCHAR(5) NOT NULL DEFAULT '12h',
                    ""Currency"" VARCHAR(5) NOT NULL DEFAULT 'USD',
                    ""NumberFormat"" VARCHAR(10) NOT NULL DEFAULT 'comma',
                    ""Notifications"" TEXT DEFAULT '{}',
                    ""SidebarCollapsed"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CompactMode"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""ShowTooltips"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""AnimationsEnabled"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""SoundEnabled"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""AutoSave"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""WorkArea"" VARCHAR(100),
                    ""DashboardLayout"" TEXT,
                    ""QuickAccessItems"" TEXT DEFAULT '[]',
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT ""FK_UserPreferences_MainAdminUsers_UserId""
                        FOREIGN KEY (""UserId"")
                        REFERENCES ""MainAdminUsers""(""Id"")
                        ON DELETE CASCADE
                );
            ");

            // Ensure Skills table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Skills"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Name"" VARCHAR(100) NOT NULL,
                    ""Description"" VARCHAR(500),
                    ""Category"" VARCHAR(100) NOT NULL,
                    ""Level"" VARCHAR(20),
                    ""CreatedUser"" VARCHAR(100) NOT NULL,
                    ""ModifyUser"" VARCHAR(100),
                    ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" TIMESTAMP,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                );
            ");

            // Ensure UserSkills table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserSkills"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""UserId"" INTEGER NOT NULL,
                    ""SkillId"" INTEGER NOT NULL,
                    ""ProficiencyLevel"" VARCHAR(20),
                    ""YearsOfExperience"" INTEGER,
                    ""Certifications"" VARCHAR(500),
                    ""Notes"" VARCHAR(1000),
                    ""AssignedBy"" VARCHAR(100) NOT NULL,
                    ""AssignedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    CONSTRAINT ""FK_UserSkills_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserSkills_Skills_SkillId"" FOREIGN KEY (""SkillId"") REFERENCES ""Skills""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""UQ_UserSkills_UserId_SkillId"" UNIQUE (""UserId"", ""SkillId"")
                );
            ");

            // Ensure RoleSkills table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""RoleSkills"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""RoleId"" INTEGER NOT NULL,
                    ""SkillId"" INTEGER NOT NULL,
                    ""AssignedBy"" VARCHAR(100) NOT NULL,
                    ""AssignedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Notes"" VARCHAR(500),
                    CONSTRAINT ""FK_RoleSkills_Roles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""Roles""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_RoleSkills_Skills_SkillId"" FOREIGN KEY (""SkillId"") REFERENCES ""Skills""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""UQ_RoleSkills_RoleId_SkillId"" UNIQUE (""RoleId"", ""SkillId"")
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop in reverse order to satisfy foreign keys
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""RoleSkills"" CASCADE;
                DROP TABLE IF EXISTS ""UserSkills"" CASCADE;
                DROP TABLE IF EXISTS ""Skills"" CASCADE;
                DROP TABLE IF EXISTS ""UserPreferences"" CASCADE;
                DROP TABLE IF EXISTS ""MainAdminUsers"" CASCADE;
            ");
        }
    }
}
