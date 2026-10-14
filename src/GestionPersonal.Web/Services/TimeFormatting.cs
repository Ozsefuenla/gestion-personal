using MudBlazor;

namespace GestionPersonal.Web.Services;

public static class TimeFormatting
{
    private static readonly string[] MonthNames =
    [
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    ];

    public static string MonthName(int month) => MonthNames[month];

    public static string FormatHours(TimeSpan value) =>
        value.Minutes == 0 ? $"{(int)value.TotalHours} h" : $"{(int)value.TotalHours} h {value.Minutes} m";

    public static string FormatDelta(TimeSpan worked, TimeSpan target)
    {
        var delta = (int)Math.Round(worked.TotalMinutes - target.TotalMinutes);
        return delta == 0 ? "0 min" : delta > 0 ? $"+{delta} min" : $"{delta} min";
    }

    public static Color DeltaColor(TimeSpan worked, TimeSpan target) =>
        worked.TotalMinutes >= target.TotalMinutes ? Color.Success : Color.Error;
}
