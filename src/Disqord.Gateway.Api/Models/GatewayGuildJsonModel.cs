using System;
using System.Collections.Generic;
using Disqord.Models;
using Disqord.Serialization.Json;
using Qommon;

namespace Disqord.Gateway.Api.Models;

[JsonSkippedProperties("lazy")]
public class GatewayGuildJsonModel : GuildJsonModel
{
    [JsonProperty("joined_at")]
    public DateTimeOffset JoinedAt;

    [JsonProperty("large")]
    public bool Large;

    [JsonProperty("unavailable")]
    public Optional<bool> Unavailable;

    [JsonProperty("member_count")]
    public int MemberCount;

    [JsonProperty("voice_states")]
    public IJsonArray VoiceStates = null!;

    [JsonProperty("members")]
    public IJsonArray Members = null!;

    [JsonProperty("channels")]
    public IJsonArray Channels = null!;

    [JsonProperty("threads")]
    public IJsonArray Threads = null!;

    [JsonProperty("presences")]
    public IJsonArray Presences = null!;

    [JsonProperty("stage_instances")]
    public IJsonArray StageInstances = null!;

    [JsonProperty("guild_scheduled_events")]
    public IJsonArray GuildScheduledEvents = null!;
}
