using Disqord.Models;

namespace Disqord;

public class TransientCollectibles(CollectiblesJsonModel model)
    : TransientEntity<CollectiblesJsonModel>(model), ICollectibles
{
    /// <inheritdoc/>
    public INameplate? Nameplate
    {
        get
        {
            if (!Model.Nameplate.HasValue || Model.Nameplate.Value == null)
                return null;

            return field ??= new TransientNameplate(Model.Nameplate.Value);
        }
    }
}
