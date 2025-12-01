using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class RenamePreferencesToPreferencesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the Preferences column to PreferencesJson to avoid conflict with navigation property
            migrationBuilder.RenameColumn(
                name: "Preferences",
                table: "MainAdminUsers",
                newName: "PreferencesJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the column name back to Preferences
            migrationBuilder.RenameColumn(
                name: "PreferencesJson",
                table: "MainAdminUsers",
                newName: "Preferences");
        }
    }
}
