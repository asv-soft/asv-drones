using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api
{
    public class MapPageViewConfig
    {
        public string Columns { get; set; }
        public string Rows { get; set; }
    }

    public partial class MapPageView : ReactiveUserControl<MapPageViewModel>
    {
        public MapPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected static void SetColumnAndRowDefinitions(Grid grid, MapPageViewConfig cfg)
        {
            var parsedColumnValues = Regex.Split(cfg.Columns, ",");
            var parsedRowValues = Regex.Split(cfg.Rows, ",");

            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            foreach (var value in parsedColumnValues)
            {
                if (value.Contains('*'))
                {
                    int.TryParse(value.Substring(0, value.Length - 1).Split('.').FirstOrDefault(),
                        out var gridLengthValue);

                    grid.ColumnDefinitions.Add(
                        new ColumnDefinition(new GridLength(gridLengthValue, GridUnitType.Star)));
                }
                else
                {
                    int.TryParse(value.Split('.').FirstOrDefault(), out var gridLengthValue);

                    grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(gridLengthValue)));
                }
            }

            foreach (var value in parsedRowValues)
            {
                if (value.Contains('*'))
                {
                    int.TryParse(value.Substring(0, value.Length - 1).Split('.').FirstOrDefault(),
                        out var gridLengthValue);

                    grid.RowDefinitions.Add(new RowDefinition(new GridLength(gridLengthValue, GridUnitType.Star)));
                }
                else
                {
                    int.TryParse(value.Split('.').FirstOrDefault(), out var gridLengthValue);

                    grid.RowDefinitions.Add(new RowDefinition(new GridLength(gridLengthValue)));
                }
            }
        }
    }
}