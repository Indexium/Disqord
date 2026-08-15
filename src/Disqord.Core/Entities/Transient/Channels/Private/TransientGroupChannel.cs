using System.Collections.Generic;
using Disqord.Models;
using Qommon;
using Qommon.Collections.ReadOnly;

namespace Disqord;

/// <inheritdoc cref="IGroupChannel"/>
public class TransientGroupChannel(IClient client, ChannelJsonModel model)
    : TransientPrivateChannel(client, model), IGroupChannel
{
    /// <inheritdoc/>
    public string? IconHash => Model.Icon.GetValueOrDefault();

    /// <inheritdoc/>
    public Snowflake OwnerId => Model.OwnerId.Value;

    /// <inheritdoc/>
    public IReadOnlyDictionary<Snowflake, IUser> Recipients => _recipients ??= Model.Recipients.Value.ToReadOnlyDictionary(Client,
        (model, _) => model.Id, (model, client) => new TransientUser(client, model) as IUser);

    private IReadOnlyDictionary<Snowflake, IUser>? _recipients;

    /// <inheritdoc/>
    public Snowflake? ApplicationId => Model.ApplicationId.GetValueOrDefault();

    /// <inheritdoc/>
    public bool IsManaged => Model.Managed.GetValueOrDefault();
}
