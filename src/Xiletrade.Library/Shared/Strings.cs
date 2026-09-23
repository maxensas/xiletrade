namespace Xiletrade.Library.Shared;

/// <summary> Partial static class containing ALL and ONLY global constants strings.<br/>
/// Subclass are located under 'Strings' directory.</summary>
/// <remarks> Does not respect intentionally naming conventions for global constants.</remarks>
public static partial class Strings
{
    internal static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    
    /// <summary>Carriage Return + Line Feed</summary>
    internal const string CRLF = "\r\n";
    /// <summary>Line Feed</summary>
    internal const string LF = "\n";
    /// <summary>Delimiter used for POE item info descriptions.</summary>
    internal const string ItemInfoDelimiter = "--------";
    /// <summary> Delimiter used for POE item info descriptions + Carriage Return + Line Feed</summary>
    internal const string ItemInfoDelimiterCRLF = "--------\r\n";
    internal const string PoeClass = "POEWindowClass";
    internal const string Info = " [Xiletrade POE Helper]";
    internal const string Blight = "Blight";
    internal const string Ravaged = "Ravaged";
    internal const string Maps = "Maps";
    internal const string UnscalableValue = "Unscalable Value";
    internal const string ChaosOrb = "Chaos Orb";
    internal const string NullClass = "NullClass";

    // initialized with data service
    private static bool IsPoe2 { get; set; }
    private static int Gateway { get; set; }

    internal static void InitializeGameContext(bool isPoe2, int gateway)
    {
        IsPoe2 = isPoe2;
        Gateway = gateway;
    }

    internal static string PoeCaption { get => IsPoe2 ? "Path of Exile 2" : "Path of Exile"; }

    /// <summary>
    /// Get Poe1 or Poe2 BULK category.
    /// </summary>
    /// <param name="curClass"></param>
    /// <param name="curId"></param>
    /// <returns></returns>
    internal static string GetBulkCategory(string curClass, string curId) 
        => IsPoe2 ? CurrencyTypePoe2.GetCategory(curClass, curId) 
        : CurrencyTypePoe1.GetCategory(curClass, curId);
}