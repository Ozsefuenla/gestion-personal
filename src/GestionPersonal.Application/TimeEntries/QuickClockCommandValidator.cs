using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class QuickClockCommandValidator : AbstractValidator<QuickClockCommand>
{
    private static readonly string[] AllowedActions = ["start", "pause", "end"];

    public QuickClockCommandValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("Formato de PIN incorrecto.")
            .Matches(@"^\d{4}$").WithMessage("Formato de PIN incorrecto.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("La acción es obligatoria.")
            .Must(action => AllowedActions.Contains(action)).WithMessage("La acción no es válida.");
    }
}
