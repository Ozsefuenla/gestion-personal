namespace GestionPersonal.Infrastructure.Persistence.Postgres;

internal static class PgConvert
{
    public static DateTimeOffset ToOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public static DateTimeOffset? ToOffset(DateTime? value) =>
        value.HasValue ? ToOffset(value.Value) : null;

    public static DateTimeOffset ToUtc(DateTimeOffset value) =>
        value.ToUniversalTime();

    public static DateTimeOffset? ToUtc(DateTimeOffset? value) =>
        value?.ToUniversalTime();
}
