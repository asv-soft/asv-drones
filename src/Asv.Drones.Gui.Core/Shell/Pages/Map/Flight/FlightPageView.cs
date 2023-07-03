using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(FlightPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightPageView:MapPageView
    {
        private readonly IConfiguration _configuration;
        public FlightPageView()
        {
            
        }
        [ImportingConstructor]
        public FlightPageView(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (_configuration.Exist<MapPageViewConfig>(nameof(FlightPageView)))
            {
                var mapPageViewConfig = _configuration.Get<MapPageViewConfig>(nameof(FlightPageView));
                
                SetColumnAndRowDefinitions((Grid)Content, mapPageViewConfig);
            }
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            var mapPageViewConfig = new MapPageViewConfig
            {
                Columns = ((Grid)this.Content).ColumnDefinitions.ToString(),
                Rows = ((Grid)this.Content).RowDefinitions.ToString()
            };

            _configuration.Set(nameof(FlightPageView), mapPageViewConfig);
        }
    }
}