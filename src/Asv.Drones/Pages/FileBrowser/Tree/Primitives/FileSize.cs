using System;

namespace Asv.Drones;

public readonly struct FileSize(long size) : IEquatable<FileSize>
{
    private const long OneKilobyte = 1024;
    private const long OneMegabyte = OneKilobyte * 1024;
    private const long OneGigabyte = OneMegabyte * 1024;
    private const long OneTerabyte = OneGigabyte * 1024;

    private long Size => size;

    public override string ToString()
    {
        string unit;
        double value;

        switch (size)
        {
            case < OneKilobyte:
                unit = RS.Unit_Byte_Abbreviation;
                value = size;
                break;
            case < OneMegabyte:
                unit = RS.Unit_Kilobyte_Abbreviation;
                value = size / (double)OneKilobyte;
                break;
            case < OneGigabyte:
                unit = RS.Unit_Megabyte_Abbreviation;
                value = size / (double)OneMegabyte;
                break;
            case < OneTerabyte:
                unit = RS.Unit_Gigabyte_Abbreviation;
                value = size / (double)OneGigabyte;
                break;
            default:
                unit = RS.Unit_Terabyte_Abbreviation;
                value = size / (double)OneTerabyte;
                break;
        }

        return $"{value:0.##} {unit}";
    }

    public int CompareTo(FileSize other)
    {
        return Size.CompareTo(other.Size);
    }

    public bool Equals(FileSize other)
    {
        return Size.Equals(other.Size);
    }

    public override bool Equals(object? obj)
    {
        return obj is FileSize fileSize && Equals(fileSize);
    }

    public static bool operator ==(FileSize left, FileSize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FileSize left, FileSize right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Size.GetHashCode();
    }
}
