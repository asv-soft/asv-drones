using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    public class FlightPageViewConfig
    {
        public string Columns { get; set; }
    }
    
    [ExportView(typeof(FlightPageViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
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

            if (_configuration.Exist<FlightPageViewConfig>(nameof(FlightPageViewConfig)))
            {
                var columns = _configuration.Get<FlightPageViewConfig>(nameof(FlightPageViewConfig));
                
                SetColumnDefinitions(((Grid)this.Content).ColumnDefinitions, columns.Columns);
            }
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            var flightPageViewConfig = new FlightPageViewConfig
            {
                Columns = ((Grid)this.Content).ColumnDefinitions.ToString()
            };

            _configuration.Set(nameof(FlightPageViewConfig), flightPageViewConfig);
        }
    }
}