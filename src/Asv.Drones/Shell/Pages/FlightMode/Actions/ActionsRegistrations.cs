using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class ActionsRegistrations
{
    extension(FlightModeRegistrations.Builder builder)
    {
        public Builder Actions => new(builder);

        public FlightModeRegistrations.Builder RegisterActions(Action<Builder>? configure = null)
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
            this.RegisterDroneWidgetActions();
            this.RegisterUavAnchorActions();
            this.RegisterDialogs();
            return this;
        }
    }
}
