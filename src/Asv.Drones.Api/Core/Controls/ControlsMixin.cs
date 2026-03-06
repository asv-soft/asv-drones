using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones.Api;

public static class ControlsMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseApiControls(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }
    }
    
    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder UseDefault()
        {
            return UseMavParams();
        }

        public Builder UseMavParams()
        {
            builder.ViewLocator.RegisterViewFor<MavParamButtonViewModel, MavParamButtonView>();
            builder.ViewLocator.RegisterViewFor<MavParamComboBoxViewModel, MavParamComboBoxView>();
            builder.ViewLocator.RegisterViewFor<MavParamAltitudeTextBoxViewModel, MavParamAltitudeTextBoxView>();
            builder.ViewLocator.RegisterViewFor<MavParamLatLonTextBoxViewModel, MavParamLatLonTextBoxView>();
            builder.ViewLocator.RegisterViewFor<MavParamAsciiCharViewModel, MavParamAsciiCharView>();
            builder.ViewLocator.RegisterViewFor<MavParamButtonViewModel, MavParamButtonView>();
            return this;
        }
    }
}