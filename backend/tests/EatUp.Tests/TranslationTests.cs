using EatUp.Api.Domain;

namespace EatUp.Tests;

public class TranslationTests
{
    private static readonly Catalog Real = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));
    private static readonly Language[] Translated = [Language.Norwegian, Language.Finnish];

    public static TheoryData<Language> TranslatedLanguages() => new(Translated);

    [Theory]
    [MemberData(nameof(TranslatedLanguages))]
    public void Every_ingredient_and_recipe_is_translated(Language language)
    {
        var untranslatedIngredients = Real.Ingredients.Where(i => Real.NameOf(i, language) == i.Name && !IsSameWord(i.Name)).Select(i => i.Key);
        var untranslatedRecipes = Real.Recipes.Where(r => Real.TextOf(r, language).Steps == r.Steps).Select(r => r.Key);

        Assert.Empty(untranslatedIngredients);
        Assert.Empty(untranslatedRecipes);
    }

    // Names that are spelled the same in English, Norwegian and Finnish.
    private static bool IsSameWord(string name) => name is "Pasta" or "Tofu";

    [Theory]
    [InlineData("Tine Lettmelk", "milk")]
    [InlineData("Valio kevytmaito 1 l", "milk")]
    [InlineData("Kokosmelk", "coconut-milk")]
    [InlineData("Kookosmaito", "coconut-milk")]
    [InlineData("Rødløk", "onion")]
    [InlineData("Hvitløk", "garlic")]
    [InlineData("Valkosipuli", "garlic")]
    [InlineData("Prior kyllingfilet", "chicken")]
    [InlineData("Kirsikkatomaatit", "tomato")]
    [InlineData("Hakkede tomater", "canned-tomatoes")]
    [InlineData("Peanøttsmør", "peanut-butter")]
    [InlineData("Maapähkinävoi", "peanut-butter")]
    [InlineData("Brunost", "cheese")]
    [InlineData("Luomu kananmunat", "egg")]
    public void Nordic_product_names_match_by_word_ending(string name, string expected) =>
        Assert.Equal(expected, Real.Match(name)?.Key);

    [Theory]
    [InlineData("Goats cheese", "cheese")] // English words don't match by ending, so this isn't oats
    [InlineData("Caramel sauce", null)]
    public void English_words_still_need_whole_word_matches(string name, string? expected) =>
        Assert.Equal(expected, Real.Match(name)?.Key);

    [Fact]
    public void Search_finds_any_language_and_names_results_in_the_users_language()
    {
        Assert.Equal("Melk", Real.Search("milk", Language.Norwegian).First(i => i.Key == "milk").Name);
        Assert.Equal("Milk", Real.Search("maito", Language.English).First(i => i.Key == "milk").Name);
    }

    [Fact]
    public void Untranslated_text_falls_back_to_english()
    {
        var catalog = new Catalog([TestCatalog.Ing("egg")], [TestCatalog.Rec("omelette", "egg")]);

        Assert.Equal("egg", catalog.NameOf(catalog.Find("egg")!, Language.Finnish));
        Assert.Equal("omelette", catalog.TextOf(catalog.Recipes[0], Language.Finnish).Title);
    }

    [Fact]
    public void Translations_must_use_known_keys()
    {
        var typo = new CatalogTranslation(new() { ["eggs"] = new IngredientText("Egg", []) }, []);

        Assert.Throws<InvalidOperationException>(() =>
            new Catalog([TestCatalog.Ing("egg")], [], new Dictionary<Language, CatalogTranslation> { [Language.Norwegian] = typo }));
    }

    [Theory]
    [InlineData(Language.English, "Milk expires tomorrow (+1 more)", "Cook idea: Pancakes")]
    [InlineData(Language.Norwegian, "Melk går ut på dato i morgen (+1 til)", "Matidé: Pancakes")]
    [InlineData(Language.Finnish, "Maito: käytä viimeistään huomenna (+1 muuta)", "Ruokaidea: Pancakes")]
    public void Daily_notification_is_written_in_the_users_language(Language language, string title, string body)
    {
        var profile = new UserProfile { TimeZone = "Europe/Oslo", Language = language };
        PantryItem[] items = [new(1, title.Split(' ', ':')[0], "milk", 1), new(2, "Eggs", "egg", 2)];

        var message = DailyNotification.Decide(profile, items, "Pancakes", DateTimeOffset.UtcNow, force: true)!;

        Assert.Equal(title, message.Title);
        Assert.Equal(body, message.Body);
    }
}
