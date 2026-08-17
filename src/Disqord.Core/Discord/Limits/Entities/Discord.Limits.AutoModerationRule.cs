namespace Disqord;

public static partial class Discord
{
    public static partial class Limits
    {
        /// <summary>
        ///     Represents limits for auto-moderation rules.
        /// </summary>
        public static class AutoModerationRule
        {
            /// <summary>
            ///     The maximum length of names.
            /// </summary>
            public const int MaxNameLength = 100;

            /// <summary>
            ///     The minimum amount of actions.
            /// </summary>
            public const int MinActionAmount = 1;

            /// <summary>
            ///     The maximum amount of actions.
            /// </summary>
            public const int MaxActionAmount = 5;

            /// <summary>
            ///     The maximum amount of exempt roles.
            /// </summary>
            public const int MaxExemptRoleAmount = 20;

            /// <summary>
            ///     The maximum amount of exempt channels.
            /// </summary>
            public const int MaxExemptChannelAmount = 50;

            /// <summary>
            ///     Represents limits for auto-moderation rule trigger metadata.
            /// </summary>
            public static class TriggerMetadata
            {
                /// <summary>
                ///     The maximum amount of keywords in the keyword filter.
                /// </summary>
                public const int MaxKeywordAmount = 1000;

                /// <summary>
                ///     The minimum length of a keyword.
                /// </summary>
                public const int MinKeywordLength = 1;

                /// <summary>
                ///     The maximum length of a keyword.
                /// </summary>
                public const int MaxKeywordLength = 60;

                /// <summary>
                ///     The maximum amount of regex patterns.
                /// </summary>
                public const int MaxRegexPatternAmount = 10;

                /// <summary>
                ///     The minimum length of a regex pattern.
                /// </summary>
                public const int MinRegexPatternLength = 1;

                /// <summary>
                ///     The maximum length of a regex pattern.
                /// </summary>
                public const int MaxRegexPatternLength = 260;

                /// <summary>
                ///     The maximum amount of allow list keywords when the trigger type is <see cref="AutoModerationRuleTrigger.Keyword"/> or <see cref="AutoModerationRuleTrigger.MemberProfile"/>.
                /// </summary>
                public const int MaxKeywordAllowListAmount = 100;

                /// <summary>
                ///     The maximum amount of allow list keywords when the trigger type is <see cref="AutoModerationRuleTrigger.KeywordPreset"/>.
                /// </summary>
                public const int MaxKeywordPresetAllowListAmount = 1000;

                /// <summary>
                ///     The minimum length of an allow list keyword.
                /// </summary>
                public const int MinAllowedKeywordLength = 1;

                /// <summary>
                ///     The maximum length of an allow list keyword.
                /// </summary>
                public const int MaxAllowedKeywordLength = 60;

                /// <summary>
                ///     The minimum mention limit when the trigger type is <see cref="AutoModerationRuleTrigger.MentionSpam"/>.
                /// </summary>
                public const int MinMentionLimit = 0;

                /// <summary>
                ///     The maximum mention limit when the trigger type is <see cref="AutoModerationRuleTrigger.MentionSpam"/>.
                /// </summary>
                public const int MaxMentionLimit = 50;
            }

            /// <summary>
            ///     Represents limits for auto-moderation rule action metadata.
            /// </summary>
            public static class ActionMetadata
            {
                /// <summary>
                ///     The maximum length of a custom message when the action type is <see cref="AutoModerationActionType.BlockMessage"/>.
                /// </summary>
                public const int MaxCustomMessageLength = 300;

                /// <summary>
                ///     The minimum timeout duration in seconds when the action type is <see cref="AutoModerationActionType.Timeout"/>.
                /// </summary>
                public const int MinTimeoutDurationSeconds = 0;

                /// <summary>
                ///     The maximum timeout duration in seconds when the action type is <see cref="AutoModerationActionType.Timeout"/>.
                /// </summary>
                public const int MaxTimeoutDurationSeconds = 2419200;
            }
        }
    }
}
