using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInvoice.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultiModelChassisAndLearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChassisCodeLookups_ChassisCode",
                table: "ChassisCodeLookups");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ChassisCodeLookups",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialPrefixOrRange",
                table: "ChassisCodeLookups",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultFuelType", "DefaultMake", "DefaultModel", "Notes", "SerialPrefixOrRange" },
                values: new object[] { "HATCHBACK", 5, "PETROL", "TOYOTA", "VITZ", "Vitz / Yaris hatchback", null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Notes", "SerialPrefixOrRange" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "ChassisCodeLookups",
                columns: new[] { "Id", "ChassisCode", "DefaultBodyType", "DefaultDoors", "DefaultEngineCc", "DefaultEngineNumber", "DefaultFuelType", "DefaultMake", "DefaultModel", "DefaultSeats", "Notes", "SerialPrefixOrRange" },
                values: new object[] { 37, "KSP130", "VAN", 5, 1300, null, "PETROL", "TOYOTA", "PROBOX", null, "Also 1000cc / WAGON variants; serial ranges may differ by market", null });

            migrationBuilder.CreateIndex(
                name: "IX_ChassisCodeLookups_ChassisCode",
                table: "ChassisCodeLookups",
                column: "ChassisCode");

            migrationBuilder.CreateIndex(
                name: "IX_ChassisCodeLookups_ChassisCode_DefaultMake_DefaultModel_SerialPrefixOrRange",
                table: "ChassisCodeLookups",
                columns: new[] { "ChassisCode", "DefaultMake", "DefaultModel", "SerialPrefixOrRange" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChassisCodeLookups_ChassisCode",
                table: "ChassisCodeLookups");

            migrationBuilder.DropIndex(
                name: "IX_ChassisCodeLookups_ChassisCode_DefaultMake_DefaultModel_SerialPrefixOrRange",
                table: "ChassisCodeLookups");

            migrationBuilder.DeleteData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ChassisCodeLookups");

            migrationBuilder.DropColumn(
                name: "SerialPrefixOrRange",
                table: "ChassisCodeLookups");

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultFuelType", "DefaultMake", "DefaultModel" },
                values: new object[] { null, null, "Petrol", "Toyota", "Vitz" });

            migrationBuilder.CreateIndex(
                name: "IX_ChassisCodeLookups_ChassisCode",
                table: "ChassisCodeLookups",
                column: "ChassisCode",
                unique: true);
        }
    }
}
