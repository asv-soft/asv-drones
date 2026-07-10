using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones.Api;

public static class MavParamRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder RegisterMavParams(Action<Builder>? configure = null)
        {
            configure ??= x => x.RegisterDefault();
            builder.Services.AddSingleton<IMavParamsEditorFactory, MavParamsEditorFactory>();
            configure(new Builder(builder));
            return builder;
        }

        public Builder MavParams => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            builder.ViewLocator.RegisterViewFor<
                MavParamGeoPointPropertyViewModel,
                PropertyGeoPointView
            >();

            return RegisterEditor<MavParamTextBoxPropertyViewModel, PropertyTextBoxView>(
                    MavParamTextBoxPropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamAsciiCharPropertyViewModel, PropertyTextBoxView>(
                    MavParamAsciiCharPropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamComboBoxPropertyViewModel, PropertyComboBoxView>(
                    MavParamComboBoxPropertyViewModel.TypeId
                )
                .RegisterEditor<
                    MavParamToggleButtonGroupPropertyViewModel,
                    PropertyToggleButtonGroupView
                >(MavParamToggleButtonGroupPropertyViewModel.TypeId)
                .RegisterEditor<MavParamToggleSwitchPropertyViewModel, PropertyToggleSwitchView>(
                    MavParamToggleSwitchPropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamSliderPropertyViewModel, PropertySliderView>(
                    MavParamSliderPropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamButtonPropertyViewModel, PropertyButtonView>(
                    MavParamButtonPropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamAltitudePropertyViewModel, PropertyUnitView>(
                    MavParamAltitudePropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamLatitudePropertyViewModel, PropertyUnitView>(
                    MavParamLatitudePropertyViewModel.TypeId
                )
                .RegisterEditor<MavParamLongitudePropertyViewModel, PropertyUnitView>(
                    MavParamLongitudePropertyViewModel.TypeId
                );
        }

        public Builder RegisterEditor<
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TViewModel,
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TView
        >(string widgetType)
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
