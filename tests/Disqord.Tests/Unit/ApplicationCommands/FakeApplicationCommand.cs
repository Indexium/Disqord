using System.Collections.Generic;
using System.Globalization;
using Disqord.Models;
using Qommon.Collections.ReadOnly;

namespace Disqord.Tests.Unit.ApplicationCommands;

internal sealed class FakeApplicationCommand : IApplicationCommand
{
    public Snowflake Id { get; }

    public Snowflake? GuildId => null;

    public ApplicationCommandType Type { get; }

    public Snowflake ApplicationId => default;

    public string Name { get; }

    public IReadOnlyDictionary<CultureInfo, string> NameLocalizations => ReadOnlyDictionary<CultureInfo, string>.Empty;

    public Permissions? DefaultRequiredMemberPermissions => null;

    public bool IsEnabledInPrivateChannels => true;

    public bool IsEnabledByDefault => true;

    public bool IsAgeRestricted => false;

    public Snowflake Version => default;

    public IClient Client => null!;

    public FakeApplicationCommand(Snowflake id, string name, ApplicationCommandType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public void Update(ApplicationCommandJsonModel model)
    { }
}
