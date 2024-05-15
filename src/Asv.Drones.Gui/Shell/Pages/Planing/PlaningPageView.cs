using System.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(PlaningPageViewModel))]
    public class PlaningPageView : MapPageView
    {
        private readonly IConfiguration _configuration;

        public PlaningPageView()
        {
            DesignTime.ThrowIfNotDesignMode();
        }

        [ImportingConstructor]
        public PlaningPageView(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (_configuration.Exist<MapPageViewConfig>(nameof(PlaningPageView)))
            {
                var mapPageViewConfig = _configuration.Get<MapPageViewConfig>(nameof(PlaningPageView));

                SetColumnAndRowDefinitions((Grid)Content, mapPageViewConfig);
            }
        }


        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            var mapPageViewConfig = new MapPageViewConfig
            {
                Columns = ((Grid)this.Content).ColumnDefinitions.ToString(),
                Rows = ((Grid)this.Content).RowDefinitions.ToString()
            };

            _configuration.Set(nameof(PlaningPageView), mapPageViewConfig);
        }
    }
}