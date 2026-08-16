using System.Threading.Tasks;
using Disqord.Gateway.Api;
using Disqord.Gateway.Api.Models;

namespace Disqord.Gateway.Default.Dispatcher;

public class ThreadMemberUpdateDispatchHandler : DispatchHandler<ThreadMemberUpdateJsonModel, CurrentThreadMemberUpdatedEventArgs>
{
    public override ValueTask<CurrentThreadMemberUpdatedEventArgs?> HandleDispatchAsync(IShard shard, ThreadMemberUpdateJsonModel model)
    {
        var thread = Client.GetChannel(model.GuildId, model.Id.Value) as CachedThreadChannel;
        thread?.Update(model);
        var member = thread?.CurrentMember ?? new TransientThreadMember(Client, model);
        var e = new CurrentThreadMemberUpdatedEventArgs(model.GuildId, model.Id.Value, thread, member);
        return new(e);
    }
}
