using Asv.Avalonia;
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
            return UseEditor<MavParamTextBoxPropertyViewModel, MavParamTextBoxPropertyView>(
                MavParamTextBoxPropertyViewModel.TypeId
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
