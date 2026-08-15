using System;
using Disqord.Serialization.Json;
using Qommon;

namespace Disqord.Models;

public class IncidentsDataJsonModel : JsonModel
{
    [JsonProperty("invites_disabled_until")]
    public DateTimeOffset? InvitesDisabledUntil;

    [JsonProperty("dms_disabled_until")]
    public DateTimeOffset? DmsDisabledUntil;

    [JsonProperty("dm_spam_detected_at")]
    public Optional<DateTimeOffset?> DmSpamDetectedAt;

    [JsonProperty("raid_detected_at")]
    public Optional<DateTimeOffset?> RaidDetectedAt;
}
