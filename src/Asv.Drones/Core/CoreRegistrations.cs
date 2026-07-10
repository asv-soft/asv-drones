using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class CoreRegistrations
{
    extension(AsvDronesRegistrations.Builder builder)
    {
        public Builder Core => new(builder);

        public AsvDronesRegistrations.Builder RegisterCore(Action<Builder>? configure = null)
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
            this.RegisterServices();
            return this;
        }
    }
}
