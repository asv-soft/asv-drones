using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Api;

public class TreePageExplorerDesignTime : ViewModelProviderBase<ITreePageMenuItem>, ITreePageContext
{
    public static TreePageExplorerDesignTime Instance { get; } = new();
    public static TreePageExplorerDesignTime[] Instances { get; } = [Instance];

    public BreadCrumbItem[] BreadCrumbs { get; }

    private TreePageExplorerDesignTime()
    {
        const int rootCnt = 10;
        const int subRootCnt = 10;
        var maxIconIndex = Enum.GetValues<MaterialIconKind>().Length;
        var icons = Enum.GetValues<MaterialIconKind>();
        for (var i = 0; i < rootCnt; i++)
        {
            var root = new TreePageCallbackMenuItem($"asv:{i}",
                $"Settings {i}",
                icons[Random.Shared.Next(0, maxIconIndex)],
                $"Settings {i} description",
                rootCnt - i)
            {
                Status = i % 3 == 0 ? null : i.ToString()
            };
            Source.AddOrUpdate(root);
            for (var j = 0; j < subRootCnt; j++)
            {
                Source.AddOrUpdate(new TreePageCallbackMenuItem($"asv:{i}{j}",
                    $"Settings {i}",
                    icons[Random.Shared.Next(0, maxIconIndex)],
                    $"Settings {i} {j} description",
                    rootCnt - i,
                    root.Id, x => new TreePageExampleViewModel(x, $"asv:{i}{j}"))
                {
                    Status = i % 3 == 0 ? null : i.ToString()
                });
            }
        }

        BreadCrumbs = Source.Items.Take(2).Select((x, i) => new BreadCrumbItem(i == 0, x)).ToArray();
    }

}