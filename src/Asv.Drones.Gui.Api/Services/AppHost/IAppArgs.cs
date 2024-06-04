namespace Asv.Drones.Gui.Api;

public interface IAppArgs
{
    IReadOnlyDictionary<string,string> Args { get; }
    IReadOnlySet<string> Tags { get; }

    bool TryParse(IEnumerable<string> args);
    bool TryParseFile(string argsFile = "app.args");

    string this[string key, string defaultValue] { get; }
}