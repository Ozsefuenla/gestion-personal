using GestionPersonal.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionPersonal.Web.Services;

public sealed record TimeControlExportWorker(
    string FullName,
    int Year,
    int Month,
    TimeSpan DailyTarget,
    MonthlySummary Summary);

public sealed class TimeControlPdfExporter
{
    public byte[] Build(IReadOnlyList<TimeControlExportWorker> workers)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Spacing(16);

                    foreach (var worker in workers)
                    {
                        column.Item().Column(section =>
                        {
                            section.Spacing(6);

                            section.Item().Text($"Trabajador: {worker.FullName}").FontSize(14).SemiBold();
                            section.Item().Text($"Mes: {TimeFormatting.MonthName(worker.Month)} {worker.Year}").FontSize(11);

                            section.Item().Table(table => BuildTable(table, worker));

                            var totalWorked = worker.Summary.Days.Aggregate(TimeSpan.Zero, (total, day) => total + day.WorkedTime);
                            var totalTarget = worker.DailyTarget * CountWorkingDays(worker);

                            section.Item().Text(text =>
                            {
                                text.Span("Total trabajadas: ").SemiBold();
                                text.Span(TimeFormatting.FormatHours(totalWorked)).SemiBold();
                                text.Span(" · Diferencia: ").SemiBold();
                                text.Span(TimeFormatting.FormatDelta(totalWorked, totalTarget)).SemiBold();
                            });
                        });
                    }
                });
            });
        }).GeneratePdf();
    }

    private static void BuildTable(TableDescriptor table, TimeControlExportWorker worker)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(80);
            columns.RelativeColumn(3);
            columns.RelativeColumn(2);
            columns.RelativeColumn(2);
        });

        table.Header(header =>
        {
            header.Cell().Element(CellStyle).Text("Fecha").SemiBold();
            header.Cell().Element(CellStyle).Text("Previstas").SemiBold();
            header.Cell().Element(CellStyle).Text("Trabajadas").SemiBold();
            header.Cell().Element(CellStyle).AlignRight().Text("Diferencia").SemiBold();
        });

        for (var day = 1; day <= DateTime.DaysInMonth(worker.Year, worker.Month); day++)
        {
            var date = new DateTime(worker.Year, worker.Month, day);
            var isWorkingDay = date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
            var summary = worker.Summary.Days.FirstOrDefault(d => d.Day == day);

            string previstasText;
            string trabajadasText;
            string diferenciaText;

            if (summary is { IsAbsence: true })
            {
                previstasText = $"Ausencia - {AbsenceLabel(summary.AbsenceType)}";
                trabajadasText = "—";
                diferenciaText = "—";
            }
            else if (!isWorkingDay)
            {
                previstasText = "No laborable";
                trabajadasText = "—";
                diferenciaText = "—";
            }
            else if (summary is not { HasEntries: true })
            {
                previstasText = "Sin datos";
                trabajadasText = "—";
                diferenciaText = "—";
            }
            else
            {
                previstasText = TimeFormatting.FormatHours(worker.DailyTarget);
                trabajadasText = TimeFormatting.FormatHours(summary.WorkedTime);
                diferenciaText = TimeFormatting.FormatDelta(summary.WorkedTime, worker.DailyTarget);
            }

            table.Cell().Element(CellStyle).Text(date.ToString("dd/MM/yyyy"));
            table.Cell().Element(CellStyle).Text(previstasText);
            table.Cell().Element(CellStyle).Text(trabajadasText);
            table.Cell().Element(CellStyle).AlignRight().Text(diferenciaText);
        }
    }

    private static int CountWorkingDays(TimeControlExportWorker worker)
    {
        var first = new DateTime(worker.Year, worker.Month, 1);
        var today = DateTime.Today;

        if (today < first)
        {
            return 0;
        }

        var last = first.AddMonths(1).AddDays(-1);
        var end = today > last ? last : today;

        return Enumerable.Range(0, (end - first).Days + 1)
            .Select(d => first.AddDays(d))
            .Count(d => d.DayOfWeek != DayOfWeek.Saturday
                        && d.DayOfWeek != DayOfWeek.Sunday
                        && !worker.Summary.Days.Any(x => x.Day == d.Day && x.IsAbsence));
    }

    private static string AbsenceLabel(string? type) => type switch
    {
        "vacations" => "Vacaciones",
        "sick-leave" => "Enfermedad",
        _ => "Otro"
    };

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(4);
    }
}
