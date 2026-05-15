using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ISupportRename : IViewModel
{
    public const string CommandId = "cmd.rename.commit";
    ValueTask<string> RenameAsync(string oldValue, string newValue, CancellationToken ct);
}
