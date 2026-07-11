using Asv.Avalonia;
using Asv.Drones.Api;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Drones;

public static class SetupRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterSetup(Action<Builder>? configure = null)
        {
            configure ??= x => x.RegisterDefault();
            var setup = new Builder(builder);
            configure(setup);
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            AppBuilder.Shell.Pages.Register<SetupPageViewModel, SetupPageView>(
                SetupPageViewModel.PageId
            );
            AppBuilder.Shell.Pages.Home.UseItemExtension<HomePageSetupDeviceItemAction>();
            AppBuilder.Extensions.Register<ISetupPage, DefaultSetupExtension>();
            RegisterFrameTypeSubPage();
            RegisterMotorSubPage();
            return this;
        }

        public Builder RegisterFrameTypeSubPage()
        {
            AppBuilder.Extensions.Register<ISetupPage, FrameTypeSetupPageExtension>();
            AppBuilder.ViewLocator.RegisterViewFor<DroneFrameItemViewModel, DroneFrameItemView>();
            AddSubPage<SetupFrameTypeViewModel, SetupFrameTypeView>(SetupFrameTypeViewModel.PageId);
            return this;
        }

        public Builder RegisterMotorSubPage()
        {
            AppBuilder.ViewLocator.RegisterViewFor<MotorItemViewModel, MotorItemView>();
            AddSubPage<SetupMotorsViewModel, SetupMotorsView>(SetupMotorsViewModel.PageId);
            AppBuilder.Extensions.Register<ISetupPage, MotorsSetupPageExtension>();
            return this;
        }

        public Builder AddSubPage<
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TViewModel,
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TView
        >(string pageId)
            where TViewModel : class, ISetupSubpage
            where TView : Control
        {
            AppBuilder.TreePage.Register<ISetupPage, ISetupSubpage, TViewModel, TView>(pageId);
            return this;
        }

        public Builder AddSubPage<
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TViewModel,
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TView,
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
            )]
                TTreeMenu
        >(string pageId)
            where TViewModel : class, ISetupSubpage
            where TView : Control
            where TTreeMenu : class, ITreePageMenuItem
        {
            AddSubPage<TViewModel, TView>(pageId);
            AppBuilder.Services.AddKeyedTransient<ITreePageMenuItem, TTreeMenu>(
                SetupPageViewModel.PageId
            );
            return this;
        }
    }
}
