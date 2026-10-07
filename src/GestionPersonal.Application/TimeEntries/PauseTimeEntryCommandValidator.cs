using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class PauseTimeEntryCommandValidator : AbstractValidator<PauseTimeEntryCommand>
{
    public PauseTimeEntryCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");
    }
}
