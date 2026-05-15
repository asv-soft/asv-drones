using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public abstract class SetupSubpage : TreeSubpage, ISetupSubpage
{
    protected SetupSubpage(NavId id, ILoggerFactory loggerFactory)
        : base(id.TypeId, id.Args)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected ILogger Logger { get; }

    public virtual ValueTask Init(ISetupPage context)
    {
        return ValueTask.CompletedTask;
    }
}
