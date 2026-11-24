using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EatUp.Api.Data;

public class EatUpDb(DbContextOptions<EatUpDb> options) : DbContext(options)
{
    public DbSet<FoodItem> Items => Set<FoodItem>();
    public DbSet<UserProfile> Profiles => Set<UserProfile>();
    public DbSet<PushSubscriptionRecord> PushSubscriptions => Set<PushSubscriptionRecord>();
    public DbSet<ShoppingItem> Shopping => Set<ShoppingItem>();
    public DbSet<ConsumeEvent> ConsumeEvents => Set<ConsumeEvent>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<PlannedMeal> PlannedMeals => Set<PlannedMeal>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<FoodItem>(e =>
        {
            e.Property(i => i.Location).HasConversion<string>();
            e.Property(i => i.Status).HasConversion<string>();
            e.HasIndex(i => new { i.Status, i.ExpiryDate });
            // A consume that loaded the item before another one saved must not count the same food twice.
            e.Property(i => i.Status).IsConcurrencyToken();
            e.Property(i => i.Quantity).IsConcurrencyToken();
        });

        model.Entity<UserProfile>(e =>
        {
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.Diet).HasConversion<string>();
            e.Property(p => p.Allergens).HasConversion(
                list => string.Join(',', list),
                text => text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<Allergen>).ToList(),
                new ValueComparer<List<Allergen>>((a, b) => a!.SequenceEqual(b!), l => l.Aggregate(0, (h, x) => HashCode.Combine(h, x)), l => l.ToList()));
        });

        model.Entity<ConsumeEvent>(e =>
        {
            e.Property(c => c.Outcome).HasConversion<string>();
            // No foreign key: history outlives the item and keeps its id (item ids are never reused), so the
            // waste stats still see one item behind several partial uses after it is deleted.
            e.HasIndex(c => c.FoodItemId);
            e.HasIndex(c => c.ConsumedAt);
        });

        model.Entity<MealPlan>(e =>
        {
            e.HasIndex(p => p.WeekStart).IsUnique();
            e.HasMany(p => p.Meals).WithOne().HasForeignKey(m => m.MealPlanId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<PlannedMeal>(e =>
        {
            e.Property(m => m.Slot).HasConversion<string>();
            e.Property(m => m.FreeText).HasMaxLength(MealPlanning.MaxFreeTextLength);
            // Backs up the API validation, so a planned meal can never be both or neither.
            e.ToTable(t => t.HasCheckConstraint("CK_PlannedMeals_RecipeOrFreeText", "(\"RecipeKey\" IS NULL) <> (\"FreeText\" IS NULL)"));
        });

        model.Entity<PushSubscriptionRecord>().HasIndex(s => s.Endpoint).IsUnique();
    }
}
