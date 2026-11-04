namespace EatUp.Api.Domain;

/// <summary>What a product database knows about a barcode. Category tags look like "en:semi-skimmed-milks".</summary>
public record ScannedProduct(string? Name, string[] CategoryTags, string? Quantity);

/// <param name="Name">The catalogue name in the user's language when recognised, so the form suggests a shelf life; otherwise the name on the package.</param>
public record RecognisedProduct(string? Name, string? ProductName, Ingredient? Ingredient, PackageSize? Size);

public static class ProductRecognition
{
    public static RecognisedProduct Recognise(ScannedProduct product, Catalog catalog, Language language)
    {
        var productName = string.IsNullOrWhiteSpace(product.Name) ? null : product.Name.Trim();
        var ingredient = MatchCategories(product.CategoryTags, catalog) ?? (productName is null ? null : catalog.Match(productName));
        var name = ingredient is null ? productName : catalog.NameOf(ingredient, language);
        return new(name, productName, ingredient, PackageSize.Parse(product.Quantity));
    }

    // Exact matches only, most specific tag first: loose word matching would turn "plant milks" into milk
    // or "chicken eggs" into chicken, while the generic parent tag ("en:milks", "en:eggs") is always listed too.
    private static Ingredient? MatchCategories(IEnumerable<string> tags, Catalog catalog) =>
        tags.Reverse()
            .SelectMany(tag => SingularForms(TagToPhrase(tag)))
            .Select(catalog.FindByName)
            .FirstOrDefault(i => i is not null);

    private static string TagToPhrase(string tag) => tag[(tag.IndexOf(':') + 1)..].Replace('-', ' ');

    private static IEnumerable<string> SingularForms(string phrase)
    {
        yield return phrase;
        if (phrase.EndsWith('s')) yield return phrase[..^1];
        if (phrase.EndsWith("es")) yield return phrase[..^2];
    }
}
