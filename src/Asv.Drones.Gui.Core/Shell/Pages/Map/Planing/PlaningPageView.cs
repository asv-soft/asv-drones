using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(PlaningPageViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PlaningPageView:MapPageView
    {
        private IConfiguration _configuration;
        public PlaningPageView()
        {
        }

        [ImportingConstructor]
        public PlaningPageView(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (_configuration.Exist<MapPageViewConfig>(nameof(PlaningPageView)))
            {
                var mapPageViewConfig = _configuration.Get<MapPageViewConfig>(nameof(PlaningPageView));
                
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

            _configuration.Set(nameof(PlaningPageView), mapPageViewConfig);
        }
    }
}