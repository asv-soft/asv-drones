using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Api;

public class MavParamsEditorFactory : IMavParamsEditorFactory
{
    private readonly IServiceProvider _container;

    public MavParamsEditorFactory(IServiceProvider container)
    {
        ArgumentNullException.ThrowIfNull(container);
        _container = container;
    }

    public IEnumerable<IMavParamPropertyViewModel> CreateList(
        IParamsClientEx client,
        params IEnumerable<IMavParamTypeMetadata> paramList
    )
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(paramList);

        var infos = paramList.Select(param => new MavParamInfo(param)).ToArray();
        var geoGroups = CreateGeoPointGroups(infos);
        var consumed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var info in infos)
        {
            if (consumed.Contains(info.Metadata.Name))
            {
                continue;
            }

            if (
                TryGetCompleteGeoPointGroup(info, geoGroups, out var geoGroup)
                && TryCreateGeoPointEditor(geoGroup, client, out var geoEditor)
            )
            {
                consumed.Add(geoGroup.Latitude.Metadata.Name);
                consumed.Add(geoGroup.Longitude.Metadata.Name);
                consumed.Add(geoGroup.Altitude.Metadata.Name);
                yield return geoEditor;
                continue;
            }

            var editor = Create(info, client);
            if (editor is not null)
            {
                yield return editor;
            }
        }
    }

    public IMavParamPropertyViewModel? Create(IMavParamTypeMetadata param, IParamsClientEx client)
    {
        ArgumentNullException.ThrowIfNull(param);
        ArgumentNullException.ThrowIfNull(client);

        return Create(new MavParamInfo(param), client);
    }

    private IMavParamPropertyViewModel? Create(MavParamInfo info, IParamsClientEx client)
    {
        if (MavParamWidgetIds.GetIdByName(info.WidgetType) == MavParamWidgetIds.Hidden)
        {
            return null;
        }

        var context = new MavParamContext(info, client);
        foreach (var key in GetWidgetTypeKeys(info.WidgetType))
        {
            var editor = _container.TryCreateViewModel<
                IMavParamPropertyViewModel,
                IMavParamContext
            >(key, context);
            if (editor is not null)
            {
                return editor;
            }
        }

        return null;
    }

    private bool TryCreateGeoPointEditor(
        GeoPointParamGroup group,
        IParamsClientEx client,
        out IMavParamPropertyViewModel editor
    )
    {
        try
        {
            editor = new MavParamGeoPointPropertyViewModel(
                group.Latitude,
                group.Longitude,
                group.Altitude,
                client,
                _container.GetRequiredService<IUnitService>(),
                _container.GetRequiredService<IDialogService>()
            )
            {
                Header = group.Altitude.GroupBy,
            };
            return true;
        }
        catch
        {
            editor = null!;
            return false;
        }
    }

    private static Dictionary<string, GeoPointGroupBuilder> CreateGeoPointGroups(
        IEnumerable<MavParamInfo> infos
    )
    {
        var groups = new Dictionary<string, GeoPointGroupBuilder>(StringComparer.OrdinalIgnoreCase);
        foreach (var info in infos)
        {
            if (
                string.IsNullOrWhiteSpace(info.GroupBy)
                || TryGetGeoPointAxis(info, out var axis) == false
            )
            {
                continue;
            }

            if (groups.TryGetValue(info.GroupBy, out var group) == false)
            {
                group = new GeoPointGroupBuilder();
                groups.Add(info.GroupBy, group);
            }

            group.Set(axis, info);
        }

        return groups;
    }

    private static bool TryGetCompleteGeoPointGroup(
        MavParamInfo info,
        Dictionary<string, GeoPointGroupBuilder> groups,
        out GeoPointParamGroup group
    )
    {
        group = default;
        if (
            string.IsNullOrWhiteSpace(info.GroupBy)
            || TryGetGeoPointAxis(info, out _) == false
            || groups.TryGetValue(info.GroupBy, out var builder) == false
        )
        {
            return false;
        }

        return builder.TryBuild(out group);
    }

    private static bool TryGetGeoPointAxis(MavParamInfo info, out GeoPointAxis axis)
    {
        axis = default;
        switch (MavParamWidgetIds.GetIdByName(info.WidgetType))
        {
            case MavParamWidgetIds.Latitude:
                axis = GeoPointAxis.Latitude;
                return true;
            case MavParamWidgetIds.Longitude:
                axis = GeoPointAxis.Longitude;
                return true;
            case MavParamWidgetIds.Altitude:
                axis = GeoPointAxis.Altitude;
                return true;
            default:
                return false;
        }
    }

    private static IEnumerable<string> GetWidgetTypeKeys(string? widgetType)
    {
        if (!string.IsNullOrWhiteSpace(widgetType))
        {
            var widgetId = MavParamWidgetIds.GetIdByName(widgetType);
            yield return widgetId;

            var rawWidgetType = widgetType.Trim();
            if (rawWidgetType != widgetId)
            {
                yield return rawWidgetType;
            }
        }

        yield return MavParamTextBoxPropertyViewModel.TypeId;
    }

    private enum GeoPointAxis
    {
        Latitude,
        Longitude,
        Altitude,
    }

    private readonly record struct GeoPointParamGroup(
        MavParamInfo Latitude,
        MavParamInfo Longitude,
        MavParamInfo Altitude
    );

    private sealed class GeoPointGroupBuilder
    {
        private bool _hasDuplicates;

        public MavParamInfo? Latitude { get; private set; }
        public MavParamInfo? Longitude { get; private set; }
        public MavParamInfo? Altitude { get; private set; }

        public void Set(GeoPointAxis axis, MavParamInfo info)
        {
            switch (axis)
            {
                case GeoPointAxis.Latitude:
                    if (Latitude is not null)
                    {
                        _hasDuplicates = true;
                    }
                    else
                    {
                        Latitude = info;
                    }
                    break;
                case GeoPointAxis.Longitude:
                    if (Longitude is not null)
                    {
                        _hasDuplicates = true;
                    }
                    else
                    {
                        Longitude = info;
                    }
                    break;
                case GeoPointAxis.Altitude:
                    if (Altitude is not null)
                    {
                        _hasDuplicates = true;
                    }
                    else
                    {
                        Altitude = info;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public bool TryBuild(out GeoPointParamGroup group)
        {
            if (_hasDuplicates || Latitude is null || Longitude is null || Altitude is null)
            {
                group = default;
                return false;
            }

            group = new GeoPointParamGroup(Latitude, Longitude, Altitude);
            return true;
        }
    }
}
