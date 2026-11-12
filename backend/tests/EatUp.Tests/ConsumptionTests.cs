using EatUp.Api.Domain;

namespace EatUp.Tests;

public class ConsumptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 18, 30, 0, TimeSpan.Zero);

    private static FoodItem Eggs(double quantity = 6) => new()
    {
        Id = 7, Name = "Eggs", IngredientKey = "egg", Quantity = quantity, Unit = "pcs",
    };

    [Fact]
    public void Partial_consume_lowers_the_quantity_keeps_the_item_active_and_records_the_part()
    {
        var eggs = Eggs();

        var consumed = Consumption.Consume(eggs, ConsumeOutcome.Discarded, 2, Now);

        Assert.Equal(4, eggs.Quantity);
        Assert.Equal(ItemStatus.Active, eggs.Status);
        Assert.Null(eggs.StatusChangedAt);
        Assert.Equal(new ConsumeEvent
        {
            FoodItemId = 7, ItemName = "Eggs", IngredientKey = "egg", Amount = 2, Unit = "pcs",
            Outcome = ConsumeOutcome.Discarded, ConsumedAt = Now,
        }, consumed, ConsumeEventComparer.Instance);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(6.0)]
    [InlineData(10.0)]
    public void Full_or_overshooting_consume_records_the_remaining_quantity(double? amount)
    {
        var eggs = Eggs();

        var consumed = Consumption.Consume(eggs, ConsumeOutcome.Used, amount, Now);

        Assert.Equal(ItemStatus.Used, eggs.Status);
        Assert.Equal(Now, eggs.StatusChangedAt);
        Assert.Equal(6, consumed.Amount);
        Assert.Equal(ConsumeOutcome.Used, consumed.Outcome);
    }

    [Fact]
    public void Full_discard_marks_the_item_discarded()
    {
        var eggs = Eggs();

        Consumption.Consume(eggs, ConsumeOutcome.Discarded, null, Now);

        Assert.Equal(ItemStatus.Discarded, eggs.Status);
    }

    [Theory]
    [InlineData(ItemStatus.Used)]
    [InlineData(ItemStatus.Discarded)]
    public void Only_active_items_can_be_consumed(ItemStatus status)
    {
        Assert.False(Consumption.CanConsume(new FoodItem { Name = "Eggs", Status = status }));
        Assert.Throws<InvalidOperationException>(() =>
            Consumption.Consume(new FoodItem { Name = "Eggs", Status = status }, ConsumeOutcome.Used, null, Now));
    }

    private sealed class ConsumeEventComparer : IEqualityComparer<ConsumeEvent>
    {
        public static readonly ConsumeEventComparer Instance = new();

        public bool Equals(ConsumeEvent? a, ConsumeEvent? b) =>
            a is not null && b is not null &&
            (a.FoodItemId, a.ItemName, a.IngredientKey, a.Amount, a.Unit, a.Outcome, a.ConsumedAt) ==
            (b.FoodItemId, b.ItemName, b.IngredientKey, b.Amount, b.Unit, b.Outcome, b.ConsumedAt);

        public int GetHashCode(ConsumeEvent e) => e.ItemName.GetHashCode();
    }
}
