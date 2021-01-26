namespace NameBadger.Bot.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using JetBrains.Annotations;
    using NameBadger.Bot.Contexts;
    using NameBadger.Bot.Models;
    using ProfanityFilter;

    public sealed class NameBadgeService
    {
        private static DiscordClient _client;
        private static Timer         _timer;

        public NameBadgeService([NotNull] DiscordClient client)
        {
            _client              =  client;
            client.GuildMemberRemoved += ClientOnGuildMemberRemoved;
            client.MessageCreated += ClientOnMessageCreated;

            var timeNow    = DateTime.UtcNow;
            var timeTarget = timeNow.AddDays(1).Date;
            _timer         =  new Timer((timeTarget - timeNow).TotalMilliseconds);
            _timer.Elapsed += DailyTimerOnElapsed;
            _timer.Enabled =  true;
        }

        private static async void DailyTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Yield();
            var db      = new NameBadgeContext();
            var timeNow = DateTime.UtcNow;
            foreach (var badge in db.NameBadges.Where(badge => (timeNow - badge.LastInteraction).Days > 2))
                await (await _client.GetGuildAsync(badge.GuildId)).GetRole(badge.RoleId).ModifyAsync(hoist: false);

            var timeTarget = timeNow.AddDays(1).Date;
            _timer         =  new Timer((timeTarget - timeNow).Milliseconds);
            _timer.Elapsed += DailyTimerOnElapsed;
            _timer.Enabled =  true;
        }

        private static async Task ClientOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            await Task.Yield();
            var db = new NameBadgeContext();

            if (!db.NameBadges.Any(x => x.UserId == e.Author.Id && x.GuildId == e.Guild.Id)) return;

            var badge = db.NameBadges.Single(x => x.UserId == e.Author.Id && x.GuildId == e.Guild.Id);

            if (!badge.IsHoisted)
            {
                await e.Guild.GetRole(badge.RoleId).ModifyAsync(hoist: true);
                badge.IsHoisted = true;
            }

            badge.LastInteraction = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

        private static async Task ClientOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            await Task.Yield();
            var db = new NameBadgeContext();
            if (!db.NameBadges.Any(x => x.UserId == e.Member.Id && x.GuildId == e.Guild.Id)) return;

            var badgesToClear = db.NameBadges.Where(x => x.GuildId == e.Guild.Id && x.UserId == e.Member.Id).ToList();
            foreach (var badge in badgesToClear.Where(badge => e.Guild.Roles.Any(x => x.Key == badge.RoleId)))
            {
                await e.Guild.GetRole(badge.RoleId).DeleteAsync();
                db.NameBadges.Remove(badge);
            }

            await db.SaveChangesAsync();
        }

        internal static async Task SetNameBadge([NotNull] CommandContext ctx, string roleName, DiscordColor color = default, bool admin = false, [CanBeNull] DiscordUser userOverride = null)
        {
            var filter = new ProfanityFilter();
            if (!admin && filter.IsProfanity(roleName))
            {
                await ctx.RespondAsync("Uh oh, you used a naughty word!\nIf you disagree then please go and contact an admin :3");
                return;
            }

            var         db         = new NameBadgeContext();
            var         targetUser = userOverride ?? ctx.User;
            NameBadge   userBadge  = null;
            DiscordRole userRole   = null;
            if (db.NameBadges.Any(x => x.UserId == targetUser.Id && x.GuildId == ctx.Guild.Id))
            {
                userBadge = db.NameBadges.Single(x => x.UserId == targetUser.Id && x.GuildId == ctx.Guild.Id);
                userRole = ctx.Guild.Roles.Any(x => x.Key == userBadge.RoleId)
                    ? ctx.Guild.Roles.Single(x => x.Key   == userBadge.RoleId).Value
                    : null;
            }

            userRole ??= await ctx.Guild.CreateRoleAsync();

            await userRole.ModifyAsync(roleName, color: color, hoist: true);

            await (await ctx.Guild.GetMemberAsync(targetUser.Id)).GrantRoleAsync(userRole);

            if (userBadge == null)
            {
                userBadge = new NameBadge
                {
                    GuildId   = ctx.Guild.Id,
                    RoleId    = userRole.Id,
                    UserId    = targetUser.Id,
                    RoleColor = color.ToString(),
                    RoleName  = roleName,
                    IsHoisted = true,
                    LastInteraction = DateTime.UtcNow
                };

                await db.NameBadges.AddAsync(userBadge);
            }
            else
            {
                userBadge.RoleName = roleName;
            }

            await db.SaveChangesAsync();

            if (userOverride != null)
                await ctx.RespondAsync($"{userOverride.Mention}! I now name you, {userRole.Mention}. Go forth and spread the badgers word!");
            else
                await ctx.RespondAsync($"I now name thee, {userRole.Mention}. Go forth and spread the badgers word!");
        }

        internal static async Task AddNameBadge(ulong guildId, ulong roleId, ulong userId, string color, string roleName)
        {
            var db = new NameBadgeContext();
            await db.NameBadges.AddAsync(new NameBadge
            {
                GuildId = guildId,
                RoleId = roleId,
                UserId = userId,
                RoleColor = color,
                RoleName = roleName,
                IsHoisted = true,
                LastInteraction = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}