using System.Net;
using System.Text.Json;
using EatUp.Api.Domain;

namespace EatUp.Api.Services;

public interface IProductLookup
{
    /// <summary>The product behind a barcode, named in the given language when known, or null when the database doesn't know it.</summary>
    Task<ScannedProduct?> FindAsync(string barcode, Language language, CancellationToken cancellationToken);
}

/// <summary>Open Food Facts: free, open data, no API key.</summary>
public class OpenFoodFactsLookup(HttpClient http) : IProductLookup
{
    public static readonly Uri BaseAddress = new("https://world.openfoodfacts.org/");

    // Norwegian products are tagged both "nb" (Bokmål) and "no".
    private static readonly Dictionary<Language, string[]> NameLanguages = new()
    {
        [Language.English] = ["en"],
        [Language.Norwegian] = ["nb", "no"],
        [Language.Finnish] = ["fi"],
    };

    public async Task<ScannedProduct?> FindAsync(string barcode, Language language, CancellationToken cancellationToken)
    {
        var codes = NameLanguages.GetValueOrDefault(language, NameLanguages[Language.English]);
        var nameFields = codes.Select(code => $"product_name_{code}").Concat(["product_name", "product_name_en", "generic_name"]).ToList();
        var fields = string.Join(',', nameFields.Concat(["categories_tags", "quantity"]));

        using var response = await http.GetAsync($"api/v2/product/{barcode}.json?fields={fields}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        if (!IsFound(body.RootElement, out var product)) return null;
        return new ScannedProduct(FirstText(product, nameFields), StringArray(product, "categories_tags"), FirstText(product, ["quantity"]));
    }

    private static bool IsFound(JsonElement root, out JsonElement product)
    {
        product = default;
        return root.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.Number && status.GetInt32() == 1
            && root.TryGetProperty("product", out product);
    }

    private static string? FirstText(JsonElement product, IEnumerable<string> fields) =>
        fields
            .Select(f => product.TryGetProperty(f, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

    private static string[] StringArray(JsonElement product, string field) =>
        product.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!).ToArray()
            : [];
}
