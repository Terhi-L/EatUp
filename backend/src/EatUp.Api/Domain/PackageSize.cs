using System.Globalization;
using System.Text.RegularExpressions;

namespace EatUp.Api.Domain;

public record PackageSize(double Quantity, string Unit)
{
    private static readonly Dictionary<string, string> KnownUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = "g", ["kg"] = "kg", ["ml"] = "ml", ["cl"] = "cl", ["dl"] = "dl", ["l"] = "l",
        ["pcs"] = "pcs", ["pieces"] = "pcs", ["stk"] = "pcs", ["st"] = "pcs", ["kpl"] = "pcs",
    };

    // Product data is user-edited, so an absurd pack count is ignored rather than trusted.
    private static readonly Regex Multipack = new(@"^(\d{1,4})\s*[x×]", RegexOptions.IgnoreCase);
    private static readonly Regex Amount = new(@"^(\d+(?:[.,]\d+)?)\s*([a-z]+)\b", RegexOptions.IgnoreCase);

    /// <summary>Reads a package quantity such as "1 l", "500 g" or "6 x 125 g" (counted as 6 pieces).</summary>
    public static PackageSize? Parse(string? text)
    {
        var trimmed = text?.Trim() ?? "";
        if (Multipack.Match(trimmed) is { Success: true } pack) return new(int.Parse(pack.Groups[1].Value), "pcs");

        var amount = Amount.Match(trimmed);
        if (!amount.Success || !KnownUnits.TryGetValue(amount.Groups[2].Value, out var unit)) return null;
        return new(double.Parse(amount.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture), unit);
    }
}
