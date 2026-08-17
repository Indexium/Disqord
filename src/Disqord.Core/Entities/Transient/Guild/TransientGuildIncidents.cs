using System;
using Disqord.Models;
using Qommon;

namespace Disqord;

public class TransientGuildIncidents(IncidentsDataJsonModel model)
    : TransientEntity<IncidentsDataJsonModel>(model), IGuildIncidents
{
    /// <inheritdoc/>
    public DateTimeOffset? InvitesDisabledUntil => Model.InvitesDisabledUntil;

    /// <inheritdoc/>
    public DateTimeOffset? DirectMessagesDisabledUntil => Model.DmsDisabledUntil;

    /// <inheritdoc/>
    public DateTimeOffset? DirectMessageSpamDetectedAt => Model.DmSpamDetectedAt.GetValueOrDefault();

    /// <inheritdoc/>
    public DateTimeOffset? RaidDetectedAt => Model.RaidDetectedAt.GetValueOrDefault();
}
