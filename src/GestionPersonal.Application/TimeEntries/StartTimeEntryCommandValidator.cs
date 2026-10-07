using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class StartTimeEntryCommandValidator : AbstractValidator<StartTimeEntryCommand>
{
    public StartTimeEntryCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");
    }
}
