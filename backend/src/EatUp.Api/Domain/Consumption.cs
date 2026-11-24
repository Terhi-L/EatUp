namespace EatUp.Api.Domain;

public static class Consumption
{
    public static bool CanConsume(FoodItem item) => item.Status == ItemStatus.Active;

    /// <summary>A partial amount lowers the quantity and keeps the item active; anything else consumes what is left.</summary>
    public static ConsumeEvent Consume(FoodItem item, ConsumeOutcome outcome, double? amount, DateTimeOffset now)
    {
        if (!CanConsume(item)) throw new InvalidOperationException($"Item {item.Id} is already {item.Status}.");

        var consumed = RecordOf(item, outcome, amount is { } part && part < item.Quantity ? part : item.Quantity, now);
        if (consumed.Amount < item.Quantity)
        {
            item.Quantity -= consumed.Amount;
            return consumed;
        }

        item.Status = outcome == ConsumeOutcome.Used ? ItemStatus.Used : ItemStatus.Discarded;
        item.StatusChangedAt = now;
        return consumed;
    }

    private static ConsumeEvent RecordOf(FoodItem item, ConsumeOutcome outcome, double amount, DateTimeOffset now) => new()
    {
        FoodItemId = item.Id, ItemName = item.Name, IngredientKey = item.IngredientKey,
        Amount = amount, Unit = item.Unit, Outcome = outcome, ConsumedAt = now,
    };
}
