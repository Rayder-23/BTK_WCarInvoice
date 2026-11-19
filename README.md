<p align="center">
  <img src="CarInvoice.Desktop/Assets/logo-512.png" alt="CarInvoice logo" width="160" />
</p>

# CarInvoice

Offline-first **.NET 8** Windows desktop app for second-hand / JDM **export invoice** generation (MDK-style). Built for Bahria Town / export-desk workflows: pull vehicle details from Outlook or auction sheets, normalize chassis plates, translate Japanese auction terms locally, verify key fields, and generate Word invoices without a live server.

## Features

- **Create Invoice** — Classic Outlook inbox, drop `.msg` / `.eml`, paste email text, Excel stock rows, or manual entry
- **Chassis parsing** — messy plates like `DBA-NZE161-7048291` → code + number; multi-model lookup when one code maps to several cars; duplicate-chassis alerts
- **Pre-generate verification** — confirm color, year, odometer, and price/currency before writing the Word doc
- **Per-invoice currency** — pick USD, PKR, JPY, EUR, … (or type a code); amount is what you enter — **no FX conversion**
- **Bank profiles** — remittance details (e.g. MUFG) stored in SQLite and selected per invoice
- **Japanese glossary** — offline JP → EN for colors, history, fuel/spec; editable in Settings; color suggestions come from glossary
- **Dictionary learning** — successful generates can learn new chassis mappings and glossary terms
- **Word invoices** — MiniWord tags against a `.docx` template; money tags include the chosen currency code
- **Excel** — ClosedXML import/export for vehicles, ledger, and Settings dictionaries (chassis / glossary / bank)
- **Vehicles & Invoices** — searchable local registry and invoice ledger; open generated docs from the success banner
- **Shaken & Washi UI** — Gofun / Kachi / Ai-iro theme; Consolas paths (Yu Gothic would draw `\` as ¥)

## Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Optional: **Classic Outlook** desktop for live Inbox / draft-with-attachment (late-bound COM; app still runs without Office)

## Quick start

```powershell
dotnet restore CarInvoice.sln
dotnet build CarInvoice.sln -c Release
dotnet run --project CarInvoice.Desktop -c Release
```

Or open `CarInvoice.sln` in Visual Studio 2022 and run the **CarInvoice.Desktop** project.

## Solution layout

| Project | Role |
|---------|------|
| `CarInvoice.Core` | Entities, DTOs, interfaces, chassis parser, JP translator, Outlook/Excel/Word, dictionary learning |
| `CarInvoice.Data` | EF Core SQLite, migrations, repositories / unit of work, seed data |
| `CarInvoice.Desktop` | WPF + Wpf.Ui + CommunityToolkit.Mvvm shell and views |

## Local data

On first launch the app creates:

| Path | Contents |
|------|----------|
| `%LocalAppData%\CarInvoice\carinvoice.db` | SQLite DB (vehicles, customers, invoices, bank profiles, lookups, glossary, settings) |
| `%LocalAppData%\CarInvoice\Templates\DefaultInvoiceTemplate.docx` | Default Word template (if none configured) |
| `%LocalAppData%\CarInvoice\Invoices\` | Generated `.docx` output |
| `%LocalAppData%\CarInvoice\Backups\` | Automatic DB backups |

Configure template / output / backup folders and **default currency** under **Dictionaries & Settings**. Folder Browse uses the Windows folder picker and applies immediately.

Seeded on first run: **35+** common JDM chassis codes (including multi-model rows), **70+** Japanese glossary terms, and a default **bank profile**. All are editable under Settings; chassis / glossary / bank can be imported and exported as Excel.

## Create Invoice flow (locked layout)

1. Compact page header + source toolbar (Outlook / paste / drop / Excel / clear)
2. Chassis plate card (Cascadia Mono) + optional multi-model picker
3. **Invoice & destination** | **Buyer & consignee** side by side (trade term, currency + price, bank profile)
4. **Vehicle** full-width card (color, year, odometer, body, fuel, …)
5. Collapsed **JP → EN** raw source expander at the bottom
6. Required-field highlighting → verification dialog → generate; success banner shows Consolas path + open-folder / open-invoice icons

Numeric fields (price, year, odometer, CC, doors, seats, weight) accept digits only. ComboBox dropdowns scroll with the mouse wheel without stealing page scroll (`ComboBoxWheelFix`).

## Invoice template tags

Point Settings → Word Template at your client layout. Supported MiniWord tags include:

**Header / parties:** `InvoiceNo` / `InvoiceNumber`, `InvoiceDate`, `Port`, `Country`, `CustomerName`, `CustomerAddress`, `CustomerPhone`, `CustomerEmail`, `CompanyName`, `ConsigneeName`, `ConsigneeAddress`

**Vehicle:** `StockNo`, `Maker`, `Make`, `Model`, `Chassis`, `ChassisCode`, `ChassisNumber`, `CC` / `EngineCc`, `Color`, `Year`, `Odometer` / `Mileage`, `Door`, `Condition`, `Steering`, `EngineNo`, `Seat`, `Body`, `Fuel` / `FuelType`, `UnladenWeight`, `GradeTrim`, `Transmission`, `AuctionGrade`, `HistoryNotes`

**Pricing:** `TradeTerm`, `CandF`, `SalePrice`, `TaxAmount`, `AdditionalFees`, `TotalAmount` (alias `TotalUsd`), `Currency`, `PaymentMethod`, `Notes`

**Bank:** `BankName`, `AccountHolderName`, `AccountNo` / `AccountNumber`, `Branch`, `BranchCode`, `BranchAddress`, `SwiftCode`, `IntermediaryBankName`, `IntermediaryCountry`, `IntermediaryBranch`, `IntermediarySwiftCode`

Money values are formatted as `{Currency} {amount}` (e.g. `USD 12,500`). The built-in default template uses `TOTAL {{Currency}}: {{TotalAmount}}` and `{{AccountHolderName}}` for the company header.

## UI theme

Visual language and brush/typography rules live in:

- `CarInvoice.Desktop/Themes/JapaneseTheme.xaml`
- `.cursor/rules/ui-japanese-theme.mdc` (agent/editor always-apply rule)

Primary buttons use **Ai-iro** (`#2B4C7E`), not the Windows accent color. Paths use `PathFont` / `PathTextBoxStyle` / `PathTextStyle` (Consolas) so backslashes stay readable under Yu Gothic UI.

## License

See [LICENSE](LICENSE).

## Developed By

**[Rayder-23](https://github.com/Rayder-23)**
