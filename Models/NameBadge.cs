namespace NameBadger.Bot.Models
{
    using System;

    internal sealed class NameBadge
    {
        public int      Id              { get; set; }
        public string   RoleName        { get; set; }
        public string   RoleColor       { get; set; }
        public ulong    RoleId          { get; set; }
        public ulong    UserId          { get; set; }
        public ulong    GuildId         { get; set; }
        public bool     IsHoisted       { get; set; }
        public DateTime LastInteraction { get; set; }
    }
}