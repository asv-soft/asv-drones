using Asv.Avalonia;
using R3;

namespace Asv.Drones.Api;

internal static class MavParamPropertyEditorMetadata
{
    public static void ApplyMavParamMetadata(this PropertyViewModel editor, MavParamInfo info)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(info);

        editor.Header = info.Title;
        editor.ShortHeader = info.Shortcut;
        editor.Description = info.Description;
        editor.Icon = info.Icon;
        if (info.IconColor.HasValue)
        {
            editor.IconColor = info.IconColor.Value;
        }
    }

    public static IDisposable ScheduleInitialRead(Func<CancellationToken, ValueTask> refresh)
    {
        ArgumentNullException.ThrowIfNull(refresh);

        return Observable
            .Timer(TimeSpan.FromMilliseconds(Random.Shared.Next(1, 1000)))
            .Take(1)
            .SubscribeAwait(async (_, cancel) => await refresh(cancel).ConfigureAwait(false));
    }
}
