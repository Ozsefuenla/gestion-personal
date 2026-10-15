using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class AddManualTimeEntryCommandValidator : AbstractValidator<AddManualTimeEntryCommand>
{
    private static readonly string[] AllowedTypes = ["start", "pause", "end"];

    public AddManualTimeEntryCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de marcaje es obligatorio.")
            .Must(BeValidType).WithMessage("El tipo de marcaje no es válido.");

        RuleFor(x => x.Date)
            .NotNull().WithMessage("La fecha es obligatoria.")
            .Must(BeNotInFuture).WithMessage("No se pueden añadir marcajes en fechas posteriores a hoy.");

        RuleFor(x => x.StartTime)
            .NotNull().WithMessage("La hora de inicio es obligatoria.")
            .When(x => IsType(x, "start") || IsType(x, "pause"));

        RuleFor(x => x.EndTime)
            .NotNull().WithMessage("La hora de final es obligatoria.")
            .When(x => IsType(x, "end"));

        RuleFor(x => x.EndTime)
            .Null().WithMessage("El tipo Iniciar no admite hora de final.")
            .When(x => IsType(x, "start"));

        RuleFor(x => x.StartTime)
            .Null().WithMessage("El tipo Finalizar no admite hora de inicio.")
            .When(x => IsType(x, "end"));

        RuleFor(x => x.EndTime)
            .Must((command, endTime) => endTime.HasValue && command.StartTime.HasValue && endTime.Value > command.StartTime.Value)
            .WithMessage("La hora de final debe ser mayor a la hora de inicio.")
            .When(x => IsType(x, "pause") && x.StartTime.HasValue && x.EndTime.HasValue);
    }

    private static bool IsType(AddManualTimeEntryCommand command, string type) =>
        string.Equals(command.Type?.Trim(), type, StringComparison.OrdinalIgnoreCase);

    private static bool BeValidType(string? type) =>
        type is not null && AllowedTypes.Contains(type.Trim().ToLowerInvariant());

    private static bool BeNotInFuture(DateOnly? date)
    {
        if (date is null)
        {
            return false;
        }

        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, MadridTimeZone.Instance);
        return date.Value <= DateOnly.FromDateTime(localNow.Date);
    }
}
