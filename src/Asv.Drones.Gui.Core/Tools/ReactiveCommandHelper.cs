namespace Asv.Drones.Gui.Core;

public static class ReactiveCommandHelper
{
    
}

public delegate Task<TResult> CommandExecuteDelegate<TArg, TResult>(TArg arg, IProgress<double> progress, CancellationToken cancel);