namespace EatUp.Api.Domain;

/// <summary>Wording of the daily notification in each language.</summary>
public record NotificationText(
    Func<string, int, string> Expires,
    Func<int, string> AndMore,
    string OpenApp,
    Func<string, string> CookIdea)
{
    public static NotificationText For(Language language) => language switch
    {
        Language.Norwegian => Norwegian,
        Language.Finnish => Finnish,
        _ => English,
    };

    private static readonly NotificationText English = new(
        (name, days) => $"{name} {(IsEnglishPlural(name) ? "expire" : "expires")} {days switch { 0 => "today", 1 => "tomorrow", _ => $"in {days} days" }}",
        more => $" (+{more} more)",
        "Open EatUp to see what you can make.",
        recipe => $"Cook idea: {recipe}");

    private static readonly NotificationText Norwegian = new(
        (name, days) => $"{name} går ut på dato {days switch { 0 => "i dag", 1 => "i morgen", _ => $"om {days} dager" }}",
        more => $" (+{more} til)",
        "Åpne EatUp og se hva du kan lage.",
        recipe => $"Matidé: {recipe}");

    // "Käytä viimeistään" (use by) avoids singular/plural verb agreement with the item name.
    private static readonly NotificationText Finnish = new(
        (name, days) => $"{name}: käytä viimeistään {days switch { 0 => "tänään", 1 => "huomenna", _ => $"{days} päivän kuluessa" }}",
        more => $" (+{more} muuta)",
        "Avaa EatUp ja katso, mitä voit tehdä.",
        recipe => $"Ruokaidea: {recipe}");

    // Good enough for food names: "Eggs" expire, "Milk" / "Hummus" / "Cress" expires.
    private static bool IsEnglishPlural(string name)
    {
        var n = name.Trim().ToLowerInvariant();
        return n.EndsWith('s') && !n.EndsWith("ss") && !n.EndsWith("us");
    }
}
