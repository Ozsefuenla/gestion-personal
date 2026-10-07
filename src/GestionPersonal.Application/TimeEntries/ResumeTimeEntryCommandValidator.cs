using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class ResumeTimeEntryCommandValidator : AbstractValidator<ResumeTimeEntryCommand>
{
    public ResumeTimeEntryCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");
    }
}
