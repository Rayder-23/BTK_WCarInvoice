using CarInvoice.Core.Entities;

namespace CarInvoice.Core.Seed;

/// <summary>
/// Default chassis lookups and Japanese glossary terms seeded on first database creation.
/// </summary>
public static class SeedData
{
    public static IReadOnlyList<ChassisCodeLookup> ChassisLookups { get; } = new List<ChassisCodeLookup>
    {
        new() { ChassisCode = "NZE161", DefaultMake = "Toyota", DefaultModel = "Corolla Axio", DefaultEngineCc = 1500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "NZE164", DefaultMake = "Toyota", DefaultModel = "Corolla Axio", DefaultEngineCc = 1500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "NKE165", DefaultMake = "Toyota", DefaultModel = "Corolla Axio Hybrid", DefaultEngineCc = 1500, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "ZRE142", DefaultMake = "Toyota", DefaultModel = "Corolla Axio", DefaultEngineCc = 1800, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZVW30", DefaultMake = "Toyota", DefaultModel = "Prius", DefaultEngineCc = 1800, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "ZVW50", DefaultMake = "Toyota", DefaultModel = "Prius", DefaultEngineCc = 1800, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "ZVW51", DefaultMake = "Toyota", DefaultModel = "Prius", DefaultEngineCc = 1800, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "NHP10", DefaultMake = "Toyota", DefaultModel = "Aqua", DefaultEngineCc = 1500, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "NHP10H", DefaultMake = "Toyota", DefaultModel = "Aqua", DefaultEngineCc = 1500, DefaultFuelType = "Hybrid" },
        new()
        {
            ChassisCode = "KSP130",
            DefaultMake = "TOYOTA",
            DefaultModel = "VITZ",
            DefaultEngineCc = 1000,
            DefaultFuelType = "PETROL",
            DefaultBodyType = "HATCHBACK",
            DefaultDoors = 5,
            Notes = "Vitz / Yaris hatchback"
        },
        new() { ChassisCode = "NSP130", DefaultMake = "Toyota", DefaultModel = "Vitz", DefaultEngineCc = 1300, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "NCP131", DefaultMake = "Toyota", DefaultModel = "Vitz", DefaultEngineCc = 1500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZRR80", DefaultMake = "Toyota", DefaultModel = "Voxy", DefaultEngineCc = 2000, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZRR85", DefaultMake = "Toyota", DefaultModel = "Voxy Hybrid", DefaultEngineCc = 1800, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "GRX130", DefaultMake = "Toyota", DefaultModel = "Mark X", DefaultEngineCc = 2500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "GRS204", DefaultMake = "Toyota", DefaultModel = "Crown Athlete", DefaultEngineCc = 2500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "AWS210", DefaultMake = "Toyota", DefaultModel = "Crown Hybrid", DefaultEngineCc = 2500, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "ACA31", DefaultMake = "Toyota", DefaultModel = "RAV4", DefaultEngineCc = 2400, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "HA36S", DefaultMake = "Suzuki", DefaultModel = "Alto", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "HA25S", DefaultMake = "Suzuki", DefaultModel = "Alto", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "MH34S", DefaultMake = "Suzuki", DefaultModel = "Wagon R", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "MH55S", DefaultMake = "Suzuki", DefaultModel = "Wagon R", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZC72S", DefaultMake = "Suzuki", DefaultModel = "Swift", DefaultEngineCc = 1200, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZC32S", DefaultMake = "Suzuki", DefaultModel = "Swift Sport", DefaultEngineCc = 1600, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "LA300S", DefaultMake = "Daihatsu", DefaultModel = "Mira e:S", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "LA150S", DefaultMake = "Daihatsu", DefaultModel = "Move", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "LA600S", DefaultMake = "Daihatsu", DefaultModel = "Tanto", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "B21W", DefaultMake = "Mitsubishi", DefaultModel = "eK Wagon", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "JF1", DefaultMake = "Honda", DefaultModel = "N-BOX", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "JF3", DefaultMake = "Honda", DefaultModel = "N-BOX", DefaultEngineCc = 660, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "GK3", DefaultMake = "Honda", DefaultModel = "Fit", DefaultEngineCc = 1300, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "GP5", DefaultMake = "Honda", DefaultModel = "Fit Hybrid", DefaultEngineCc = 1500, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "RU3", DefaultMake = "Honda", DefaultModel = "Vezel", DefaultEngineCc = 1500, DefaultFuelType = "Hybrid" },
        new() { ChassisCode = "RU1", DefaultMake = "Honda", DefaultModel = "Vezel", DefaultEngineCc = 1500, DefaultFuelType = "Petrol" },
        new() { ChassisCode = "ZE2", DefaultMake = "Honda", DefaultModel = "Insight", DefaultEngineCc = 1300, DefaultFuelType = "Hybrid" },
        new()
        {
            ChassisCode = "VR2E26",
            DefaultMake = "NISSAN",
            DefaultModel = "CARAVAN",
            DefaultEngineCc = 2000,
            DefaultFuelType = "PETROL",
            DefaultBodyType = "MINI BUS",
            DefaultDoors = 5,
            DefaultSeats = 6,
            DefaultEngineNumber = "QR20"
        },
        // Shared chassis code — multi-model picker demo (same code as VITZ above).
        new()
        {
            ChassisCode = "KSP130",
            DefaultMake = "TOYOTA",
            DefaultModel = "PROBOX",
            DefaultEngineCc = 1300,
            DefaultFuelType = "PETROL",
            DefaultBodyType = "VAN",
            DefaultDoors = 5,
            Notes = "Also 1000cc / WAGON variants; serial ranges may differ by market"
        },
    };

    public static IReadOnlyList<BankProfile> BankProfiles { get; } = new List<BankProfile>
    {
        new()
        {
            ProfileName = "MUFG Bank Kanazawa (Default)",
            IsDefault = true,
            BankName = "MUFG Bank, Ltd.",
            AccountHolderName = "MDK CORPORATION CO. LTD",
            AccountNumber = "0043215",
            BranchName = "Kanazawa",
            BranchCode = "0005",
            BranchAddress = "KANAZAWA (0005), KANAZAWASHI ISHIKAWA, JAPAN",
            SwiftCode = "BOTKJPJT",
            IntermediaryBankName = "MUFG Bank, Ltd.",
            IntermediaryCountry = "Newyork, USA",
            IntermediaryBranch = "Mitsubishi Newyork Branch",
            IntermediarySwiftCode = "BOTKUS33"
        }
    };

    public static IReadOnlyList<JapaneseTermGlossary> GlossaryTerms { get; } = new List<JapaneseTermGlossary>
    {
        // Colors
        new() { JapaneseTerm = "パール", EnglishTranslation = "Pearl White", Category = "Color" },
        new() { JapaneseTerm = "パールホワイト", EnglishTranslation = "Pearl White", Category = "Color" },
        new() { JapaneseTerm = "ホワイト", EnglishTranslation = "White", Category = "Color" },
        new() { JapaneseTerm = "白", EnglishTranslation = "White", Category = "Color" },
        new() { JapaneseTerm = "ブラック", EnglishTranslation = "Black", Category = "Color" },
        new() { JapaneseTerm = "黒", EnglishTranslation = "Black", Category = "Color" },
        new() { JapaneseTerm = "シルバー", EnglishTranslation = "Silver", Category = "Color" },
        new() { JapaneseTerm = "銀", EnglishTranslation = "Silver", Category = "Color" },
        new() { JapaneseTerm = "グレー", EnglishTranslation = "Grey", Category = "Color" },
        new() { JapaneseTerm = "レッド", EnglishTranslation = "Red", Category = "Color" },
        new() { JapaneseTerm = "赤", EnglishTranslation = "Red", Category = "Color" },
        new() { JapaneseTerm = "ブルー", EnglishTranslation = "Blue", Category = "Color" },
        new() { JapaneseTerm = "青", EnglishTranslation = "Blue", Category = "Color" },
        new() { JapaneseTerm = "ネイビー", EnglishTranslation = "Navy Blue", Category = "Color" },
        new() { JapaneseTerm = "グリーン", EnglishTranslation = "Green", Category = "Color" },
        new() { JapaneseTerm = "ブロンズ", EnglishTranslation = "Bronze", Category = "Color" },
        new() { JapaneseTerm = "ベージュ", EnglishTranslation = "Beige", Category = "Color" },
        new() { JapaneseTerm = "ブラウン", EnglishTranslation = "Brown", Category = "Color" },
        new() { JapaneseTerm = "ゴールド", EnglishTranslation = "Gold", Category = "Color" },
        new() { JapaneseTerm = "オレンジ", EnglishTranslation = "Orange", Category = "Color" },
        new() { JapaneseTerm = "ワイン", EnglishTranslation = "Wine Red", Category = "Color" },
        new() { JapaneseTerm = "メタリック", EnglishTranslation = "Metallic", Category = "Color" },

        // Condition / History
        new() { JapaneseTerm = "修復歴あり", EnglishTranslation = "Repair History Present (Accident Repaired)", Category = "Condition/History" },
        new() { JapaneseTerm = "修復歴なし", EnglishTranslation = "No Repair History", Category = "Condition/History" },
        new() { JapaneseTerm = "修復歴", EnglishTranslation = "Repair History", Category = "Condition/History" },
        new() { JapaneseTerm = "事故歴あり", EnglishTranslation = "Accident History Present", Category = "Condition/History" },
        new() { JapaneseTerm = "事故歴なし", EnglishTranslation = "No Accident History", Category = "Condition/History" },
        new() { JapaneseTerm = "ワンオーナー", EnglishTranslation = "One Owner", Category = "Condition/History" },
        new() { JapaneseTerm = "禁煙車", EnglishTranslation = "Non-Smoking Vehicle", Category = "Condition/History" },
        new() { JapaneseTerm = "喫煙車", EnglishTranslation = "Smoking Vehicle", Category = "Condition/History" },
        new() { JapaneseTerm = "ペット乗車あり", EnglishTranslation = "Pet Usage Present", Category = "Condition/History" },
        new() { JapaneseTerm = "走行距離", EnglishTranslation = "Mileage", Category = "Condition/History" },
        new() { JapaneseTerm = "実走行", EnglishTranslation = "Genuine Mileage", Category = "Condition/History" },
        new() { JapaneseTerm = "改ざん疑義", EnglishTranslation = "Odometer Tampering Suspected", Category = "Condition/History" },
        new() { JapaneseTerm = "車検", EnglishTranslation = "Shaken (Inspection)", Category = "Condition/History" },
        new() { JapaneseTerm = "車検切れ", EnglishTranslation = "Shaken Expired", Category = "Condition/History" },
        new() { JapaneseTerm = "リサイクル券", EnglishTranslation = "Recycling Certificate", Category = "Condition/History" },
        new() { JapaneseTerm = "書類完備", EnglishTranslation = "Documents Complete", Category = "Condition/History" },
        new() { JapaneseTerm = "冠水歴", EnglishTranslation = "Flood History", Category = "Condition/History" },
        new() { JapaneseTerm = "雹害", EnglishTranslation = "Hail Damage", Category = "Condition/History" },

        // Fuel / Transmission
        new() { JapaneseTerm = "ハイブリッド", EnglishTranslation = "Hybrid", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "ガソリン", EnglishTranslation = "Petrol", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "ディーゼル", EnglishTranslation = "Diesel", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "電気自動車", EnglishTranslation = "Electric Vehicle (EV)", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "オートマ", EnglishTranslation = "Automatic", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "AT", EnglishTranslation = "Automatic", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "MT", EnglishTranslation = "Manual", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "マニュアル", EnglishTranslation = "Manual", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "CVT", EnglishTranslation = "CVT", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "4WD", EnglishTranslation = "4WD", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "2WD", EnglishTranslation = "2WD", Category = "Fuel/Transmission" },
        new() { JapaneseTerm = "FF", EnglishTranslation = "Front-Wheel Drive", Category = "Fuel/Transmission" },

        // Grade / Spec
        new() { JapaneseTerm = "ナビ", EnglishTranslation = "Navigation", Category = "Grade/Spec" },
        new() { JapaneseTerm = "バックカメラ", EnglishTranslation = "Reverse Camera", Category = "Grade/Spec" },
        new() { JapaneseTerm = "プッシュスタート", EnglishTranslation = "Push Start", Category = "Grade/Spec" },
        new() { JapaneseTerm = "スマートキー", EnglishTranslation = "Smart Key", Category = "Grade/Spec" },
        new() { JapaneseTerm = "サンルーフ", EnglishTranslation = "Sunroof", Category = "Grade/Spec" },
        new() { JapaneseTerm = "レザーシート", EnglishTranslation = "Leather Seats", Category = "Grade/Spec" },
        new() { JapaneseTerm = "パワーシート", EnglishTranslation = "Power Seats", Category = "Grade/Spec" },
        new() { JapaneseTerm = "クルーズコントロール", EnglishTranslation = "Cruise Control", Category = "Grade/Spec" },
        new() { JapaneseTerm = "エアバッグ", EnglishTranslation = "Airbags", Category = "Grade/Spec" },
        new() { JapaneseTerm = "ABS", EnglishTranslation = "ABS", Category = "Grade/Spec" },
        new() { JapaneseTerm = "ETC", EnglishTranslation = "ETC", Category = "Grade/Spec" },
        new() { JapaneseTerm = "アルミホイール", EnglishTranslation = "Alloy Wheels", Category = "Grade/Spec" },
        new() { JapaneseTerm = "フォグランプ", EnglishTranslation = "Fog Lamps", Category = "Grade/Spec" },
        new() { JapaneseTerm = "LEDヘッドライト", EnglishTranslation = "LED Headlights", Category = "Grade/Spec" },
        new() { JapaneseTerm = "両側パワースライド", EnglishTranslation = "Dual Power Sliding Doors", Category = "Grade/Spec" },
        new() { JapaneseTerm = "パワースライドドア", EnglishTranslation = "Power Sliding Door", Category = "Grade/Spec" },
        new() { JapaneseTerm = "年式", EnglishTranslation = "Model Year", Category = "Grade/Spec" },
        new() { JapaneseTerm = "型式", EnglishTranslation = "Model Code", Category = "Grade/Spec" },
        new() { JapaneseTerm = "車台番号", EnglishTranslation = "Chassis Number", Category = "Grade/Spec" },
        new() { JapaneseTerm = "排気量", EnglishTranslation = "Engine Displacement", Category = "Grade/Spec" },
        new() { JapaneseTerm = "グレード", EnglishTranslation = "Grade", Category = "Grade/Spec" },
        new() { JapaneseTerm = "評価点", EnglishTranslation = "Auction Grade", Category = "Grade/Spec" },
    };
}
