namespace NameBadger.Bot.Contexts
{
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using NameBadger.Bot.Models;

    public class NameBadgeContext : DbContext
    {
        internal DbSet<NameBadge> NameBadges { get; [UsedImplicitly] set; }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bot.db");
        }
    }
}