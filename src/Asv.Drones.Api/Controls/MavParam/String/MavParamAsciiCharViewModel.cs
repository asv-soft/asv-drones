using System.Buffers.Binary;
using System.Text;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamAsciiCharViewModel : MavParamTextBoxViewModel
{
    public MavParamAsciiCharViewModel(
        MavParamInfo param,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory)
        : base(param, update, initReadCallback, loggerFactory)
    {
        
    }
    
    protected override string ValueToText(ValueType remoteValue)
    {
        Span<byte> raw = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(raw, (int)remoteValue);
        var sb = new StringBuilder(4);
        foreach (char c in raw)
        {
            if (!char.IsControl(c) && !char.IsWhiteSpace(c) && char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    protected override Exception? TextToValue(string valueAsString, out ValueType value)
    {

        if (string.IsNullOrWhiteSpace(valueAsString))
        {
            value = 0;
            return null;
        }


        var filtered = string.Concat(valueAsString.Where(c => 
            !char.IsControl(c) && 
            !char.IsWhiteSpace(c) && 
            char.IsLetterOrDigit(c)));

        if (filtered.Length == 0)
        {
            value = 0;
            return null;
        }

        if (filtered.Length > 4)
        {
            filtered = filtered.Substring(0, 4);
        }

        Span<byte> buffer = stackalloc byte[4];
        Encoding.ASCII.GetBytes(filtered, buffer);
        value = BinaryPrimitives.ReadInt32BigEndian(buffer);
        return null;
    }
}