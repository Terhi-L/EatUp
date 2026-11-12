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

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<FoodItem>(e =>
        {
            e.Property(i => i.Location).HasConversion<string>();
            e.Property(i => i.Status).HasConversion<string>();
            e.HasIndex(i => new { i.Status, i.ExpiryDate });
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
            e.HasOne<FoodItem>().WithMany().HasForeignKey(c => c.FoodItemId).OnDelete(DeleteBehavior.SetNull);
        });

        model.Entity<PushSubscriptionRecord>().HasIndex(s => s.Endpoint).IsUnique();
    }
}
