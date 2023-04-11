using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class ParametersEditorPageViewConfig
{
    public string Columns { get; set; }
}


[ExportView(typeof(ParametersEditorPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ParametersEditorPageView : ReactiveUserControl<ParametersEditorPageViewModel>
{
    private readonly IConfiguration _configuration;
    
    public ParametersEditorPageView()
    {
        InitializeComponent();
    }
    
    [ImportingConstructor]
    public ParametersEditorPageView(IConfiguration configuration) : this()
    {
        _configuration = configuration;
    }
    
    protected override void OnLoaded()
    {
        base.OnLoaded();

        if (_configuration.Exist<ParametersEditorPageViewConfig>(nameof(ParametersEditorPageView)))
        {
            var mapPageViewConfig = _configuration.Get<ParametersEditorPageViewConfig>(nameof(ParametersEditorPageView));
                
            SetColumnDefinitions(MainGrid, mapPageViewConfig);
        }
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();

        var parametersEditorPageViewConfig = new ParametersEditorPageViewConfig
        {
            Columns = MainGrid.ColumnDefinitions.ToString()
        };

        _configuration.Set(nameof(ParametersEditorPageView), parametersEditorPageViewConfig);
    }
    
    protected static void SetColumnDefinitions(Grid grid, ParametersEditorPageViewConfig cfg)
    {
        var parsedColumnValues = Regex.Split(cfg.Columns, ",");

        grid.ColumnDefinitions.Clear();

        foreach (var value in parsedColumnValues)
        {
            if (value.Contains('*'))
            {
                int.TryParse(value.Substring(0, value.Length - 1).Split('.').FirstOrDefault(), out var gridLengthValue);
                
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(gridLengthValue, GridUnitType.Star)));
            }
            else
            {
                int.TryParse(value.Split('.').FirstOrDefault(), out var gridLengthValue);
                    
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(gridLengthValue)));
            }
        }
        
        grid.ColumnDefinitions[0].MinWidth = 220;
                
        grid.ColumnDefinitions[2].MinWidth = 500;
    }
}