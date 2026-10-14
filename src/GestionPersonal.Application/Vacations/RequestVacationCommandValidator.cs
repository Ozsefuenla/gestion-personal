using FluentValidation;

namespace GestionPersonal.Application.Vacations;

public sealed class RequestVacationCommandValidator : AbstractValidator<RequestVacationCommand>
{
    private static readonly string[] AllowedTypes = ["vacations", "sick-leave", "other"];

    public RequestVacationCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("El trabajador es obligatorio.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de ausencia es obligatorio.")
            .Must(IsAllowedType).WithMessage("El tipo de ausencia no es válido.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha de fin no puede ser anterior a la de inicio.");
    }

    private static bool IsAllowedType(string? type) =>
        type is not null && AllowedTypes.Contains(type.Trim().ToLowerInvariant());
}
