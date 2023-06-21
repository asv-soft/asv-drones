using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(PlaningMissionItemViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PlaningMissionItemView : ReactiveUserControl<PlaningMissionItemViewModel>
    {
        public PlaningMissionItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            ViewModel.NavigateMapToMe();
        }
    }
}