using System;
using Disqord.Models;

namespace Disqord;

public class TransientModalComponent<TModalComponentModel>(ModalBaseComponentJsonModel model)
    : TransientBaseComponent<TModalComponentModel>(model), IModalComponent
    where TModalComponentModel : ModalBaseComponentJsonModel
{
    void IJsonUpdatable<ModalBaseComponentJsonModel>.Update(ModalBaseComponentJsonModel model)
    {
        throw new NotSupportedException("Transient entities do not support updates.");
    }
}
