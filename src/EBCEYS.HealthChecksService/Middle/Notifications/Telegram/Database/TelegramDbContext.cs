using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;

public class TelegramDbContext(DbContextOptions<TelegramDbContext> options) : DbContext(options)
{
    public DbSet<TgBotSubscriber> Subscribers => Set<TgBotSubscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TgBotSubscriber>(ent =>
        {
            ent.HasKey(x => x.Id);
            ent.Property(x => x.Id).ValueGeneratedOnAdd();
            ent.HasIndex(x => x.ChatId).IsUnique();
            ent.HasIndex(x => x.Subscriber).IsUnique();
        });
    }
}

[Table("tg_bot_subscriptions")]
public class TgBotSubscriber
{
    [Key] public ulong Id { get; init; }

    [Column("subscriber")]
    [MaxLength(255)]
    public required string Subscriber { get; init; }

    [Column("chat_id")] public long ChatId { get; init; }

    [Column("is_active")] public bool IsActive { get; set; }

    [Column("created_at_utc")] public DateTimeOffset CreatedAtUtc { get; init; }
}