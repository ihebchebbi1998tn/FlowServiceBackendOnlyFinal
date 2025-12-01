using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class ComprehensiveTableNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // NORMALIZE ALL TABLE NAMES TO MATCH CONFIGURATIONS
            // =====================================================
            
            // PascalCase tables - normalize if needed
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Contacts Module (PascalCase)
                    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'contacts') 
                    AND NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Contacts') THEN
                        ALTER TABLE contacts RENAME TO ""Contacts"";
                    ELSIF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'contacts') 
                    AND EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Contacts') THEN
                        DROP TABLE contacts CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'roles') 
                    AND NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Roles') THEN
                        ALTER TABLE roles RENAME TO ""Roles"";
                    ELSIF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'roles') 
                    AND EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Roles') THEN
                        DROP TABLE roles CASCADE;
                    END IF;
                    
                    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'currencies') 
                    AND NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Currencies') THEN
                        ALTER TABLE currencies RENAME TO ""Currencies"";
                    ELSIF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'currencies') 
                    AND EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Currencies') THEN
                        DROP TABLE currencies CASCADE;
                    END IF;
                    
                    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'lookupitems') 
                    AND NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'LookupItems') THEN
                        ALTER TABLE lookupitems RENAME TO ""LookupItems"";
                    ELSIF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'lookupitems') 
                    AND EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'LookupItems') THEN
                        DROP TABLE lookupitems CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: normalized table names are canonical
        }
    }
}
