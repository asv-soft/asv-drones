using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class ShellRegistrations
{
    extension(AsvDronesRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public AsvDronesRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(AsvDronesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterPages();
            return this;
        }
    }
}
