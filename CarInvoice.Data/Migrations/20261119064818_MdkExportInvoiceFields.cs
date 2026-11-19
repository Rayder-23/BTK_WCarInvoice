using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInvoice.Data.Migrations
{
    /// <inheritdoc />
    public partial class MdkExportInvoiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MileageUnit",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "KM",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16,
                oldDefaultValue: "km");

            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "PETROL",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "USED");

            migrationBuilder.AddColumn<int>(
                name: "Doors",
                table: "Vehicles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seats",
                table: "Vehicles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Steering",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "RHD");

            migrationBuilder.AddColumn<string>(
                name: "StockNumber",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnladenWeightKg",
                table: "Vehicles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Invoices",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8,
                oldDefaultValue: "PKR");

            migrationBuilder.AddColumn<int>(
                name: "BankProfileId",
                table: "Invoices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsigneeAddress",
                table: "Invoices",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsigneeName",
                table: "Invoices",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationCountry",
                table: "Invoices",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationPort",
                table: "Invoices",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConsigneeSameAsCustomer",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAmount",
                table: "Invoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TradeTerm",
                table: "Invoices",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "C & F");

            migrationBuilder.AddColumn<string>(
                name: "DefaultBodyType",
                table: "ChassisCodeLookups",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultDoors",
                table: "ChassisCodeLookups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultEngineNumber",
                table: "ChassisCodeLookups",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSeats",
                table: "ChassisCodeLookups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    AccountHolderName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    BranchName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BranchCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    BranchAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SwiftCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    IntermediaryBankName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IntermediaryCountry = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IntermediaryBranch = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IntermediarySwiftCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankProfiles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BankProfiles",
                columns: new[] { "Id", "AccountHolderName", "AccountNumber", "BankName", "BranchAddress", "BranchCode", "BranchName", "IntermediaryBankName", "IntermediaryBranch", "IntermediaryCountry", "IntermediarySwiftCode", "IsDefault", "ProfileName", "SwiftCode" },
                values: new object[] { 1, "MDK CORPORATION CO. LTD", "0043215", "MUFG Bank, Ltd.", "KANAZAWA (0005), KANAZAWASHI ISHIKAWA, JAPAN", "0005", "Kanazawa", "MUFG Bank, Ltd.", "Mitsubishi Newyork Branch", "Newyork, USA", "BOTKUS33", true, "MUFG Bank Kanazawa (Default)", "BOTKJPJT" });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "DefaultBodyType", "DefaultDoors", "DefaultEngineNumber", "DefaultSeats" },
                values: new object[] { null, null, null, null });

            migrationBuilder.InsertData(
                table: "ChassisCodeLookups",
                columns: new[] { "Id", "ChassisCode", "DefaultBodyType", "DefaultDoors", "DefaultEngineCc", "DefaultEngineNumber", "DefaultFuelType", "DefaultMake", "DefaultModel", "DefaultSeats" },
                values: new object[] { 36, "VR2E26", "MINI BUS", 5, 2000, "QR20", "PETROL", "NISSAN", "CARAVAN", 6 });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_StockNumber",
                table: "Vehicles",
                column: "StockNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BankProfileId",
                table: "Invoices",
                column: "BankProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BankProfiles_IsDefault",
                table: "BankProfiles",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_BankProfiles_ProfileName",
                table: "BankProfiles",
                column: "ProfileName");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_BankProfiles_BankProfileId",
                table: "Invoices",
                column: "BankProfileId",
                principalTable: "BankProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_BankProfiles_BankProfileId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "BankProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_StockNumber",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BankProfileId",
                table: "Invoices");

            migrationBuilder.DeleteData(
                table: "ChassisCodeLookups",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Doors",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Seats",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Steering",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "StockNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UnladenWeightKg",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "BankProfileId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ConsigneeAddress",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ConsigneeName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DestinationCountry",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DestinationPort",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsConsigneeSameAsCustomer",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PriceAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TradeTerm",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DefaultBodyType",
                table: "ChassisCodeLookups");

            migrationBuilder.DropColumn(
                name: "DefaultDoors",
                table: "ChassisCodeLookups");

            migrationBuilder.DropColumn(
                name: "DefaultEngineNumber",
                table: "ChassisCodeLookups");

            migrationBuilder.DropColumn(
                name: "DefaultSeats",
                table: "ChassisCodeLookups");

            migrationBuilder.AlterColumn<string>(
                name: "MileageUnit",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "km",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16,
                oldDefaultValue: "KM");

            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32,
                oldDefaultValue: "PETROL");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Invoices",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "PKR",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8,
                oldDefaultValue: "USD");
        }
    }
}
