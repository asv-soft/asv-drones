using System.Collections.ObjectModel;
using Avalonia.Input;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Api;

public class TreePageExampleViewModel : TreePageViewModel
{
    private static int _instanceCounter = 0;

    public TreePageExampleViewModel() : base(WellKnownUri.UndefinedUri)
    {
    }

    public TreePageExampleViewModel(ITreePageContext treePageContext, string id) : base(id)
    {
        var collection = new ObservableCollection<IMenuItem>
        {
            new MenuItem("asv:1")
            {
                Header = "Action 1",
                Icon = MaterialIconKind.AccessPoint,
                Command = ReactiveCommand.Create(() => { }),
                Order = 1
            },

            new MenuItem("asv:2")
            {
                Header = "Action 2",
                Icon = MaterialIconKind.AccessPoint,
                Command = ReactiveCommand.Create(() => { }),
                Order = 2,
                Items = new ReadOnlyObservableCollection<IMenuItem>(new ObservableCollection<IMenuItem>
                {
                    new MenuItem("asv:2.1")
                    {
                        Header = "Action 2.1",
                        Icon = MaterialIconKind.AccessPoint,
                        Command = ReactiveCommand.Create(() => { }),
                        Order = 2,
                        HotKey = new KeyGesture(Key.A, KeyModifiers.Alt)
                    },
                    new MenuItem("asv:2.2")
                    {
                        Header = "Action 2.2",
                        Icon = MaterialIconKind.Create,
                        Command = ReactiveCommand.Create(() => { }),
                        Order = 2,
                        HotKey = new KeyGesture(Key.A, KeyModifiers.Alt),
                    },
                })
            },
        };
        _instanceCounter++;
        for (int i = 0; i < Math.Min(_instanceCounter, 3); i++)
        {
            collection.Add(new MenuItem($"asv:{3 + i}")
            {
                Header = $"Action {3 + i}",
                Icon = (MaterialIconKind)i,
                Command = ReactiveCommand.Create(() => { }),
                Order = i
            });
        }

        Actions = new ReadOnlyObservableCollection<IMenuItem>(collection);
    }
}