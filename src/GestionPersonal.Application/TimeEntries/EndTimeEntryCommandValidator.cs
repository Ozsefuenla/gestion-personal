using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EndTimeEntryCommandValidator : AbstractValidator<EndTimeEntryCommand>
{
    public EndTimeEntryCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");
    }
}
