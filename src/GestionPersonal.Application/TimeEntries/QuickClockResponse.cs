namespace GestionPersonal.Application.TimeEntries;

public sealed record QuickClockResponse(
    bool Success,
    string Message,
    string? CurrentStatus,
    IReadOnlyList<string> AvailableActions,
    DateOnly? StaleEntryDate = null,
    bool MustChangePin = false);
