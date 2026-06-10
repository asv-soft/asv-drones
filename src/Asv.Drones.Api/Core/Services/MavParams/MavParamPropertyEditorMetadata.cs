using Asv.Avalonia;
using Asv.Mavlink;
using Material.Icons;
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
        if (info.Category != null)
        {
            editor.DisplayScopes.Add(info.Category);    
        }
        
        if (info.IconColor.HasValue)
        {
            editor.IconColor = info.IconColor.Value;
        }
    }

    public static void ApplyMavParamMetadata(
        this PropertyViewModel editor,
        MavParamInfo info,
        IParamsClientEx client,
        Func<CancellationToken, ValueTask> refresh
    )
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(refresh);

        editor.ApplyMavParamMetadata(info);
        editor.Menu.Add(
            new MenuItem(
                $"{editor.Id}.set-default",
                $"Set default ({GetDefaultValueDisplay(info)})"
            )
            {
                Icon = MaterialIconKind.Restore,
                Order = 10,
                Command = new ReactiveCommand(
                    async (_, cancel) =>
                    {
                        await client
                            .WriteOnce(info.Metadata.Name, info.Convert(info.DefaultValue), cancel)
                            .ConfigureAwait(false);
                        await refresh(cancel).ConfigureAwait(false);
                    }
                ),
            }
        );
        editor.Menu.Add(
            new MenuItem($"{editor.Id}.refresh", "Refresh")
            {
                Icon = MaterialIconKind.Refresh,
                Order = 20,
                Command = new ReactiveCommand(
                    async (_, cancel) => await refresh(cancel).ConfigureAwait(false)
                ),
            }
        );
    }

    public static IDisposable ScheduleInitialRead(Func<CancellationToken, ValueTask> refresh)
    {
        ArgumentNullException.ThrowIfNull(refresh);

        return Observable
            .Timer(TimeSpan.FromMilliseconds(Random.Shared.Next(1, 1000)))
            .Take(1)
            .SubscribeAwait(async (_, cancel) => await refresh(cancel).ConfigureAwait(false));
    }

    private static string GetDefaultValueDisplay(MavParamInfo info)
    {
        return info.Print(info.DefaultValue) ?? info.DefaultValue.ToString() ?? string.Empty;
    }
}
