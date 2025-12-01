using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeUsersSkillsTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize Users vs users
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- If only lower-case 'users' exists, rename it to ""Users""
                    IF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'users'
                    ) AND NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'Users'
                    ) THEN
                        ALTER TABLE users RENAME TO ""Users"";
                    ELSIF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'users'
                    ) AND EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'Users'
                    ) THEN
                        -- Both exist: drop the lower-case duplicate to keep schema clean
                        DROP TABLE users CASCADE;
                    END IF;
                END $$;
            ");

            // Normalize Skills vs skills
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'skills'
                    ) AND NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'Skills'
                    ) THEN
                        ALTER TABLE skills RENAME TO ""Skills"";
                    ELSIF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'skills'
                    ) AND EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'Skills'
                    ) THEN
                        DROP TABLE skills CASCADE;
                    END IF;
                END $$;
            ");

            // Normalize UserSkills vs userskills
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'userskills'
                    ) AND NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'UserSkills'
                    ) THEN
                        ALTER TABLE userskills RENAME TO ""UserSkills"";
                    ELSIF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'userskills'
                    ) AND EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'UserSkills'
                    ) THEN
                        DROP TABLE userskills CASCADE;
                    END IF;
                END $$;
            ");

            // Normalize RoleSkills vs roleskills (just in case)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'roleskills'
                    ) AND NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'RoleSkills'
                    ) THEN
                        ALTER TABLE roleskills RENAME TO ""RoleSkills"";
                    ELSIF EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'roleskills'
                    ) AND EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' AND tablename = 'RoleSkills'
                    ) THEN
                        DROP TABLE roleskills CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: we consider normalized PascalCase table names the canonical form
        }
    }
}
