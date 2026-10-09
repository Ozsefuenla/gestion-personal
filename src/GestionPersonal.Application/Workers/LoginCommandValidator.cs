using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("Formato de PIN incorrecto.")
            .Matches(@"^\d{4}$").WithMessage("Formato de PIN incorrecto.");
    }
}
