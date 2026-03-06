using Asv.Avalonia;
using Avalonia.Markup.Xaml;

namespace Asv.Drones;

public partial class App : AsvApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
