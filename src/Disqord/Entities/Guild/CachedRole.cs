using System.Collections.Generic;
using Disqord.Collections;
using Disqord.Models;

namespace Disqord
{
    public sealed partial class CachedRole : CachedSnowflakeEntity, IRole
    {
        public string Name { get; private set; }

        public Color? Color { get; private set; }

        public bool IsHoisted { get; private set; }

        public int Position { get; private set; }

        public GuildPermissions Permissions { get; private set; }

        public bool IsManaged { get; private set; }

        public bool IsMentionable { get; private set; }

        public string Mention => Discord.MentionRole(this);

        public CachedGuild Guild { get; }

        public IReadOnlyDictionary<Snowflake, CachedMember> Members { get; }

        Snowflake IRole.GuildId => Guild.OwnerId;

        internal CachedRole(DiscordClient client, RoleModel model, CachedGuild guild) : base(client, model.Id)
        {
            Guild = guild;
            Members = new ReadOnlyValuePredicateDictionary<Snowflake, CachedMember>(guild.Members, x => x.Roles.ContainsKey(Id));
            Update(model);
        }

        internal void Update(RoleModel model)
        {
            Name = model.Name;
            Color = model.Color != 0
                ? (int?) model.Color
                : null;
            IsHoisted = model.Hoist;
            Position = model.Position;
            Permissions = model.Permissions;
            IsManaged = model.Managed;
            IsMentionable = model.Mentionable;
        }

        internal CachedRole Clone()
            => (CachedRole) MemberwiseClone();

        public override string ToString()
            => Name;
    }
}