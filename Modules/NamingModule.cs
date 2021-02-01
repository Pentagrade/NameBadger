namespace NameBadger.Bot.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity.Extensions;
    using JetBrains.Annotations;
    using NameBadger.Bot.Contexts;
    using NameBadger.Bot.Services;

    [Group("name")]
    [UsedImplicitly]
    public class NamingModule : BaseCommandModule
    {
        [Command]
        [Priority(1)]
        [UsedImplicitly]
        public async Task Set([NotNull] CommandContext ctx, [RemainingText] string roleName)
        {
            await NameBadgeService.SetNameBadge(ctx, roleName);
        }

        [Command]
        [Priority(2)]
        [UsedImplicitly]
        public async Task Set([NotNull] CommandContext ctx, string color, [RemainingText] string roleName)
        {
            DiscordColor discordColor;
            try
            {
                discordColor = new DiscordColor(color);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(
                    $"Uh oh, there might be a problem with the colour you have chosen\n`{ex.Message}`");
                return;
            }

            await NameBadgeService.SetNameBadge(ctx, roleName, discordColor);
        }

        [Command]
        [RequirePermissions(Permissions.ManageRoles)]
        [Priority(1)]
        [UsedImplicitly]
        public async Task AdminSet([NotNull] CommandContext ctx, DiscordUser user, [RemainingText] string roleName)
        {
            await NameBadgeService.SetNameBadge(ctx, roleName, admin: true, userOverride: user);
        }

        [Command]
        [RequirePermissions(Permissions.ManageRoles)]
        [Priority(2)]
        [UsedImplicitly]
        public async Task AdminSet([NotNull] CommandContext ctx, DiscordUser user, string color, [RemainingText] string roleName)
        {
            DiscordColor discordColor;
            try
            {
                discordColor = new DiscordColor(color);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(
                    $"Uh oh, there might be a problem with the colour you have chosen\n`{ex.Message}`");
                return;
            }

            await NameBadgeService.SetNameBadge(ctx, roleName, discordColor, true, user);
        }

        [Command]
        [RequirePermissions(Permissions.ManageRoles)]
        [UsedImplicitly]
        public async Task Banish([NotNull] CommandContext ctx, DiscordUser user)
        {
            var db = new NameBadgeContext();

            if (!db.NameBadges.Any(x => x.GuildId == ctx.Guild.Id && x.UserId == user.Id))
            {
                await ctx.RespondAsync("I have not bestowed a name upon this ruffian!");
                return;
            }

            var userBadge = db.NameBadges.Single(x => x.GuildId == ctx.Guild.Id && x.UserId == user.Id);
            var roleToClear = ctx.Guild.Roles.Any(x => x.Key == userBadge.RoleId)
                ? ctx.Guild.Roles.Single(x => x.Key == userBadge.RoleId).Value
                : null;

            if (roleToClear is { }) await roleToClear.DeleteAsync();
            db.NameBadges.Remove(userBadge);
            await db.SaveChangesAsync();

            await ctx.RespondAsync("This name has been banished to the nether realms of my burrow, good day to you!");
        }

        [Command]
        [RequirePermissions(Permissions.ManageRoles)]
        [UsedImplicitly]
        public async Task List([NotNull] CommandContext ctx)
        {
            var db = new NameBadgeContext();

            var listBuilder = string.Empty;
            foreach (var badge in db.NameBadges.Select(x => x))
            {
                var user = await ctx.Client.GetUserAsync(badge.UserId);
                var role = ctx.Guild.GetRole(badge.RoleId);
                listBuilder += $"{user.Mention} - {role.Mention}";
            }

            if (listBuilder == string.Empty) return;

            var interactivity = ctx.Client.GetInteractivity();
            var pages         = interactivity.GeneratePagesInEmbed(listBuilder);

            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }

        [Command]
        [RequirePermissions(Permissions.ManageRoles)]
        [UsedImplicitly]
        public async Task Link([NotNull] CommandContext ctx, [NotNull] DiscordRole targetRole)
        {
            var counter = 0;
            try
            {
                DiscordMember targetMember = null;
                foreach (var (_, value) in ctx.Guild.Members)
                {
                    if (value.Roles.Contains(targetRole))
                    {
                        counter++;
                        targetMember = value;
                    }

                    if (counter == 2) throw new Exception("Bad Count");
                }

                if (counter == 0 || targetMember == null) throw new Exception("Bad Count");

                await NameBadgeService.AddNameBadge(ctx.Guild.Id, targetRole.Id, targetMember.Id, targetRole.Color.ToString(), targetRole.Name);

                await ctx.RespondAsync("I now recognise this name, for I am... the Name Badger!");
            }
            catch (Exception ex)
            {
                if (ex.Message != "Bad Count") throw;

                var reason = counter == 0 ? "no one has this name!" : "this name is popular!";

                await ctx.RespondAsync(
                    $"My badger nose has detected that {reason} I only like making 1 badge, and that badger better be used!");
            }
        }

        [Command]
        [RequireOwner]
        [UsedImplicitly]
        public async Task Clean()
        {
            await NameBadgeService.CleanBadgeRecords();
        }
    }
}