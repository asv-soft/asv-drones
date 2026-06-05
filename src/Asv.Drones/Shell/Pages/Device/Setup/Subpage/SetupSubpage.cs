using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public abstract class SetupSubpage : TreeSubpage<ISetupPage>, ISetupSubpage
{
    protected SetupSubpage(
        string typeId,
        ITreeSubPageContext<ISetupPage> context,
        ILoggerFactory loggerFactory
    )
        : base(typeId, context)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected ILogger Logger { get; }
}

internal sealed class DesignTimeSetupSubPageContext : ITreeSubPageContext<ISetupPage>
{
    public static ITreeSubPageContext<ISetupPage> Instance { get; } =
        new DesignTimeSetupSubPageContext();

    public NavArgs Args => default;

    public ISetupPage Context => null!;
}
