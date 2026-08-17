using System.Runtime.CompilerServices;
using Disqord.Models;
using Qommon;

namespace Disqord.Rest.Api;

public static partial class RestContentValidation
{
    public static class AutoModeration
    {
        public static void ValidateName(string name, [CallerArgumentExpression(nameof(name))] string? argumentExpression = null)
        {
            Guard.IsNotNullOrWhiteSpace(name, argumentExpression);
            Guard.HasSizeLessThanOrEqualTo(name, Discord.Limits.AutoModerationRule.MaxNameLength, argumentExpression);
        }

        public static void ValidateName(Optional<string> name, [CallerArgumentExpression(nameof(name))] string? argumentExpression = null)
        {
            OptionalGuard.CheckValue(name, static name => ValidateName(name), argumentExpression);
        }

        public static void ValidateExemptRoles(Optional<Snowflake[]> exemptRoles, [CallerArgumentExpression(nameof(exemptRoles))] string? argumentExpression = null)
        {
            OptionalGuard.CheckValue(exemptRoles, static exemptRoles =>
            {
                Guard.HasSizeLessThanOrEqualTo(exemptRoles, Discord.Limits.AutoModerationRule.MaxExemptRoleAmount);
            }, argumentExpression);
        }

        public static void ValidateExemptChannels(Optional<Snowflake[]> exemptChannels, [CallerArgumentExpression(nameof(exemptChannels))] string? argumentExpression = null)
        {
            OptionalGuard.CheckValue(exemptChannels, static exemptChannels =>
            {
                Guard.HasSizeLessThanOrEqualTo(exemptChannels, Discord.Limits.AutoModerationRule.MaxExemptChannelAmount);
            }, argumentExpression);
        }

        public static void ValidateActions(AutoModerationActionJsonModel[] actions, [CallerArgumentExpression(nameof(actions))] string? argumentExpression = null)
        {
            Guard.IsNotNull(actions, argumentExpression);
            Guard.IsGreaterThanOrEqualTo(actions.Length, Discord.Limits.AutoModerationRule.MinActionAmount, argumentExpression);
            Guard.HasSizeLessThanOrEqualTo(actions, Discord.Limits.AutoModerationRule.MaxActionAmount, argumentExpression);

            for (var i = 0; i < actions.Length; i++)
            {
                actions[i].Validate();
            }
        }

        public static void ValidateActions(Optional<AutoModerationActionJsonModel[]> actions, [CallerArgumentExpression(nameof(actions))] string? argumentExpression = null)
        {
            OptionalGuard.CheckValue(actions, static actions => ValidateActions(actions), argumentExpression);
        }

        public static void ValidateTriggerMetadata(AutoModerationRuleTrigger trigger, Optional<AutoModerationTriggerMetadataJsonModel> triggerMetadata, [CallerArgumentExpression(nameof(triggerMetadata))] string? argumentExpression = null)
        {
            if (trigger is not AutoModerationRuleTrigger.Spam)
            {
                OptionalGuard.HasValue(triggerMetadata, argumentExpression);
            }

            if (!triggerMetadata.HasValue)
            {
                return;
            }

            var metadata = triggerMetadata.Value;
            switch (trigger)
            {
                case AutoModerationRuleTrigger.Keyword:
                case AutoModerationRuleTrigger.MemberProfile:
                {
                    ValidateKeywordFilter(metadata);
                    ValidateRegexPatterns(metadata);
                    ValidateAllowList(metadata, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxKeywordAllowListAmount);
                    break;
                }
                case AutoModerationRuleTrigger.KeywordPreset:
                {
                    OptionalGuard.HasValue(metadata.Presets);
                    ValidateAllowList(metadata, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxKeywordPresetAllowListAmount);
                    break;
                }
                case AutoModerationRuleTrigger.MentionSpam:
                {
                    OptionalGuard.HasValue(metadata.MentionTotalLimit);
                    Guard.IsBetweenOrEqualTo(metadata.MentionTotalLimit.Value, Discord.Limits.AutoModerationRule.TriggerMetadata.MinMentionLimit, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxMentionLimit);
                    break;
                }
            }
        }

        public static void ValidateTriggerMetadata(Optional<AutoModerationTriggerMetadataJsonModel> triggerMetadata, [CallerArgumentExpression(nameof(triggerMetadata))] string? argumentExpression = null)
        {
            if (!triggerMetadata.HasValue)
            {
                return;
            }

            var metadata = triggerMetadata.Value;
            ValidateKeywordFilter(metadata);
            ValidateRegexPatterns(metadata);
            ValidateAllowList(metadata, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxKeywordPresetAllowListAmount);

            if (metadata.MentionTotalLimit.HasValue)
            {
                Guard.IsBetweenOrEqualTo(metadata.MentionTotalLimit.Value, Discord.Limits.AutoModerationRule.TriggerMetadata.MinMentionLimit, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxMentionLimit);
            }
        }

        private static void ValidateKeywordFilter(AutoModerationTriggerMetadataJsonModel metadata)
        {
            if (!metadata.KeywordFilter.HasValue)
            {
                return;
            }

            var keywordFilter = metadata.KeywordFilter.Value;
            Guard.HasSizeLessThanOrEqualTo(keywordFilter, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxKeywordAmount);

            for (var i = 0; i < keywordFilter.Length; i++)
            {
                Guard.HasSizeBetweenOrEqualTo(keywordFilter[i], Discord.Limits.AutoModerationRule.TriggerMetadata.MinKeywordLength, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxKeywordLength);
            }
        }

        private static void ValidateRegexPatterns(AutoModerationTriggerMetadataJsonModel metadata)
        {
            if (!metadata.RegexPatterns.HasValue)
            {
                return;
            }

            var regexPatterns = metadata.RegexPatterns.Value;
            Guard.HasSizeLessThanOrEqualTo(regexPatterns, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxRegexPatternAmount);

            for (var i = 0; i < regexPatterns.Length; i++)
            {
                Guard.HasSizeBetweenOrEqualTo(regexPatterns[i], Discord.Limits.AutoModerationRule.TriggerMetadata.MinRegexPatternLength, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxRegexPatternLength);
            }
        }

        private static void ValidateAllowList(AutoModerationTriggerMetadataJsonModel metadata, int maxAmount)
        {
            if (!metadata.AllowList.HasValue)
            {
                return;
            }

            var allowList = metadata.AllowList.Value;
            Guard.HasSizeLessThanOrEqualTo(allowList, maxAmount);

            for (var i = 0; i < allowList.Length; i++)
            {
                Guard.HasSizeBetweenOrEqualTo(allowList[i], Discord.Limits.AutoModerationRule.TriggerMetadata.MinAllowedKeywordLength, Discord.Limits.AutoModerationRule.TriggerMetadata.MaxAllowedKeywordLength);
            }
        }
    }
}
