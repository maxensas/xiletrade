namespace XiletradeJson
{
    // Optimization in order to avoid unnecessary string allocations.
    internal static class Strings
    {
        internal static readonly string BaseItemTypes = "baseitemtypes";
        internal static readonly string Mods = "mods";
        internal static readonly string MonsterVarieties = "monstervarieties";
        internal static readonly string Words = "words";
        internal static readonly string Gems = "gemeffects";

        internal static readonly string[] DatNames = { BaseItemTypes, Mods, MonsterVarieties, Words, Gems };
        internal static readonly string[] PathGgpk = {
            "C:\\Path of Exile\\Content.ggpk",
            "C:\\Jeux\\Path of Exile\\Content.ggpk",
            "C:\\Program Files\\Path of Exile\\Content.ggpk",
            "C:\\Program Files (x86)\\Path of Exile\\Content.ggpk",
            "C:\\Games\\Path of Exile\\Content.ggpk",
            "D:\\Path of Exile\\Content.ggpk",
            "D:\\Jeux\\Path of Exile\\Content.ggpk",
            "D:\\Games\\Path of Exile\\Content.ggpk"
        };

        internal static readonly KeyValuePair<string, string>[] GlobalLang = {
            new("english", "en-US"),
            new("french", "fr-FR"),
            new("german", "de-DE"),
            new("japanese", "ja-JP"),
            new("korean", "ko-KR"),
            new("portuguese", "pt-BR"),
            new("russian", "ru-RU"),
            new("spanish", "es-ES"),
            new("thai", "th-TH"),
            new("traditional chinese", "zh-TW")
        };
        internal static readonly KeyValuePair<string, string>[] TencentLang = {
            new("english", "en-US"),
            new("simplified chinese", "zh-CN")
        };
        internal static readonly KeyValuePair<int, string>[] BasesIndex = {
            new(0, "Id"),
            new(4, "Name"),
            new(5, "InheritsFrom")
        };
        internal static readonly KeyValuePair<int, string>[] ModsIndex = {
            new(0, "Id"),
            new(9, "Name")
        };
        internal static readonly KeyValuePair<int, string>[] MonstersIndex = {
            new(0, "Id"),
            new(8, "BaseMonsterTypeIndex"),
            new(32, "Name")
        };
        internal static readonly KeyValuePair<int, string>[] WordsIndex = {
            new(1, "Text"),
            new(5, "Text2")
        };
        internal static readonly KeyValuePair<int, string>[] GemsIndex = {
            new(0, "Id"),
            new(1, "Name")
        };

        internal static class Parser
        {
            internal static readonly string ModsInherits = "Mods/Mod";
            internal static readonly string NameBaseUnwanted = "[DNT]";
            internal static readonly string StackableCurrency = "Currency/StackableCurrency";
            internal static readonly string IncursionVial = "CurrencyIncursionVial";

            internal static readonly string[] IdModsUnwanted = { "Essence", "Enchantment", "WeaponEnchantment", "ArmourEnchantment",
            "BlightEnchantment", "TalismanMonster", "ArmageddonBrand", "StormBrand", "CritChanceWith"};
            internal static readonly string[] InheritsBaseUnwanted = { "Currency/AbstractMicrotransaction", "HideoutDoodads/AbstractHideoutDoodad",
                "PantheonSouls/AbstactPantheonSoul", "Delve/DelveSocketableCurrency", "Delve/DelveStackableSocketableCurrency","Maps/AbstractMap",
                "MapFragments/AbstractMapFragment", "DivinationCards/AbstractDivinationCard", "Legion/Incubator", "Item", 
                "QuestItems/AbstractQuestItem", "Labyrinth/AbstractLabyrinthItem", "AtlasUpgrades/AtlasRegionUpgrade" }; // maybe add: "Sentinel/SentinelDroneBase", "Relics/AbstractRelic"

            internal static readonly KeyValuePair<string, string> HexSpaceSring = new(" ", " "); // 0xa0 => 0x20;
            internal static readonly KeyValuePair<string, string> MetaItem = new("Metadata/Items/", string.Empty);
            internal static readonly KeyValuePair<string, string> MetaMonster = new("Metadata/Monsters/", string.Empty);
            
            internal static readonly KeyValuePair<string, string>[] NameRules = {
                new("<if:MS>{", string.Empty),
                new("}<elif:FS>{", "/"),
                new("}<elif:NS>{", "/"),
                new("}<elif:MP>{", "/"),
                new("}<elif:FP>{", "/"),
                new("}<elif:NP>{", "/"),
                new("}", string.Empty),
                new("/NONEXISTENT", string.Empty),
                new("/ NONEXISTENT", string.Empty)
            };
        }
    }
}
