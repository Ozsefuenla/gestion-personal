namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class TelemetryOptions
{
    public int ChannelCapacity { get; init; } = 10_000;

    public int BatchSize { get; init; } = 100;

    public int RetentionDays { get; init; } = 90;

    public static TelemetryOptions FromConfiguration(Func<string, string?> read)
    {
        return new TelemetryOptions
        {
            ChannelCapacity = ReadInt(read, "Telemetry:ChannelCapacity", 10_000),
            BatchSize = ReadInt(read, "Telemetry:BatchSize", 100),
            RetentionDays = ReadInt(read, "Telemetry:RetentionDays", 90)
        };
    }

    private static int ReadInt(Func<string, string?> read, string key, int fallback) =>
        int.TryParse(read(key), out var parsed) && parsed > 0 ? parsed : fallback;
}
