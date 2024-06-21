using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Asv.Common;
using DynamicData;
using Microsoft.VisualBasic.FileIO;

namespace Asv.Drones.Gui.Airports;

public class AirportTypeAssociation(string text) : Attribute
{
    public string Text { get; } = text;
}

public enum AirportType
{
    [AirportTypeAssociation("large_airport")]
    LargeAirport,
    [AirportTypeAssociation("medium_airport")]
    MediumAirport,
    [AirportTypeAssociation("small_airport")]
    SmallAirport,
    [AirportTypeAssociation("heliport")]
    Heliport,
    [AirportTypeAssociation("balloonport")]
    Balloonport,
    [AirportTypeAssociation("seaplane_base")]
    SeaplaneBase,
    [AirportTypeAssociation("closed")]
    Closed,
}

public static class AirportsLoader
{
    private static readonly SourceCache<AirportModel, long> AirportsCache = new(k => k.Id);
    public static readonly IObservable<IChangeSet<AirportModel, long>> Airports = AirportsCache.Connect().RefCount();
    
    public static void Load(string path, AirportType[] types)
    {
        AirportsCache.Clear();
        
        using var csvParser = new TextFieldParser(path);
        csvParser.SetDelimiters([";"]);
        csvParser.HasFieldsEnclosedInQuotes = false;
        csvParser.ReadLine();

        var typesToShow = new ObservableCollection<string?>();
        types.ForEach(t => typesToShow.Add(t.Associate()));

        while (!csvParser.EndOfData)
        {
            // Read current line fields, pointer moves to the next line.
            var fields = csvParser.ReadFields();
            
            if (!typesToShow.Contains(fields?[2])) continue;

            long.TryParse(fields?[0], CultureInfo.InvariantCulture, out var id);
            double.TryParse(fields?[4], CultureInfo.InvariantCulture, out var lat);
            double.TryParse(fields?[5], CultureInfo.InvariantCulture, out var lon);
            double.TryParse(fields?[6], CultureInfo.InvariantCulture, out var alt);
            
            
            
            AirportsCache?.AddOrUpdate(new AirportModel
            {
                Id = id,
                Ident = fields[1],
                Name = fields[3],
                Location = new GeoPoint(lat, lon, alt / 3.28),
                GPSCode = fields[12],
                LocalCode = fields[14]
            });
        }
        AirportsCache?.Refresh();
    }
    
    private static string Associate(this Enum enumeration)
    {
        var type = enumeration.GetType();
        var memInfo = type.GetMember(enumeration.ToString());
        if (memInfo.Length <= 0) return enumeration.ToString();
        var attrs = memInfo[0].GetCustomAttributes(typeof(AirportTypeAssociation), false);
        return attrs.Length > 0 ? ((AirportTypeAssociation)attrs[0]).Text : enumeration.ToString();
    }
}