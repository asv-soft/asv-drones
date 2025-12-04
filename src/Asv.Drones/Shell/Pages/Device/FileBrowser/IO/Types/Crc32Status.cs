namespace Asv.Drones;

public enum Crc32Status
{
    /// <summary>
    /// Basic status.
    /// </summary>
    Default,

    /// <summary>
    /// Indicates that the CRC32 check was successful.
    /// </summary>
    Correct,

    /// <summary>
    /// Indicates that the CRC32 check was unsuccessful.
    /// </summary>
    Incorrect,
}
