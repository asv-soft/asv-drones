using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ICommitRenameCommand
{
    public const string CommandId = $"{AsyncCommand.BaseId}.rename.commit";
}
