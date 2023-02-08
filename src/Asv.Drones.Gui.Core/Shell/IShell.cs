using Avalonia.Controls;
using System.ComponentModel.Composition.Hosting;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    public interface IShell
    {
        IShellPage CurrentPage { get; set; }
    }
}