using EatUp.Api.Domain;
using static EatUp.Tests.TestCatalog;

namespace EatUp.Tests;

public class ProductRecognitionTests
{
    private static readonly Catalog Catalog = new(
        [Ing("egg"), Ing("milk"), Ing("chicken"), Ing("tomato") with { Aliases = ["tomato", "tomatoes"] }, Ing("coconut milk")],
        []);

    private static RecognisedProduct Recognise(string? name, string[] tags, string? quantity = null) =>
        ProductRecognition.Recognise(new ScannedProduct(name, tags, quantity), Catalog, Language.English);

    [Fact]
    public void Generic_parent_category_wins_over_loose_word_matches()
    {
        var eggs = Recognise("Frittgående egg", ["en:farming-products", "en:eggs", "en:chicken-eggs"]);
        Assert.Equal("egg", eggs.Ingredient?.Key);
    }

    [Fact]
    public void Plural_category_tags_match_singular_catalogue_names()
    {
        Assert.Equal("milk", Recognise("Tine Lettmelk", ["en:dairies", "en:milks", "en:semi-skimmed-milks"]).Ingredient?.Key);
        Assert.Equal("coconut milk", Recognise(null, ["en:coconut-milks"]).Ingredient?.Key);
    }

    [Fact]
    public void Plant_milk_is_not_mistaken_for_milk()
    {
        Assert.Null(Recognise("Oatly Havredrikk", ["en:plant-based-foods-and-beverages", "en:plant-milks"]).Ingredient);
    }

    [Fact]
    public void Falls_back_to_the_product_name_when_no_category_matches()
    {
        var product = Recognise("Cherry tomatoes", ["en:snacks"]);
        Assert.Equal("tomato", product.Ingredient?.Key);
        Assert.Equal("tomato", product.Name); // catalogue name, so the form can suggest a shelf life
    }

    [Fact]
    public void Unknown_product_keeps_the_package_name()
    {
        var product = Recognise("  Grandiosa  ", ["en:pizzas"]);
        Assert.Null(product.Ingredient);
        Assert.Equal("Grandiosa", product.Name);
    }

    [Theory]
    [InlineData("1 l", 1, "l")]
    [InlineData("500g", 500, "g")]
    [InlineData("1,5 L", 1.5, "l")]
    [InlineData("6 x 125 g", 6, "pcs")]
    [InlineData("12 stk", 12, "pcs")]
    [InlineData("10 kpl", 10, "pcs")]
    public void Package_size_is_read_from_common_formats(string text, double quantity, string unit)
    {
        Assert.Equal(new PackageSize(quantity, unit), PackageSize.Parse(text));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("family pack")]
    [InlineData("3 bunches")]
    [InlineData("99999999999 x 1 g")]
    public void Unreadable_package_size_is_left_for_the_user(string? text)
    {
        Assert.Null(PackageSize.Parse(text));
    }
}
