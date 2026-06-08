using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones.Api;

public static class MavParamMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseMavParam(Action<Builder>? configure = null)
        {
            configure ??= x => x.Default();
            builder.Services.AddSingleton<IMavParamsEditorFactory, MavParamsEditorFactory>();
            configure(new Builder(builder));
            return builder;
        }

        public Builder MavParams => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder Default()
        {
            builder.ViewLocator.RegisterViewFor<
                MavParamGeoPointPropertyViewModel,
                PropertyGeoPointView
            >();

            return UseEditor<MavParamTextBoxPropertyViewModel, PropertyTextBoxView>(
                    MavParamTextBoxPropertyViewModel.TypeId
                )
                .UseEditor<MavParamAsciiCharPropertyViewModel, PropertyTextBoxView>(
                    MavParamAsciiCharPropertyViewModel.TypeId
                )
                .UseEditor<MavParamComboBoxPropertyViewModel, PropertyComboBoxView>(
                    MavParamComboBoxPropertyViewModel.TypeId
                )
                .UseEditor<MavParamButtonPropertyViewModel, PropertyButtonView>(
                    MavParamButtonPropertyViewModel.TypeId
                )
                .UseEditor<MavParamAltitudePropertyViewModel, PropertyUnitView>(
                    MavParamAltitudePropertyViewModel.TypeId
                )
                .UseEditor<MavParamLatitudePropertyViewModel, PropertyUnitView>(
                    MavParamLatitudePropertyViewModel.TypeId
                )
                .UseEditor<MavParamLongitudePropertyViewModel, PropertyUnitView>(
                    MavParamLongitudePropertyViewModel.TypeId
                );
        }

        public Builder UseEditor<TViewModel, TView>(string widgetType)
            where TViewModel : class, IMavParamPropertyViewModel
            where TView : Control
        {
            builder.ViewLocator.RegisterViewFor<TViewModel, TView>();
            builder.ViewModel.RegisterKeyedWithArgs<
                IMavParamPropertyViewModel,
                TViewModel,
                IMavParamContext
            >(widgetType);
            return this;
        }
    }
}
