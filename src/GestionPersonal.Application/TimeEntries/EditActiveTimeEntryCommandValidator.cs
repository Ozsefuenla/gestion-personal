using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EditActiveTimeEntryCommandValidator : AbstractValidator<EditActiveTimeEntryCommand>
{
    public EditActiveTimeEntryCommandValidator()
    {
        RuleFor(x => x.Start)
            .NotEmpty().WithMessage("La hora de inicio es obligatoria.");
    }
}
