using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class SetupMixin
{
    extension(AsvDronesMixin.Builder builder)
    {
        public AsvDronesMixin.Builder UseSetupPage(Action<Builder>? configure = null)
        {
            configure ??= x => x.UseDefault();
            var setup = new Builder(builder.Parent);
            configure(setup);
            return builder;
        }
    }

    extension(ShellMixin.PageBuilder builder)
    {
        public Builder SetupPage => new(builder.Parent.Parent);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder UseDefault()
        {
            builder.Shell.Pages.Register<SetupPageViewModel, SetupPageView>(
                SetupPageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseItemExtension<HomePageSetupDeviceItemAction>();
            builder.Extensions.Register<ISetupPage, DefaultSetupExtension>();
            return this;
        }

        public Builder UseFrameTypeSubPage()
        {
            builder.Extensions.Register<ISetupPage, FrameTypeSetupPageExtension>();
            builder.ViewLocator.RegisterViewFor<DroneFrameItemViewModel, DroneFrameItemView>();
            AddSubPage<SetupFrameTypeViewModel, SetupFrameTypeView>(SetupFrameTypeViewModel.PageId);
            return this;
        }

        public Builder UseMotorSubPage()
        {
            builder.ViewLocator.RegisterViewFor<MotorItemViewModel, MotorItemView>();
            AddSubPage<SetupMotorsViewModel, SetupMotorsView>(SetupMotorsViewModel.PageId);
            builder.Extensions.Register<ISetupPage, MotorsSetupPageExtension>();
            return this;
        }

        public Builder AddSubPage<TViewModel, TView>(string pageId)
            where TViewModel : class, ISetupSubpage
            where TView : Control
        {
            builder.ViewLocator.RegisterViewFor<TViewModel, TView>();
            builder.Services.AddKeyedTransient<ISetupSubpage, TViewModel>(pageId);
            return this;
        }

        public Builder AddSubPage<TViewModel, TView, TTreeMenu>(string pageId)
            where TViewModel : class, ISetupSubpage
            where TView : Control
            where TTreeMenu : class, ITreePage
        {
            AddSubPage<TViewModel, TView>(pageId);
            builder.Services.AddKeyedTransient<ITreePage, TTreeMenu>(SetupPageViewModel.PageId);
            return this;
        }
    }
}
