namespace GestionPersonal.Web.Models;

public sealed record QuickClockResult(
    bool Success,
    string Message,
    string? CurrentStatus,
    IReadOnlyList<string> AvailableActions);
