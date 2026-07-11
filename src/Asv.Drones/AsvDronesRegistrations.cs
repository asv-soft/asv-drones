using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class AsvDronesRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder AsvDrones => new(builder);

        public IHostApplicationBuilder RegisterDronesApp(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterCore();
            this.RegisterShell();
            return this;
        }
    }
}
