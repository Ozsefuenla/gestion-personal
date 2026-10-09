using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class ChangePinCommandValidator : AbstractValidator<ChangePinCommand>
{
    public ChangePinCommandValidator()
    {
        RuleFor(x => x.CurrentPin)
            .NotEmpty().WithMessage("Formato de PIN incorrecto.")
            .Matches(@"^\d{4}$").WithMessage("Formato de PIN incorrecto.");

        RuleFor(x => x.NewPin)
            .NotEmpty().WithMessage("Formato de PIN incorrecto.")
            .Matches(@"^\d{4}$").WithMessage("Formato de PIN incorrecto.");
    }
}
