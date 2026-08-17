using Disqord.Models;
using Disqord.Tests.Serialization;

namespace Disqord.Tests.Unit.Gateway;

public class ObfuscatedChannelJsonModelDeserializationTests : SerializationTestBase
{
    private const string ObfuscatedTextChannelJson = """
        {
            "application_id": null,
            "available_tags": [],
            "default_reaction_emoji": null,
            "default_sort_order": null,
            "default_tag_setting": null,
            "flags": 131072,
            "game_id": null,
            "hd_streaming_buyer_id": null,
            "hd_streaming_until": null,
            "icon_emoji": null,
            "id": "1537921511132762214",
            "last_message_id": null,
            "last_pin_timestamp": null,
            "linked_lobby": null,
            "name": "___hidden___",
            "nsfw": false,
            "parent_id": "1537921509107171379",
            "permission_overwrites": [
                {
                    "allow": "0",
                    "deny": "1024",
                    "id": "1477785298699030630",
                    "type": 0
                }
            ],
            "position": 4,
            "rate_limit_per_user": 0,
            "rtc_region": null,
            "status": null,
            "theme_color": null,
            "topic": null,
            "type": 0,
            "version": 1786739461487,
            "voice_background_display": null,
            "voice_hangout": null
        }
        """;

    [Test]
    public void Deserialize_ObfuscatedChannel_DoesNotThrow()
    {
        // Act & Assert
        Assert.That(() => Deserialize<ChannelJsonModel>(ObfuscatedTextChannelJson), Throws.Nothing);
    }

    [Test]
    public void Deserialize_ObfuscatedChannel_RetainsObfuscatedFlagAndIdentifyingMetadata()
    {
        // Act
        var model = Deserialize<ChannelJsonModel>(ObfuscatedTextChannelJson);

        // Assert
        Assert.That(model.Flags.Value.HasFlag(GuildChannelFlags.Obfuscated), Is.True);
        Assert.That(model.Id, Is.EqualTo((Snowflake) 1537921511132762214));
        Assert.That(model.Type, Is.EqualTo(ChannelType.Text));
        Assert.That(model.Position.Value, Is.EqualTo(4));
        Assert.That(model.ParentId.Value, Is.EqualTo((Snowflake) 1537921509107171379));
    }
}
