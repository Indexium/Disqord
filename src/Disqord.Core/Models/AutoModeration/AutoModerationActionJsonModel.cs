using Disqord.Serialization.Json;
using Qommon;

namespace Disqord.Models;

public class AutoModerationActionJsonModel : JsonModel
{
    [JsonProperty("type")]
    public AutoModerationActionType Type;

    [JsonProperty("metadata")]
    public Optional<AutoModerationActionMetadataJsonModel> Metadata;

    protected override void OnValidate()
    {
        switch (Type)
        {
            case AutoModerationActionType.SendAlertMessage:
            case AutoModerationActionType.Timeout:
            {
                OptionalGuard.HasValue(Metadata);
                break;
            }
        }

        if (!Metadata.HasValue)
        {
            return;
        }

        var metadata = Metadata.Value;
        switch (Type)
        {
            case AutoModerationActionType.BlockMessage:
            {
                OptionalGuard.CheckValue(metadata.CustomMessage, static customMessage =>
                {
                    Guard.HasSizeLessThanOrEqualTo(customMessage, Discord.Limits.AutoModerationRule.ActionMetadata.MaxCustomMessageLength);
                });

                break;
            }
            case AutoModerationActionType.SendAlertMessage:
            {
                OptionalGuard.HasValue(metadata.ChannelId);
                break;
            }
            case AutoModerationActionType.Timeout:
            {
                OptionalGuard.HasValue(metadata.DurationSeconds);
                Guard.IsBetweenOrEqualTo(metadata.DurationSeconds.Value, Discord.Limits.AutoModerationRule.ActionMetadata.MinTimeoutDurationSeconds, Discord.Limits.AutoModerationRule.ActionMetadata.MaxTimeoutDurationSeconds);
                break;
            }
        }
    }
}
