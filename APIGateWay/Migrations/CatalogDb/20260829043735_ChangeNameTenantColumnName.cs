using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Migrations.CatalogDb
{
    /// <inheritdoc />
    public partial class ChangeNameTenantColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "catalog",
                table: "Tenants",
                newName: "SchemaName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SchemaName",
                schema: "catalog",
                table: "Tenants",
                newName: "Name");
        }
    }
}
