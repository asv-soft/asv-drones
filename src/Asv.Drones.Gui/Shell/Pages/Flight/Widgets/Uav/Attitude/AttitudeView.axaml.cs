using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui
{
    public partial class AttitudeView : UserControl
    {
        public AttitudeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}