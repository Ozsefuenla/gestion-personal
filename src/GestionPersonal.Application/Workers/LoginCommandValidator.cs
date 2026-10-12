using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("El trabajador es obligatorio.");

        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("Formato de PIN incorrecto.")
            .Matches(@"^\d{4}$").WithMessage("Formato de PIN incorrecto.");
    }
}
