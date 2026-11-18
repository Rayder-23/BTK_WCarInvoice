using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarInvoice.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChassisCodeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChassisCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DefaultMake = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DefaultModel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DefaultEngineCc = table.Column<int>(type: "INTEGER", nullable: true),
                    DefaultFuelType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChassisCodeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TaxOrNationalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JapaneseTermGlossary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JapaneseTerm = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EnglishTranslation = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JapaneseTermGlossary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChassisCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ChassisNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FullChassisNormalized = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    GradeTrim = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ManufacturingYear = table.Column<int>(type: "INTEGER", nullable: false),
                    RegistrationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineCapacityCc = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExteriorColor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Transmission = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Mileage = table.Column<int>(type: "INTEGER", nullable: false),
                    MileageUnit = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false, defaultValue: "km"),
                    AuctionGrade = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    HistoryNotes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    SalePrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AdditionalFees = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false, defaultValue: "PKR"),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    GeneratedDocPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ChassisCodeLookups",
                columns: new[] { "Id", "ChassisCode", "DefaultEngineCc", "DefaultFuelType", "DefaultMake", "DefaultModel" },
                values: new object[,]
                {
                    { 1, "NZE161", 1500, "Petrol", "Toyota", "Corolla Axio" },
                    { 2, "NZE164", 1500, "Petrol", "Toyota", "Corolla Axio" },
                    { 3, "NKE165", 1500, "Hybrid", "Toyota", "Corolla Axio Hybrid" },
                    { 4, "ZRE142", 1800, "Petrol", "Toyota", "Corolla Axio" },
                    { 5, "ZVW30", 1800, "Hybrid", "Toyota", "Prius" },
                    { 6, "ZVW50", 1800, "Hybrid", "Toyota", "Prius" },
                    { 7, "ZVW51", 1800, "Hybrid", "Toyota", "Prius" },
                    { 8, "NHP10", 1500, "Hybrid", "Toyota", "Aqua" },
                    { 9, "NHP10H", 1500, "Hybrid", "Toyota", "Aqua" },
                    { 10, "KSP130", 1000, "Petrol", "Toyota", "Vitz" },
                    { 11, "NSP130", 1300, "Petrol", "Toyota", "Vitz" },
                    { 12, "NCP131", 1500, "Petrol", "Toyota", "Vitz" },
                    { 13, "ZRR80", 2000, "Petrol", "Toyota", "Voxy" },
                    { 14, "ZRR85", 1800, "Hybrid", "Toyota", "Voxy Hybrid" },
                    { 15, "GRX130", 2500, "Petrol", "Toyota", "Mark X" },
                    { 16, "GRS204", 2500, "Petrol", "Toyota", "Crown Athlete" },
                    { 17, "AWS210", 2500, "Hybrid", "Toyota", "Crown Hybrid" },
                    { 18, "ACA31", 2400, "Petrol", "Toyota", "RAV4" },
                    { 19, "HA36S", 660, "Petrol", "Suzuki", "Alto" },
                    { 20, "HA25S", 660, "Petrol", "Suzuki", "Alto" },
                    { 21, "MH34S", 660, "Petrol", "Suzuki", "Wagon R" },
                    { 22, "MH55S", 660, "Petrol", "Suzuki", "Wagon R" },
                    { 23, "ZC72S", 1200, "Petrol", "Suzuki", "Swift" },
                    { 24, "ZC32S", 1600, "Petrol", "Suzuki", "Swift Sport" },
                    { 25, "LA300S", 660, "Petrol", "Daihatsu", "Mira e:S" },
                    { 26, "LA150S", 660, "Petrol", "Daihatsu", "Move" },
                    { 27, "LA600S", 660, "Petrol", "Daihatsu", "Tanto" },
                    { 28, "B21W", 660, "Petrol", "Mitsubishi", "eK Wagon" },
                    { 29, "JF1", 660, "Petrol", "Honda", "N-BOX" },
                    { 30, "JF3", 660, "Petrol", "Honda", "N-BOX" },
                    { 31, "GK3", 1300, "Petrol", "Honda", "Fit" },
                    { 32, "GP5", 1500, "Hybrid", "Honda", "Fit Hybrid" },
                    { 33, "RU3", 1500, "Hybrid", "Honda", "Vezel" },
                    { 34, "RU1", 1500, "Petrol", "Honda", "Vezel" },
                    { 35, "ZE2", 1300, "Hybrid", "Honda", "Insight" }
                });

            migrationBuilder.InsertData(
                table: "JapaneseTermGlossary",
                columns: new[] { "Id", "Category", "EnglishTranslation", "JapaneseTerm" },
                values: new object[,]
                {
                    { 1, "Color", "Pearl White", "パール" },
                    { 2, "Color", "Pearl White", "パールホワイト" },
                    { 3, "Color", "White", "ホワイト" },
                    { 4, "Color", "White", "白" },
                    { 5, "Color", "Black", "ブラック" },
                    { 6, "Color", "Black", "黒" },
                    { 7, "Color", "Silver", "シルバー" },
                    { 8, "Color", "Silver", "銀" },
                    { 9, "Color", "Grey", "グレー" },
                    { 10, "Color", "Red", "レッド" },
                    { 11, "Color", "Red", "赤" },
                    { 12, "Color", "Blue", "ブルー" },
                    { 13, "Color", "Blue", "青" },
                    { 14, "Color", "Navy Blue", "ネイビー" },
                    { 15, "Color", "Green", "グリーン" },
                    { 16, "Color", "Bronze", "ブロンズ" },
                    { 17, "Color", "Beige", "ベージュ" },
                    { 18, "Color", "Brown", "ブラウン" },
                    { 19, "Color", "Gold", "ゴールド" },
                    { 20, "Color", "Orange", "オレンジ" },
                    { 21, "Color", "Wine Red", "ワイン" },
                    { 22, "Color", "Metallic", "メタリック" },
                    { 23, "Condition/History", "Repair History Present (Accident Repaired)", "修復歴あり" },
                    { 24, "Condition/History", "No Repair History", "修復歴なし" },
                    { 25, "Condition/History", "Repair History", "修復歴" },
                    { 26, "Condition/History", "Accident History Present", "事故歴あり" },
                    { 27, "Condition/History", "No Accident History", "事故歴なし" },
                    { 28, "Condition/History", "One Owner", "ワンオーナー" },
                    { 29, "Condition/History", "Non-Smoking Vehicle", "禁煙車" },
                    { 30, "Condition/History", "Smoking Vehicle", "喫煙車" },
                    { 31, "Condition/History", "Pet Usage Present", "ペット乗車あり" },
                    { 32, "Condition/History", "Mileage", "走行距離" },
                    { 33, "Condition/History", "Genuine Mileage", "実走行" },
                    { 34, "Condition/History", "Odometer Tampering Suspected", "改ざん疑義" },
                    { 35, "Condition/History", "Shaken (Inspection)", "車検" },
                    { 36, "Condition/History", "Shaken Expired", "車検切れ" },
                    { 37, "Condition/History", "Recycling Certificate", "リサイクル券" },
                    { 38, "Condition/History", "Documents Complete", "書類完備" },
                    { 39, "Condition/History", "Flood History", "冠水歴" },
                    { 40, "Condition/History", "Hail Damage", "雹害" },
                    { 41, "Fuel/Transmission", "Hybrid", "ハイブリッド" },
                    { 42, "Fuel/Transmission", "Petrol", "ガソリン" },
                    { 43, "Fuel/Transmission", "Diesel", "ディーゼル" },
                    { 44, "Fuel/Transmission", "Electric Vehicle (EV)", "電気自動車" },
                    { 45, "Fuel/Transmission", "Automatic", "オートマ" },
                    { 46, "Fuel/Transmission", "Automatic", "AT" },
                    { 47, "Fuel/Transmission", "Manual", "MT" },
                    { 48, "Fuel/Transmission", "Manual", "マニュアル" },
                    { 49, "Fuel/Transmission", "CVT", "CVT" },
                    { 50, "Fuel/Transmission", "4WD", "4WD" },
                    { 51, "Fuel/Transmission", "2WD", "2WD" },
                    { 52, "Fuel/Transmission", "Front-Wheel Drive", "FF" },
                    { 53, "Grade/Spec", "Navigation", "ナビ" },
                    { 54, "Grade/Spec", "Reverse Camera", "バックカメラ" },
                    { 55, "Grade/Spec", "Push Start", "プッシュスタート" },
                    { 56, "Grade/Spec", "Smart Key", "スマートキー" },
                    { 57, "Grade/Spec", "Sunroof", "サンルーフ" },
                    { 58, "Grade/Spec", "Leather Seats", "レザーシート" },
                    { 59, "Grade/Spec", "Power Seats", "パワーシート" },
                    { 60, "Grade/Spec", "Cruise Control", "クルーズコントロール" },
                    { 61, "Grade/Spec", "Airbags", "エアバッグ" },
                    { 62, "Grade/Spec", "ABS", "ABS" },
                    { 63, "Grade/Spec", "ETC", "ETC" },
                    { 64, "Grade/Spec", "Alloy Wheels", "アルミホイール" },
                    { 65, "Grade/Spec", "Fog Lamps", "フォグランプ" },
                    { 66, "Grade/Spec", "LED Headlights", "LEDヘッドライト" },
                    { 67, "Grade/Spec", "Dual Power Sliding Doors", "両側パワースライド" },
                    { 68, "Grade/Spec", "Power Sliding Door", "パワースライドドア" },
                    { 69, "Grade/Spec", "Model Year", "年式" },
                    { 70, "Grade/Spec", "Model Code", "型式" },
                    { 71, "Grade/Spec", "Chassis Number", "車台番号" },
                    { 72, "Grade/Spec", "Engine Displacement", "排気量" },
                    { 73, "Grade/Spec", "Grade", "グレード" },
                    { 74, "Grade/Spec", "Auction Grade", "評価点" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChassisCodeLookups_ChassisCode",
                table: "ChassisCodeLookups",
                column: "ChassisCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FullName",
                table: "Customers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VehicleId",
                table: "Invoices",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_JapaneseTermGlossary_Category",
                table: "JapaneseTermGlossary",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_JapaneseTermGlossary_JapaneseTerm",
                table: "JapaneseTermGlossary",
                column: "JapaneseTerm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ChassisCode",
                table: "Vehicles",
                column: "ChassisCode");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ChassisNumber",
                table: "Vehicles",
                column: "ChassisNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FullChassisNormalized",
                table: "Vehicles",
                column: "FullChassisNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Make_Model",
                table: "Vehicles",
                columns: new[] { "Make", "Model" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "ChassisCodeLookups");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "JapaneseTermGlossary");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
