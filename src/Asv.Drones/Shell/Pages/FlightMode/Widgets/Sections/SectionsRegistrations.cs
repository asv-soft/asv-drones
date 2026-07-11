using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class SectionsRegistrations
{
    extension(WidgetsRegistrations.Builder builder)
    {
        public Builder Sections => new(builder);

        public WidgetsRegistrations.Builder RegisterSections(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(WidgetsRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterTelemetrySection();
            this.RegisterTelemetryItemFactories();
            this.RegisterAttitudeIndicatorSection();
            return this;
        }
    }
}
