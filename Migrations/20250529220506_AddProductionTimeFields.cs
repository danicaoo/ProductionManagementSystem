using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionTimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkOrders",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "ProductMaterials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "CurrentWorkOrderId",
                table: "ProductionLines",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "ProductMaterials");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkOrders",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentWorkOrderId",
                table: "ProductionLines",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);
        }
    }
}
