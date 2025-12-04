using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public abstract class SetupSubpage : TreeSubpage<ISetupPage>, ISetupSubpage
{
    protected SetupSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory) { }
}
