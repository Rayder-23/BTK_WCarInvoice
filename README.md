<p align="center">
  <img src="CarInvoice.Desktop/Assets/logo-512.png" alt="CarInvoice logo" width="160" />
</p>

# CarInvoice

Offline-first **.NET 8** Windows desktop app for second-hand / JDM car invoice generation. Built for Bahria Town / export-desk workflows: pull vehicle details from Outlook or auction sheets, normalize chassis plates, translate Japanese auction terms locally, and generate Word invoices without needing a live server.

## Features

- **Create Invoice** — load from Classic Outlook, drop `.msg` / `.eml`, import Excel stock rows, or enter manually
- **Chassis parsing** — messy inputs like `DBA-NZE161-7048291` → code + number, lookup make/model/CC, duplicate-chassis alerts
- **Japanese glossary** — offline JP → EN translation for colors, history, fuel/spec terms (editable in Settings)
- **Word invoices** — MiniWord tag replacement against a `.docx` template (`{{InvoiceNumber}}`, `{{ChassisCode}}`, …)
- **Excel** — ClosedXML import (EN + JP headers) and ledger/vehicle export
- **Vehicles & Invoices** — searchable local registry and invoice ledger
- **Shaken & Washi UI** — Gofun / Kachi / Ai-iro theme with Yu Gothic typography

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
| `CarInvoice.Core` | Entities, DTOs, interfaces, chassis parser, JP translator, Outlook/Excel/Word services |
| `CarInvoice.Data` | EF Core SQLite, migrations, repositories / unit of work, seed data |
| `CarInvoice.Desktop` | WPF + Wpf.Ui + CommunityToolkit.Mvvm shell and views |

## Local data

On first launch the app creates:

| Path | Contents |
|------|----------|
| `%LocalAppData%\CarInvoice\carinvoice.db` | SQLite DB (vehicles, customers, invoices, lookups, glossary) |
| `%LocalAppData%\CarInvoice\Templates\DefaultInvoiceTemplate.docx` | Default Word template (if none configured) |
| `%LocalAppData%\CarInvoice\Invoices\` | Generated `.docx` output |
| `%LocalAppData%\CarInvoice\Backups\` | Automatic DB backups |

Seeded on first run: **35+** common JDM chassis codes and **70+** Japanese auction/glossary terms. Both are editable under **Dictionaries & Settings**.

## Invoice template tags

Point Settings → Word Template at your client layout. Supported MiniWord tags include:

`InvoiceNumber`, `InvoiceDate`, `CustomerName`, `CustomerPhone`, `CustomerEmail`, `CustomerAddress`, `CompanyName`, `Make`, `Model`, `GradeTrim`, `Year`, `ChassisCode`, `ChassisNumber`, `EngineCc`, `EngineNumber`, `Color`, `Mileage`, `FuelType`, `Transmission`, `AuctionGrade`, `HistoryNotes`, `SalePrice`, `TaxAmount`, `AdditionalFees`, `TotalAmount`, `Currency`, `PaymentMethod`, `Notes`

## UI theme

Visual language and brush/typography rules live in:

- `CarInvoice.Desktop/Themes/JapaneseTheme.xaml`
- `.cursor/rules/ui-japanese-theme.mdc` (agent/editor always-apply rule)

Primary buttons use **Ai-iro** (`#2B4C7E`), not the Windows accent color.

## License

See [LICENSE](LICENSE).

## Developed By

**[Rayder-23](https://github.com/Rayder-23)**
