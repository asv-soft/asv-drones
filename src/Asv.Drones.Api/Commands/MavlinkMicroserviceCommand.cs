using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;

namespace Asv.Drones.Api;

public abstract class MavlinkMicroserviceCommand<TMicroservice, TArg>
    : ContextCommand<IDevicePage, TArg>
    where TArg : CommandArg
    where TMicroservice : class
{
    public override bool CanExecute(
        IRoutable context,
        CommandArg parameter,
        out IRoutable targetContext
    )
    {
        if (base.CanExecute(context, parameter, out targetContext) == false)
        {
            return false;
        }

        if (targetContext is IDevicePage devicePage)
        {
            return devicePage.Device?.GetMicroservice<TMicroservice>() != null;
        }

        return false;
    }

    public override ValueTask<TArg?> InternalExecute(
        IDevicePage context,
        TArg arg,
        CancellationToken cancel
    )
    {
        var microservice = context.Device?.GetMicroservice<TMicroservice>();
        if (microservice == null)
        {
            throw new Exception(
                $"Error to execute command {GetType().Name}[{Info.Id}]: device microservice {nameof(TMicroservice)} not found"
            );
        }
        return InternalExecute(microservice, arg, cancel);
    }

    protected abstract ValueTask<TArg?> InternalExecute(
        TMicroservice microservice,
        TArg arg,
        CancellationToken cancel
    );
}
