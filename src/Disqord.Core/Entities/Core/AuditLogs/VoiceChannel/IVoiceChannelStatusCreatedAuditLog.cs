namespace Disqord.AuditLogs;

public interface IVoiceChannelStatusCreatedAuditLog : IAuditLog
{
    string? Status { get; }
}
