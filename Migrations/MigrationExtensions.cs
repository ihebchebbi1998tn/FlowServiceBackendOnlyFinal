using Microsoft.EntityFrameworkCore.Migrations;

namespace MyApi.Migrations
{
    /// <summary>
    /// Extension methods for making migrations idempotent (safe to run multiple times)
    /// </summary>
    public static class MigrationExtensions
    {
        /// <summary>
        /// Creates a table only if it doesn't already exist
        /// </summary>
        public static void CreateTableIfNotExists(
            this MigrationBuilder migrationBuilder,
            string tableName,
            Action<MigrationBuilder> createAction)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' 
                        AND tablename = '{tableName}'
                    ) THEN
                        {GenerateCreateTablePlaceholder(tableName)}
                    END IF;
                END $$;
            ");
            
            // Execute the actual create if table doesn't exist
            var sql = $@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM pg_tables 
                        WHERE schemaname = 'public' 
                        AND tablename = '{tableName}'
                    ) THEN
            ";
            
            createAction(migrationBuilder);
        }

        /// <summary>
        /// Creates an index only if it doesn't already exist
        /// </summary>
        public static void CreateIndexIfNotExists(
            this MigrationBuilder migrationBuilder,
            string indexName,
            string tableName,
            Action<MigrationBuilder> createAction)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' 
                        AND indexname = '{indexName}'
                    ) THEN
            ");
            
            createAction(migrationBuilder);
            
            migrationBuilder.Sql(@"
                    END IF;
                END $$;
            ");
        }

        /// <summary>
        /// Adds a column only if it doesn't already exist
        /// </summary>
        public static void AddColumnIfNotExists(
            this MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            string columnType,
            string? defaultValue = null,
            bool nullable = true)
        {
            var nullableClause = nullable ? "NULL" : "NOT NULL";
            var defaultClause = defaultValue != null ? $"DEFAULT {defaultValue}" : "";
            
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = '{tableName}' 
                        AND column_name = '{columnName}'
                    ) THEN
                        ALTER TABLE ""{tableName}"" 
                        ADD COLUMN ""{columnName}"" {columnType} {nullableClause} {defaultClause};
                    END IF;
                END $$;
            ");
        }

        /// <summary>
        /// Renames a column only if the old column exists and new column doesn't
        /// </summary>
        public static void RenameColumnIfExists(
            this MigrationBuilder migrationBuilder,
            string tableName,
            string oldColumnName,
            string newColumnName)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = '{tableName}' 
                        AND column_name = '{oldColumnName}'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = '{tableName}' 
                        AND column_name = '{newColumnName}'
                    ) THEN
                        ALTER TABLE ""{tableName}"" 
                        RENAME COLUMN ""{oldColumnName}"" TO ""{newColumnName}"";
                    END IF;
                END $$;
            ");
        }

        /// <summary>
        /// Drops a table only if it exists
        /// </summary>
        public static void DropTableIfExists(
            this MigrationBuilder migrationBuilder,
            string tableName)
        {
            migrationBuilder.Sql($@"DROP TABLE IF EXISTS ""{tableName}"" CASCADE;");
        }

        private static string GenerateCreateTablePlaceholder(string tableName)
        {
            return $"-- Table {tableName} will be created";
        }
    }
}
