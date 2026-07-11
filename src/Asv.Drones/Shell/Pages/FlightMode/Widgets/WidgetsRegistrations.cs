using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class WidgetsRegistrations
{
    extension(FlightModeRegistrations.Builder builder)
    {
        public Builder Widgets => new(builder);

        public FlightModeRegistrations.Builder RegisterWidgets(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(FlightModeRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterClientDeviceWidgets();
            this.RegisterSections();
            return this;
        }
    }
}
