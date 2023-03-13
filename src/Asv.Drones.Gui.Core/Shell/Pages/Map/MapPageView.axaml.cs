using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
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

        protected static void SetColumnDefinitions(ColumnDefinitions columnDefinitions, string values)
        {
            var parsedValues = Regex.Split(values, ",");

            columnDefinitions.Clear();
            
            foreach (var value in parsedValues)
            {
                if (value.Contains('*'))
                {
                    int.TryParse(value.AsSpan(0, (value.Length - 1)), out var gridLengthValue);
                    
                    columnDefinitions.Add(new ColumnDefinition(new GridLength(gridLengthValue, GridUnitType.Star)));
                }
                else
                {
                    int.TryParse(value, out var gridLengthValue);
                    
                    columnDefinitions.Add(new ColumnDefinition(new GridLength(gridLengthValue)));
                }
            }
        }
    }
}