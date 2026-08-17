using Disqord.Gateway.Api.Models;
using Disqord.Models;
using Disqord.Serialization.Json.Default;
using Disqord.Tests.Serialization;
using Microsoft.Extensions.Logging.Abstractions;

namespace Disqord.Tests.Unit.Gateway;

public class GatewayGuildJsonModelDeserializationTests : SerializationTestBase
{
    private const string MinimalGuildJson = """
        {
            "id": "1",
            "name": "Test Guild",
            "owner_id": "2",
            "verification_level": 0,
            "default_message_notifications": 0,
            "explicit_content_filter": 0,
            "roles": [],
            "emojis": [],
            "features": [],
            "mfa_level": 0,
            "system_channel_flags": 0,
            "premium_tier": 0,
            "preferred_locale": "en-US",
            "nsfw_level": 0,
            "premium_progress_bar_enabled": false,
            "joined_at": "2021-01-01T00:00:00Z",
            "large": false,
            "member_count": 1,
            "voice_states": [],
            "members": [],
            "channels": [],
            "threads": [],
            "presences": [],
            "stage_instances": [],
            "guild_scheduled_events": []
        }
        """;

    [Test]
    public void Deserialize_ChannelsContainMalformedEntry_DoesNotThrow()
    {
        // Arrange
        var json = MinimalGuildJson.Replace(
            """"channels": []"""",
            """"channels": [{"id":"not-a-snowflake","type":0},{"id":"3","type":0}]"""");

        // Act & Assert
        Assert.That(() => Deserialize<GatewayGuildJsonModel>(json), Throws.Nothing);
    }

    [Test]
    public void Deserialize_ChannelsContainMalformedEntry_SafelyDeserializeItemsSkipsOnlyTheMalformedOne()
    {
        // Arrange
        var json = MinimalGuildJson.Replace(
            """"channels": []"""",
            """"channels": [{"id":"not-a-snowflake","type":0},{"id":"3","type":0}]"""");

        var model = Deserialize<GatewayGuildJsonModel>(json);

        // Act
        var channels = model.Channels.SafelyDeserializeItems<ChannelJsonModel>(NullLogger.Instance).ToList();

        // Assert
        Assert.That(channels, Has.Count.EqualTo(1));
        Assert.That(channels[0].Id, Is.EqualTo((Snowflake) 3));
    }
}
