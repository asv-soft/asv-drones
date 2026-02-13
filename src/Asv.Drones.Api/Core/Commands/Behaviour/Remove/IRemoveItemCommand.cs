using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface IRemoveItemCommand
{
    public const string CommandId = $"{AsyncCommand.BaseId}.remove";
}
